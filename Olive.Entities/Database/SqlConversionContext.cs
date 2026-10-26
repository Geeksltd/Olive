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

        /// <summary>Writes "text LIKE pattern", or NOT LIKE when the flag is set, for Contains, NotContains, BeginsWith
        /// and EndsWith, as the database's provider matches text best (see SqlCommandGenerator.Like). Plain LIKE when not set.</summary>
        public Func<string, string, bool, string> Like;
    }
}
