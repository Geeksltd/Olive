namespace Olive.Entities.Data.IRepository.GenericRepository;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal static class DataReaderExtensions
{
    public static bool ColumnExists(this IDataReader reader, string columnName)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
