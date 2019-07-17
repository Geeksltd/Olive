using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc
{
    public class JavascriptService
    {
        public string ServiceKey { get; }
        public string Function { get; }
        public object[] Arguments { get; }

        public JavascriptService(string key, string function, params object[] arguments)
        {
            ServiceKey = key;
            Function = function;
            Arguments = arguments;
        }
    }
}
