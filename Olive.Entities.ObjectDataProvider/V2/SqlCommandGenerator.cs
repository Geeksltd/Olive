using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Olive.Entities.Data;

namespace Olive.Entities.ObjectDataProvider.V2
{
    public abstract class SqlCommandGenerator
    {
        protected abstract string SafeId(string objectName);

        public abstract string GenerateSelectCommand(IDatabaseQuery iquery, DataProviderMetaData metaData, string fields);

        public virtual string GenerateWhere(DatabaseQuery query, DataProviderMetaData metaData)
        {
            throw new NotImplementedException();
        }

        public virtual string GenerateUpdateCommand(DataProviderMetaData metaData)
        {
            var properties = metaData.Properties.Except(p => p.IsUserDefined);

            return $@"UPDATE {GetFullTablaName(metaData)} SET
                {properties.Select(x => $"{SafeId(x.Name)} = @{x.Name}").ToString(", ")}
                WHERE {metaData.IdColumnName} = @OriginalId";
        }

        public virtual string GenerateInsertCommand(DataProviderMetaData metaData)
        {
            var properties = metaData.Properties.Except(p => p.IsUserDefined);

            return $@"INSERT INTO {GetFullTablaName(metaData)}
                ({properties.Select(x => SafeId(x.Name)).ToString(", ")})
                VALUES
                ({properties.Select(x => $"@{x.Name}").ToString(", ")})";
        }

        internal virtual string GenerateDeleteCommand(DataProviderMetaData metaData) =>
            $"DELETE FROM {GetFullTablaName(metaData)} WHERE {metaData.IdColumnName} = @Id";

        string GetFullTablaName(DataProviderMetaData metaData) =>
            metaData.Schema.WithSuffix(".") + metaData.TableName;
}
}
