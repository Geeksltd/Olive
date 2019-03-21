using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract partial class ExposedType<TDomain> 
    {
        List<DependencyInfo> Dependencies = new List<DependencyInfo>();

        Type[] DependenciesEnitityTypes;

        public static DependencyInfo<TTriggerEntityType> Dependency<TTriggerEntityType>(Func<TTriggerEntityType, Task<IEnumerable<TDomain>>> reverseDependencyFunction)
            where TTriggerEntityType : class, IEntity
        {
            return new DependencyInfo<TTriggerEntityType>(reverseDependencyFunction);
        }

        public static DependencyInfo<TTriggerEntityType> DependencyWithReraise<TTriggerEntityType>(Func<TTriggerEntityType, Task<IEnumerable<IEntity>>> getEntityToRaiseFor)
            where TTriggerEntityType : class, IEntity
        {
            return new DependencyInfo<TTriggerEntityType>(
                async triggerEntity => {
                    await (await getEntityToRaiseFor(triggerEntity)).Do(pos => RaiseSaveEventFor(pos));
                    return null;
            });
        }

        public static DependencyInfo<TTriggerEntityType> Dependency<TTriggerEntityType>(Expression<Func<TTriggerEntityType, TDomain>> reverseDependency)
            where TTriggerEntityType : class, IEntity
        {
            //return new Trigger<TTriggerEntityType>(reverseDependency);
            return new DependencyInfo<TTriggerEntityType>(
                    p => Task.FromResult<IEnumerable<TDomain>>(new[] { reverseDependency.Compile().Invoke(p as TTriggerEntityType) })
            );
        }

        public static DependencyInfo Dependency<TTriggerEntityType>(Expression<Func<TDomain, TTriggerEntityType>> dependency)
            where TTriggerEntityType : class, IEntity
        {
            return new DependencyInfo(
                        typeof(TTriggerEntityType),
                        async p => await LoadRelationFromDB(p.GetId(), dependency.GetProperty())
                    );
        }


        protected void Add(DependencyInfo trigger)
        {
            Dependencies.Add(trigger);
        }

        protected void Add(IEnumerable<DependencyInfo> triggers)
        {
            Dependencies.AddRange(triggers);
        }

        static async Task<IEnumerable<TDomain>> LoadRelationFromDB(object entityId, PropertyInfo prop)
        {
            return (await
                    Context.Current.Database().Of(typeof(TDomain)).Where(
                        new Criterion(prop.Name, entityId)
                        ).GetList()).Cast<TDomain>();
        }

        static async Task RaiseSaveEventFor(IEntity entity)
        {
            var mode = entity.IsNew ? SaveMode.Insert : SaveMode.Update;
            await GlobalEntityEvents.InstanceSaved.Raise(new GlobalSaveEventArgs(entity, mode));
        }

        public class DependencyInfo
        {
            public Type EntityType { get; set; }
            public Func<IEntity, Task<IEnumerable<TDomain>>> Function { get; set; }

            internal DependencyInfo(Type entityType, Func<IEntity, Task<IEnumerable<TDomain>>> function)
            {
                EntityType = entityType;
                Function = function;
            }
        }

        public class DependencyInfo<TTriggerEntityType> : DependencyInfo where TTriggerEntityType : class, IEntity
        {
            internal DependencyInfo(Func<TTriggerEntityType, Task<IEnumerable<TDomain>>> reverseDependencyFunction)
                : base(typeof(TTriggerEntityType), async x => await reverseDependencyFunction(x as TTriggerEntityType))
            {
            }
        }
    }
}