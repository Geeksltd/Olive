using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Olive.Entities;

namespace Olive
{
    public class DatabaseFilters<T> : List<Criterion> where T : IEntity
    {
        public void Add(Expression<Func<T, bool>> criterion) => Add(Criterion.From<T>(criterion));
    }
}