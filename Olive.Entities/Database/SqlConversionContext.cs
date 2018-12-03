using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Entities
{
    public class SqlConversionContext
    {
        public Type Type;
        public string Alias;
        public IDatabaseQuery Query;
        public Func<string, string> ToSafeId;
    }
}
