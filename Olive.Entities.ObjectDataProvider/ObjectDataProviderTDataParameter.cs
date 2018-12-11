using System.Data;

namespace Olive.Entities.ObjectDataProvider
{
    internal class ObjectDataProviderTDataParameter : IDataParameter
    {
        internal ObjectDataProviderTDataParameter(string parameterName, object value, DbType dbType)
        {
            ParameterName = parameterName;
            Value = value;
            DbType = dbType;
            Direction = ParameterDirection.Input;
            SourceColumn = parameterName;
            IsNullable = true;
        }
        public DbType DbType { get; set; }
        public ParameterDirection Direction { get; set; }

        public bool IsNullable { get; set; }

        public string ParameterName { get; set; }
        public string SourceColumn { get; set; }
        public DataRowVersion SourceVersion { get; set; }
        public object Value { get; set; }
    }
}