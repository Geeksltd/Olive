using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Olive.Audit;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.EF.Replication
{
	public class EndpointSubscriber
	{
		static TimeSpan TimeSyncTolerance = 1.Seconds();
		static FieldInfo IsImmutableField;
		static PropertyInfo IsNewProperty;
		public Type DomainType { get; set; }
		public DestinationEndpoint Endpoint { get; }
		public string Schema { get; }
		public string Table { get; }
		string QualifiedTable => Schema.WithSuffix(".") + Table;
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
			Schema = domainType.GetCustomAttribute<TableAttribute>(inherit: false)?.Schema
				?? SchemaAttribute.GetSchema(domainType);
			Table = endpoint.GetTableName(domainType);
			Log = Olive.Log.For(this);
		}

		internal async Task<bool> IsTableEmpty()
		{
			var count = await DbContext.Database
				.SqlQueryRaw<int>($"SELECT COUNT(*) AS [Value] FROM {QualifiedTable}")
				.SingleAsync();

			return count <= 0;
		}

		public async Task RefreshData()
		{
			Log.Info("Data table " + Table + " is empty. Adding a refresh message.");

			var request = new RefreshMessage { TypeName = DomainType.Namespace + "." + DomainType.Name, RequestUtc = DateTime.UtcNow };
			RefreshRequestUtc = request.RequestUtc;
			await Endpoint.RefreshQueue.Publish(request);

			Log.Info("Refresh message published to queue.");

			try { await Context.Current.Database().Refresh(); }
			catch (Exception ex) { Log.Debug(ex, "Database cache refresh skipped after replication refresh request."); }
		}

		internal async Task Import(ReplicateDataMessage message)
		{
			if (message.CreationUtc < RefreshRequestUtc?.Subtract(TimeSyncTolerance))
			{
				// Ignore this. We will receive a full table after this anyway.
				Log.Debug("Ignored importing expired ReplicateDataMessage " + message.DeduplicationId + " because it's older the last refresh request.");
				return;
			}

			if (message.IsClearSignal)
			{
				Log.Info($"Received Clear Signal for {message.TypeFullName}");

				await DbContext.Database.ExecuteSqlRawAsync($"delete from {QualifiedTable}");

				return;
			}

			Log.Trace($"Beginning to import ReplicateDataMessage for {message.TypeFullName}:\n{message.Entity}\n\n");

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
						var tracked = await DbContext.FindAsync(DomainType, entity.GetId()) as IEntity ?? entity;
						DbContext.Remove(tracked);
						await DbContext.SaveChangesAsync();
						await Endpoint.OnDeleted(message, entity);
					}
				}
				else
				{
					var mode = entity.IsNew ? SaveMode.Insert : SaveMode.Update;

					if (mode == SaveMode.Update && !await IsActuallyChanged(entity)) return;

					if (!await Endpoint.OnSaving(message, entity, mode)) return;

					var bypass = SaveBehaviour.BypassAll;

					if (Config.Get("Database:Audit:EnableForEndpointDataReplication", defaultValue: false))
						bypass &= ~SaveBehaviour.BypassLogging;

					await LogReplicationSaveIfEnabled(entity, mode, bypass);

					if (entity.IsNew)
						DbContext.Add(entity);
					else
						DbContext.Update(entity);

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

		async Task<bool> IsActuallyChanged(IEntity entityBeingSaved)
		{
			try
			{
				var database = Context.Current.Database();
				var original = await database.Get(entityBeingSaved.GetId(), entityBeingSaved.GetType());
				return database.GetProvider(entityBeingSaved.GetType()).GetUpdatedValues(original, entityBeingSaved).Any();
			}
			catch (Exception ex)
			{
				Log.Debug(ex, "Skipping unchanged check for replication import.");
				return true;
			}
		}

		async Task LogReplicationSaveIfEnabled(IEntity entity, SaveMode mode, SaveBehaviour bypass)
		{
			if ((bypass & SaveBehaviour.BypassLogging) != 0) return;

			var audit = Context.Current.GetOptionalService<IAudit>();
			if (audit == null) return;

			if (mode == SaveMode.Insert)
				await audit.LogInsert(entity);
			else
				await audit.LogUpdate(entity);
		}

		async Task<IEntity> Deserialize(string serialized)
		{
			var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(serialized);
			var existing = await DbContext.FindAsync(DomainType, data["ID"].To<Guid>()) as IEntity;

			IEntity result;
			if (existing == null)
				result = DomainType.CreateInstance<IEntity>();
			else
			{
				result = existing.Clone();
				new EntityServices().SetSaved(result);
			}

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