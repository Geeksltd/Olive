namespace Olive.Entities.Data
{
    class MySqlCriterionGenerator : DatabaseCriterionSqlGenerator
    {
        public MySqlCriterionGenerator(DatabaseQuery query) : base(query) { }

        protected override string ToSafeId(string id) => "`" + id + "`";

        protected override string UnescapeId(string id) => id.Remove('`');
    }
}