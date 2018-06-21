using System;

namespace Olive
{
    [Obsolete("This enum is deprecated, please use FallBack")]
    public enum OnApiCallError
    {
        Throw,
        Ignore,
        IgnoreAndNotify
    }
}