using System;

namespace Olive.Entities
{
    public class SqlConversionContext
    {
        public Type Type;
        public string Alias;
        public IDatabaseQuery Query;
        public Func<string, string> ToSafeId;

        /// <summary>Escapes a table alias. Falls back to ToSafeId when not set.</summary>
        public Func<string, string> ToSafeAlias;
    }
}
