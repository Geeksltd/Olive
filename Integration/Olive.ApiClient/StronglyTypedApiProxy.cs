using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Olive
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class StronglyTypedApiProxy
    {
        protected List<Action<ApiClient>> Configurators = new List<Action<ApiClient>>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public StronglyTypedApiProxy Configure(Action<ApiClient> config)
        {
            if (config != null) Configurators.Add(config);
            return this;
        }
    }
}