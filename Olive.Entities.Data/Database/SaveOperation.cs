using System;
using System.Threading.Tasks;
using System.Transactions;

namespace Olive.Entities.Data
{
    partial class Database
    {
        class SaveOperation
        {
            readonly bool InTransaction;
            readonly SaveMode Mode;
            readonly Entity Entity, ClonedFrom;
            readonly IDataProvider Provider;
            readonly SaveBehaviour Behaviour;
            readonly Database Db;
            ICache Cache => Db.Cache;

            public SaveOperation(Database db, IEntity entity, SaveBehaviour behaviour)
            {
                Entity = entity as Entity ?? throw new ArgumentNullException(nameof(entity));
                InTransaction = db.AnyOpenTransaction();
                Mode = entity.IsNew ? SaveMode.Insert : SaveMode.Update;
                ClonedFrom = Entity._ClonedFrom;
                Provider = db.GetProvider(entity);
                Behaviour = behaviour;
                Db = db;
            }

            void Validate()
            {
                EnsureMutable();
                ValidateConcurrency();

                if (IsSet(Behaviour, SaveBehaviour.BypassValidation) && !Provider.SupportValidationBypassing())
                    throw new ArgumentException(Provider.GetType().Name + " does not support bypassing validation.");
            }

            void EnsureMutable()
            {
                if (Entity.Services.IsImmutable(Entity))
                    throw new ArgumentException("An immutable record must be cloned before any modifications can be applied on it. " +
                        $"Type={Entity.GetType().FullName}, Id={Entity.GetId()}.");
            }

            void ValidateConcurrency()
            {
                if (Mode == SaveMode.Insert) return;
                if (!InTransaction) return;
                if (!ClonedFrom.IsStale) return;
                if (ReferenceEquals(ClonedFrom.UpdatedClone, Entity)) return;

                throw new InvalidOperationException("This " + Entity.GetType().Name + " instance in memory is out-of-date. " +
                    "A clone of it is already updated in the transaction. It is not allowed to update the same instance multiple times in a transaction, because then the earlier updates would be overwriten by the older state of the instance in memory. \r\n\r\n" +
                    @"BAD: 
Database.Update(myObject, x=> x.P1 = ...); // Note: this could also be nested inside another method that's called here instead.
Database.Update(myObject, x=> x.P2 = ...);

GOOD: 
Database.Update(myObject, x=> x.P1 = ...);
myObject = Database.Reload(myObject);
Database.Update(myObject, x=> x.P2 = ...);");
            }

            bool Not(SaveBehaviour behaviour) => !IsSet(Behaviour, behaviour);

            internal async Task Run()
            {
                Validate();

                if (Not(SaveBehaviour.BypassValidation))
                {
                    await Entity.Services.RaiseOnValidating(Entity, EventArgs.Empty);
                    await Entity.Validate();
                }

                if (Not(SaveBehaviour.BypassSaving))
                {
                    var savingArgs = new System.ComponentModel.CancelEventArgs();
                    await Entity.Services.RaiseOnSaving(Entity, savingArgs);

                    if (savingArgs.Cancel) { Cache.Remove(Entity); return; }
                }

                if (Not(SaveBehaviour.BypassLogging))
                    if (Mode == SaveMode.Insert) await Db.Audit.LogInsert(Entity);
                    else await Db.Audit.LogUpdate(Entity);

                await Provider.Save(Entity);
                Cache.UpdateRowVersion(Entity);

                if (Mode == SaveMode.Insert)
                {
                    Entity.Services.SetSaved(Entity);
                }
                else if (ClonedFrom != null && InTransaction)
                {
                    ClonedFrom.UpdatedClone = Entity;
                    Entity.UpdatedClone = null;
                }

                UpdateCache();

                await Db.Updated.Raise(Entity);

                if (Not(SaveBehaviour.BypassSaved))
                    await Entity.Services.RaiseOnSaved(Entity, new SaveEventArgs(Mode));

                // OnSaved event handler might have read the object again and put it in the cache, which would
                // create invalid CachedReference objects.
                Cache.Remove(Entity);
            }

            void UpdateCache()
            {
                Cache.Remove(Entity);

                if (Transaction.Current != null)
                    Transaction.Current.TransactionCompleted += (s, e) => { Cache.Remove(Entity); };

                DbTransactionScope.Root?.OnTransactionCompleted(() => Cache.Remove(Entity));
            }
        }
    }
}