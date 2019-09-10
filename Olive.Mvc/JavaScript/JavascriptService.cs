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

        int ArgumentsLength { get => Arguments?.Length ?? 0; }

        public JavascriptService(string key, string function, params object[] arguments)
        {
            ServiceKey = key;
            Function = function;
            Arguments = arguments;
        }

        public static bool operator !=(JavascriptService me, JavascriptService other)
        {
            return !(me == other);
        }

        public static bool operator ==(JavascriptService me, JavascriptService other)
        {
            if (me is null ^ other is null) return false;

            if (me is null) return true;

            if (me.ServiceKey != other.ServiceKey ||
                me.Function != other.Function ||
                me.ArgumentsLength != other.ArgumentsLength)
                return false;

            if (me.Arguments != null && other.Arguments != null)
                for (int index = 0; index < me.ArgumentsLength; index++)
                    if (me.Arguments[index].ToString() != other.Arguments[index].ToString())
                        return false;

            return true;
        }
    }
}
