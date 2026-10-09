namespace Olive.Entities.Data
{
    class SqlCriterionGenerator : DatabaseCriterionSqlGenerator
    {
        public SqlCriterionGenerator(DatabaseQuery query) : base(query) { }

        protected override string ToSafeId(string id) => "[" + id?.Replace("]", "]]") + "]";

        protected override string UnescapeId(string id) => id.Trim('[', ']');
    }
}