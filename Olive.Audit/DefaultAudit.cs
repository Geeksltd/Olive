using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Olive.Entities;

namespace Olive.Audit
{
    public class DefaultAudit : IAudit
    {
        readonly IConfiguration Config;

        public DefaultAudit(IConfiguration configuration) => Config = configuration;

        public virtual Task LogInsert(IEntity entity)
        {
            if (entity is IAuditEvent) return Task.CompletedTask;

            var log = LogEventsAttribute.For(entity);

            if (!log.InsertAction) return Task.CompletedTask;

            var data = log.InsertData ? EntityProcessor.GetDataXml(entity) : string.Empty;

            return Log("Insert", data, entity);
        }

        public virtual async Task LogUpdate(IEntity entity)
        {
            if (entity is IAuditEvent) return;

            var log = LogEventsAttribute.For(entity);
            if (!log.UpdateAction) return;

            var data = await EntityProcessor.GetChangesXml(entity);
            if (data.IsEmpty()) return;

            if (!log.UpdateData) data = string.Empty;

            await Log("Update", data, entity);
        }

        public virtual async Task LogDelete(IEntity entity)
        {
            if (entity is IAuditEvent) return;

            var log = LogEventsAttribute.For(entity);
            if (!log.DeleteAction) return;

            var data = string.Empty;

            if (log.DeleteData)
            {
                var changes = Context.Current.Database()
                  .GetProvider(entity.GetType()).GetUpdatedValues(entity, null);

                data = EntityProcessor.ToChangeXml(changes);
            }

            await Log("Delete", data, entity);
        }

        public virtual Task Log(IAuditEvent auditEvent)
            => Context.Current.ServiceProvider.GetServices<IAuditLogger>().AwaitAll(x => x.Log(auditEvent));

        public virtual Task Log(string @event, string data, string group = null)
            => Log(@event, data, null, group);

        public virtual Task Log(string @event, string data, IEntity item, string group = null)
        {
            var userId = Context.Current.GetOptionalService<IContextUserProvider>()?.GetUser()?.GetId();
            var userIp = Context.Current.GetOptionalService<IContextUserProvider>()?.GetUserIP();

            return Log(@event, data, item, userId, userIp, group);
        }

        /// <summary>
        /// Logs the specified event as a record in the ApplicationEvents database table.
        /// </summary>
        /// <param name="event">The event title.</param>
        /// <param name="data">The details of the event.</param>
        /// <param name="item">The record for which this event is being logged (optional).</param>
        /// <param name="userId">The ID of the user involved in this event (optional). If not specified, the current ASP.NET context user will be used.</param>
        /// <param name="userIp">The IP address of the user involved in this event (optional). If not specified, the IP address of the current Http context (if available) will be used.</param>
        /// <param name="group">The logs can be grouped by this tag (optional).</param>
        public virtual Task Log(string @event, string data, IEntity item, string userId, string userIp, string group = null)
        {
            if (@event.IsEmpty()) throw new ArgumentNullException(nameof(@event));

            var tasks = new List<Task>();

            foreach (var provider in Context.Current.GetServices<IAuditLogger>())
            {
                var log = provider.CreateInstance();
                if (log == null) continue;

                log.ItemData = data;
                log.Event = @event;
                log.UserId = userId;
                log.UserIp = userIp;
                log.ItemGroup = group;

                if (item != null)
                {
                    log.ItemType = item.GetType().FullName;
                    log.ItemId = item.GetId().ToString();
                }

                tasks.Add(provider.Log(log));
            }

            return Task.WhenAll(tasks);
        }
    }
}