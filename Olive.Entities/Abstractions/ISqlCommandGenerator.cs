namespace Olive.Entities
{
    public interface ISqlCommandGenerator
    {
        string SafeId(string objectName);

        string UnescapeId(string id);

        string GenerateSelectCommand(IDatabaseQuery iquery, string tables, string fields);

        string GenerateSort(IDatabaseQuery query);

        string GeneratePagination(IDatabaseQuery query);

        string GenerateWhere(IDatabaseQuery query);

        string GenerateUpdateCommand(IDataProviderMetaData metaData);

        string GenerateInsertCommand(IDataProviderMetaData metaData);

        string GenerateDeleteCommand(IDataProviderMetaData metaData);
    }

    /// <summary>A command generator that spells schema names its own way (PostgreSQL keeps them in lower case).</summary>
    public interface ISchemaNamingSqlCommandGenerator
    {
        string SafeSchema(string schema);
    }

    public static class SqlCommandGeneratorExtensions
    {
        /// <summary>A schema name as this database spells it: as <see cref="ISqlCommandGenerator.SafeId"/> does,
        /// unless the generator names schemas its own way.</summary>
        public static string SafeSchema(this ISqlCommandGenerator @this, string schema) =>
            (@this as ISchemaNamingSqlCommandGenerator)?.SafeSchema(schema) ?? @this.SafeId(schema);
    }
}
