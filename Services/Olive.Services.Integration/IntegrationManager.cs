using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olive.Entities;

namespace Olive.Services.Integration
{
    /// <summary>
    /// Provides services for integration services.
    /// </summary>
    public class IntegrationManager
    {
        internal const int WaitInterval = 50;

        static Type IntegrationQueueItemType;

        /// <summary>
        /// Key = TRequest|TResponse
        /// Value = TService
        /// </summary>
        internal static Dictionary<string, Type> IntegrationServices = new Dictionary<string, Type>();

        static AsyncLock PickLock = new AsyncLock();

        /// <summary>
        /// Will find a Service Registered to process the item.
        /// </summary>
        public static async Task<IIntegrationQueueItem> Process(IIntegrationQueueItem item)
        {
            if (item.ResponseDate.HasValue) return item; // Already processed:             

            #region Pick the item

            Type serviceType;

            using (await PickLock.Lock())
            {
                if (item.DatePicked.HasValue)
                    // Already picked, let the other thread finish its job:
                    return null;

                if (!item.IsNew) item = (IIntegrationQueueItem)item.Clone();

                serviceType = IntegrationServices.GetOrDefault(item.IntegrationService);
                if (serviceType == null) return null;

                item.DatePicked = LocalTime.Now;
                await Entity.Database.Save(item);
                item = (IIntegrationQueueItem)(await Entity.Database.Reload(item)).Clone();
            }

            // TOOD: This is not thread safe in multi-server (web farm) scenarios.
            // To make it completely safe, we need a single StoredProc or SQL command that will do both at the same time, with a lock at the DB level.
            #endregion

            var service = serviceType.CreateInstance();

            var serviceInterface = serviceType.GetInterfaces().Single(x => x.Name.Contains("IServiceImplementor"));

            var typeOfRequest = serviceInterface.GetGenericArguments().First();

            var request = JsonConvert.DeserializeObject(item.Request, typeOfRequest);

            var method = serviceType.GetMethod("GetResponse", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            try
            {
                var response = method.Invoke(service, new[] { request });
                item.Response = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                item.ErrorResponse = ex.ToString();
            }

            item.ResponseDate = LocalTime.Now;
            await Entity.Database.Save(item);
            return item;
        }

        public static async Task<IIntegrationQueueItem> Process(string id) => await Process(await Entity.Database.Get<IIntegrationQueueItem>(id));

        /// <summary>
        ///  Uses the right Integration Service to process the outstanding items in all queues.
        ///  This should be called as an Automated Task in the application.
        /// </summary>
        public static async Task<IEnumerable<IIntegrationQueueItem>> ProcessOutstandingItems()
        {
            var result = new List<IIntegrationQueueItem>();

            foreach (var serviceType in IntegrationServices.Keys)
                result.AddRange(await ProcessOutstandingItems(serviceType));

            return result;
        }

        /// <summary>
        ///  Uses the right Integration Service to process the next item in that queue.
        /// </summary>
        public static async Task<IEnumerable<IIntegrationQueueItem>> ProcessOutstandingItems(string serviceName)
        {
            var serviceType = IntegrationServices.GetOrDefault(serviceName);
            if (serviceType == null) throw new Exception("Integration service not registered:" + serviceName);

            var result = new List<IIntegrationQueueItem>();

            foreach (var item in await Entity.Database.GetList<IIntegrationQueueItem>(x => x.IntegrationService == serviceName && x.DatePicked == null))
            {
                await Entity.Database.Update(item, x => x.DatePicked = LocalTime.Now);
                // TODO: Use T-SQL to fetch an item and return it too.

                await Process(item);
                result.Add(item);
            }

            return result;
        }

        static void DiscoverQueueType()
        {
            if (IntegrationQueueItemType != null) return;

            try
            {
                IntegrationQueueItemType = typeof(IIntegrationQueueItem).FindImplementerClasses().Single(x => !x.IsAbstract);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot find the correct implementation type for IIntegrationQueueItem.", ex);
            }
        }

        public static string GetServiceKey(Type serviceType)
        {
            var serviceBaseType = serviceType.WithAllParents()
                .FirstOrDefault(x => x.IsGenericType && x.Name.StartsWith("IntegrationService"));

            if (serviceBaseType == null)
                throw new Exception("TService should inherit from IntegrationService<TRequest, TResponse>");

            var types = serviceBaseType.GetGenericArguments();
            return GetServiceKey(types.First(), types.Last());
        }

        public static string GetServiceKey<TService>() where TService : IntegrationService => GetServiceKey(typeof(TService));

        static string GetServiceKey(Type request, Type response) => request.Name + "|" + response.Name;

        /// <summary>
        /// Registers an integration service.
        /// </summary>
        public static void Register<TRequest, TResponse, TService>() where TService : IServiceImplementor<TRequest, TResponse>
        {
            DiscoverQueueType();

            var key = GetServiceKey(typeof(TRequest), typeof(TResponse));

            IntegrationServices.Add(key, typeof(TService));
        }

        /// <summary>
        /// Inserts a queu item to call this service and waits until the item is processed.
        /// Then it will return the response.
        /// </summary>
        public static async Task<TResponse> Request<TRequest, TResponse>(TRequest request)
        {
            var item = await RequestAsync<TRequest, TResponse>(request);

            return await AwaitResponse<TResponse>(item);
        }

        /// <summary>
        /// Inserts a request in the queue and immediately returns without waiting for a response.
        /// It will return the token string for this request, that can be queried later on for a response (using Await Response).
        /// </summary>
        public static async Task<string> RequestAsync<TRequest, TResponse>(TRequest request)
        {
            DiscoverQueueType();

            var item = IntegrationQueueItemType.CreateInstance<IIntegrationQueueItem>();
            item.IntegrationService = GetServiceKey(request.GetType(), typeof(TResponse));
            // item.Request = new JavaScriptSerializer().Serialize(request);
            item.Request = JsonConvert.SerializeObject(request);

            item.RequestDate = LocalTime.Now;
            await Entity.Database.Save(item);

            return item.ID.ToString();
        }

        /// <summary>
        /// It will wait until a response is provided by another thread to the integration queue item specified by its token.
        /// </summary>
        public static async Task<TResponse> AwaitResponse<TResponse>(string requestToken, int waitIntervals = WaitInterval)
        {
            while (true)
            {
                // var item = Database.Get<IIntegrationQueueItem>(requestToken);

                var processedItem = await Process(requestToken);

                if (processedItem != null)
                {
                    // var result = new JavaScriptSerializer().Deserialize(processedItem.Response, typeof(TResponse));
                    var result = JsonConvert.DeserializeObject(processedItem.Response, typeof(TResponse));
                    return (TResponse)result;
                }

                await Task.Delay(waitIntervals);
            }
        }

        /// <summary>
        /// Injects an asyncronous waiter which will inject the provided response for one potential future request.
        /// It will check every 5 milliseconds to see if a request item is inserted in the queue, and in that case respond to it.
        /// </summary>
        public static async Task InjectResponse<TRequest, TResponse>(TResponse injectedResponse)
        {
            var service = GetServiceKey(typeof(TRequest), typeof(TResponse));

            // Get queue item:
            while (true)
            {
                var item = await Entity.Database.Of<IIntegrationQueueItem>().OrderBy(x => x.RequestDate)
                    .Where(x => x.IntegrationService == service && x.ResponseDate == null)
                    .FirstOrDefault();

                if (item != null)
                {
                    item = item.Clone() as IIntegrationQueueItem;

                    // item.Response = new JavaScriptSerializer().Serialize(injectedResponse);
                    item.Response = JsonConvert.SerializeObject(injectedResponse);
                    item.ResponseDate = LocalTime.Now;

                    await Entity.Database.Save(item);

                    return;
                }

                await Task.Delay(5);
            }
        }
    }
}