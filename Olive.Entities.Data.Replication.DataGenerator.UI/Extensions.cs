namespace Olive.Data.Replication.DataGenerator.UI
{
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Olive.Entities;
    using Olive.Entities.Replication;
    using System;
	using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

	public static class Extensions
	{
		public static void AddMockEndpoint<T>(this IServiceCollection services, string nameSpace = null) where T : DestinationEndpoint
		{
			var types = GetEndpointSubscribers(typeof(T));
            nameSpace = nameSpace ?? typeof(T).Namespace;
			foreach(var type in types)
			{
				services.AddSingleton<IDevCommand>( serviceProvider =>
					Activator.CreateInstance(typeof(ReplicationDataGeneratorUICommand), new object[] { nameSpace, type })
					as IDevCommand);

            }
        }
		public static void UseMockEndpoint(this IServiceProvider provider, DestinationEndpoint endpoint, string nameSpace = null)
		{
            nameSpace = nameSpace ?? endpoint.GetType().Namespace;
            foreach (var item in GetEndpointSubscribers(endpoint.GetType()))
			{
				var service = provider.GetServices<IDevCommand>()
				.FirstOrDefault(x => x.GetType() == typeof(ReplicationDataGeneratorUICommand) && x.Name == $"generate-{nameSpace}-{item}");
				if (service == null)
					throw new Exception($"There is no command registered for endpoint {nameSpace}-{item}");
				(service as ReplicationDataGeneratorUICommand)?.SetEndpoint(endpoint);


            }


		}
		private static IEnumerable<string> GetEndpointSubscribers(Type endpoint)
		{
			var subs = endpoint.GetProperties(BindingFlags.Public | BindingFlags.Static)
				.Where(x => x.PropertyType == typeof(EndpointSubscriber)).Select(w => w.Name);
			return subs;
		}
		internal static EndpointSubscriber GetSubscriber(this DestinationEndpoint endpoint, string typeName)
		{
            var prop = endpoint.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static)
             .FirstOrDefault(x => x.PropertyType == typeof(EndpointSubscriber) && x.Name == typeName);
			return prop.GetValue(endpoint) as EndpointSubscriber;
        }
		internal static ReplicateDataMessage ToReplicationMessage(this IEntity entity)
		{
			var message = new ReplicateDataMessage
			{
				TypeFullName = entity.GetType().FullName,
				CreationUtc = DateTime.UtcNow,
				Entity = JsonConvert.SerializeObject(entity, new JsonSerializerSettings
                {
                    ContractResolver = new ReplicationMessageSerializationResolver(),
					Formatting = Formatting.Indented,
                })
			};
			return message;
		}

	}
}
