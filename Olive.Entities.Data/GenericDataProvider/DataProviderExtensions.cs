using System;
using System.Collections.Generic;
using System.Linq;

namespace Olive.Entities.Data
{
    public static class DataProviderExtensions
    {
        public static DataProvider GetProvider(
                this IDataProviderMetaData @this, ICache cache, IDataAccess access, ISqlCommandGenerator sqlCommandGenerator)
        {
            return GetProvider(@this.Type, cache, access, sqlCommandGenerator);
        }

        public static DataProvider GetProvider(
                this Type @this, ICache cache, IDataAccess access, ISqlCommandGenerator sqlCommandGenerator)
        {
            return InternalDataProviderFactory.Get(@this, cache, access, sqlCommandGenerator);
        }

        public static string GetTableTemplate(this IDataProviderMetaData @this)
        {
            var result = "";

            void addTable(IDataProviderMetaData medaData)
            {
                var baseType = medaData.BaseClassesInOrder.LastOrDefault();

                result += " LEFT OUTER JOIN ".OnlyWhen(result.HasValue()) +
                    $"{medaData.Schema.WithSuffix(".")}{medaData.TableName} AS [{{0}}{medaData.TableAlias}] " +
                    $"ON [{{0}}{medaData.TableAlias}].[{medaData.IdColumnName}] = [{{0}}{baseType?.TableAlias}].[{baseType?.IdColumnName}]".OnlyWhen(baseType != null);
            }

            foreach (var parent in @this.BaseClassesInOrder)
                addTable(parent);

            addTable(@this);

            foreach (var drived in @this.DrivedClasses)
                addTable(drived);

            return result;
        }

        internal static IEnumerable<IPropertyData> GetPropertiesForFillData(this IDataProviderMetaData @this)
        {
            if (@this.BaseClassesInOrder.HasAny())
                return @this.UserDefienedProperties;

            return @this.UserDefienedAndIdProperties;
        }

        internal static IEnumerable<IPropertyData> GetPropertiesForInsert(this IDataProviderMetaData @this)
        {
            var result = @this.UserDefienedAndIdProperties.Except(p => p.IsAutoNumber);

            if (@this.IsSoftDeleteEnabled)
                return result.Concat(@this.Properties.First(p => p.IsDeleted));

            return result;
        }
            
    }
}