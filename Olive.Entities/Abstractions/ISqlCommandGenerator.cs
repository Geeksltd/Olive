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
}
