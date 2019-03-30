using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    partial class ExposedType<TDomain>
    {
        public class DataDependency
        {
            public Type EntityType { get; set; }
            public Func<IEntity, Task<IEnumerable<TDomain>>> Function { get; set; }

            internal DataDependency(Type entityType, Func<IEntity, Task<IEnumerable<TDomain>>> function)
            {
                EntityType = entityType;
                Function = function;
            }
        }

        public class DataDependency<TTriggerer> : DataDependency
            where TTriggerer : class, IEntity
        {
            internal DataDependency(Func<TTriggerer, Task<IEnumerable<TDomain>>> reverseDependencyFunction)
                : base(typeof(TTriggerer), async x => await reverseDependencyFunction(x as TTriggerer))
            {
            }
        }
    }
}