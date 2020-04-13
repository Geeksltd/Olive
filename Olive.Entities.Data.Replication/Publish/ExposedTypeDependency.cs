using Olive;
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
        List<DataDependency> Dependencies = new List<DataDependency>();

        Type[] DependenciesEnitityTypes;


        public DataDependency<TTriggerer> AddNestedChildDependency<TTriggerer>(
           Func<TTriggerer, Task<IEnumerable<IEntity>>> childDependents)
           where TTriggerer : class, IEntity
        {
            var dep = new DataDependency<TTriggerer>(
                async child =>
                {
                    var deps = await childDependents(child);
                    if(deps == null) return null;
                    foreach (var x in deps) 
                        if(x != null) await RaiseSaveEventFor(x);

                    return null;
                });

            Dependencies.Add(dep);
            return dep;
        }

        /// <summary>
        /// Marks this type as dependant directly to an associated master entity type.
        /// Use this for many-to-one associations.
        /// </summary>
        public DataDependency AddDependency<TTriggerer>(Expression<Func<TDomain, TTriggerer>> dependency)
          where TTriggerer : class, IEntity
        {
            var dep = new DataDependency<TTriggerer>(p => FromDB<TDomain>(p.GetId(), dependency.GetProperty()));
            Dependencies.Add(dep);
            return dep;
        }

        /// <summary>
        /// Define the reverse side of a master-detail relationship, where the details' data is exposed via this master object.
        /// Use this for exposed one-to-many associations to declare the child dependency.
        /// </summary>
        public DataDependency<TChildEntity> AddChildDependency<TChildEntity>(Expression<Func<TChildEntity, TDomain>> dependency)
           where TChildEntity : class, IEntity
        {
            var function = dependency.Compile();
            var dep = new DataDependency<TChildEntity>(p => Task.FromResult<IEnumerable<TDomain>>(new[] { function?.Invoke(p) }));
            Dependencies.Add(dep);
            return dep;
        }

        static async Task<IEnumerable<T>> FromDB<T>(object entityId, PropertyInfo prop)
        {
            var result = await Context.Current.Database().Of(typeof(T)).Where(new Criterion(prop.Name, entityId)).GetList();
            return result.Cast<T>();
        }

        static async Task RaiseSaveEventFor(IEntity entity)
        {
            var mode = entity.IsNew ? SaveMode.Insert : SaveMode.Update;
            await GlobalEntityEvents.OnInstanceSaved(new GlobalSaveEventArgs(entity, mode));
        }
    }
}