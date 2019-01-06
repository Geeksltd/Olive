using System;

namespace Olive.Entities.Data
{
    public abstract class DatabaseCriterionSqlGenerator
    {
        DatabaseQuery Query;
        Type EntityType;

        protected DatabaseCriterionSqlGenerator(DatabaseQuery query)
        {
            Query = query;
            EntityType = query.EntityType;
        }

        public string Generate(ICriterion criterion)
        {
            if (criterion == null) return "(1 = 1)";
            else return criterion.ToSql(new SqlConversionContext
            {
                Query = Query,
                Type = EntityType,
                ToSafeId = ToSafeId
            });
        }

        protected abstract string ToSafeId(string id);

        protected abstract string UnescapeId(string id);
    }
}