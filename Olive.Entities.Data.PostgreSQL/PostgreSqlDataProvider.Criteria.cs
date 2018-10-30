namespace Olive.Entities.Data
{
    class PostgreSqlCriterionGenerator : DatabaseCriterionSqlGenerator
    {
        public PostgreSqlCriterionGenerator(DatabaseQuery query) : base(query) { }

        protected override string ToSafeId(string id) => $"\"{id}\"";

        protected override string UnescapeId(string id) => id.Trim('\"');
    }
}