using Olive.Entities.Data;

namespace Olive.Entities.ObjectDataProvider
{
    class SqlCriterionGenerator : DatabaseCriterionSqlGenerator
    {
        public SqlCriterionGenerator(DatabaseQuery query) : base(query) {/* */ }

        protected override string ToSafeId(string id) => "[" + id + "]";

        protected override string UnescapeId(string id) => id.Remove("[").Remove("]");
    }
}
