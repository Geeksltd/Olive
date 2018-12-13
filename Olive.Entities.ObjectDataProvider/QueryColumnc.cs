using System;

namespace Olive.Entities.ObjectDataProvider
{
    class QueryColumn
    {
        public string Identifier, SqlExpression;
        internal Property Property;
        internal Type Type;
        internal string GetCode(string code)
        {
            if (Property == null) return code;

            return code;
        }
    }
}