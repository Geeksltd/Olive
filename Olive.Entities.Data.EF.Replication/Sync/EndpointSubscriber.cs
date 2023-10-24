using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Olive.Entities.SoftDeleteAttribute;

namespace Olive.Entities.EF.Replication
{
	public class EndpointSubscriber
	{
		static TimeSpan TimeSyncTolerance = 1.Seconds();
		static FieldInfo IsImmutableField;
		static PropertyInfo IsNewProperty;
		public Type DomainType { get; set; }
		public DestinationEndpoint Endpoint { get; }
		DateTime? RefreshRequestUtc;
		ILogger Log;
		protected DbContext DbContext;

		static EndpointSubscriber()
		{
			IsImmutableField = typeof(Entity).GetField("IsImmutable", BindingFlags.NonPublic | BindingFlags.Instance)
				?? throw new Exception("Field IsImmutable was not found on type Entity.");

			IsNewProperty = typeof(Entity).GetProperty("IsNew")
				?? throw new Exception("Property IsNew was not found on type Entity.");
		}

		public EndpointSubscriber(DestinationEndpoint endpoint, Type domainType)
		{
			Endpoint = endpoint;
			DomainType = domainType;
			Log = Olive.Log.For(this);
		}

		public EndpointSubscriber(DbContext dbContext, DestinationEndpoint endpoint, Type domainType)
		{
			DbContext = dbContext;
			Endpoint = endpoint;
			DomainType = domainType;
			Log = Olive.Log.For(this);
		}

		public async Task RefreshData()
		{
			Log.Warning("Data table " + Endpoint.Table + " is empty. Adding a refresh message.");

			var request = new RefreshMessage { TypeName = DomainType.Namespace + "." + DomainType.Name, RequestUtc = DateTime.UtcNow };
			RefreshRequestUtc = request.RequestUtc;
			await Endpoint.RefreshQueue.Publish(request);

			Log.Warning("Refresh message published to queue.");
		}

		internal async Task Import(ReplicateDataMessage message)
		{
			if (message.CreationUtc < RefreshRequestUtc?.Subtract(TimeSyncTolerance))
			{
				// Ignore this. We will receive a full table after this anyway.
				Log.Info("Ignored importing expired ReplicateDataMessage " + message.DeduplicationId + " because it's older the last refresh request.");
				return;
			}

			if (message.IsClearSignal)
			{
				Log.Debug($"Received Clear Signal for {message.TypeFullName}");

				await DbContext.Database.ExecuteSqlRawAsync($"delete from {Endpoint.Schema.WithSuffix(".")}{Endpoint.Table}");

				return;
			}

			Log.Debug($"Beginning to import ReplicateDataMessage for {message.TypeFullName}:\n{message.Entity}\n\n");

			IEntity entity;

			try { entity = await Deserialize(message.Entity); }
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to deserialize a message of :> " + message.TypeFullName + " message : " + message.Entity);
				throw;
			}

			try
			{
				if (message.ToDelete)
				{
					if (await Endpoint.OnDeleting(message, entity))
					{
						DbContext.Remove(entity);
						await DbContext.SaveChangesAsync();
						await Endpoint.OnDeleted(message, entity);
					}
				}
				else
				{
					var mode = entity.IsNew ? SaveMode.Insert : SaveMode.Update;

					if (!await Endpoint.OnSaving(message, entity, mode)) return;

					var bypass = SaveBehaviour.BypassAll;

					if (Config.Get("Database:Audit:EnableForEndpointDataReplication", defaultValue: false))
						bypass &= ~SaveBehaviour.BypassLogging;

					if (entity.IsNew)
						DbContext.Add(entity);

					await DbContext.SaveChangesAsync();

					await GlobalEntityEvents.OnInstanceSaved(new GlobalSaveEventArgs(entity, mode));
					await Endpoint.OnSaved(message, entity, mode);

					Log.Debug("Saved the " + entity.GetType().FullName + " " + entity.GetId());
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to import.");
				throw;
			}
		}

		async Task<IEntity> Deserialize(string serialized)
		{
			var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(serialized);
			var result = (IEntity)await DbContext.FindAsync(DomainType, data["ID"].To<Guid>());

			if (result == null)
				result = DomainType.CreateInstance<IEntity>();
			else
				new EntityServices().SetSaved(result);

			foreach (var field in data)
			{
				var property = DomainType.GetProperty(field.Key) ?? throw new Exception($"Could not find a field named [{field.Key}] in [{DomainType.Name}]");
				if (property.PropertyType.IsA<IEntity>())
					property = property.DeclaringType.GetProperty(property.Name + "Id");

				property.SetValue(result, Parse(field.Value, property.PropertyType));
			}

			return result;
		}

		object Parse(string value, Type type)
		{
			if (type.IsA<DateTime>())
				return new DateTime(value.To<long>());

			if (type.IsA<DateTime?>())
			{
				if (value.IsEmpty()) return null;
				return new DateTime(value.To<long>());
			}

			return value.To(type);
		}
	}
}