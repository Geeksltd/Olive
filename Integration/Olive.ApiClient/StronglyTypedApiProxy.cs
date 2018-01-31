using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Olive
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class StronglyTypedApiProxy
    {
        protected List<Action<ApiClient>> Configurators = new List<Action<ApiClient>>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ApiResponseCache CacheChoice { get; set; } = ApiResponseCache.Accept;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public StronglyTypedApiProxy Configure(Action<ApiClient> config)
        {
            if (config != null) Configurators.Add(config);
            return this;
        }

        #region Hide default members

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        #endregion
    }
}