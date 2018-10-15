using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Olive
{
    /// <summary>
    /// Reads the legacy web.config or app.config files.
    /// </summary>
    class XmlConfigReader : IConfiguration
    {
        Dictionary<string, string> AppSettings;
        public XmlConfigReader()
        {
            var asExe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + ".config";

            var configFile = new[] { "web.config", asExe }
            .Select(x => AppDomain.CurrentDomain.GetBaseDirectory().GetFile(x))
            .FirstOrDefault(x => x.Exists());

            if (configFile != null)
            {
                AppSettings = configFile.ReadAllText().To<XDocument>()
                       .Root.RemoveNamespaces()
                       .Element("appSettings")?.Elements()
                       .ToDictionary(x => x.GetValue<string>("@key"), x => x.GetValue<string>("@value"));
            }

            if (AppSettings == null) AppSettings = new Dictionary<string, string>();
        }

        public string this[string key] { get => AppSettings.GetOrDefault(key); set => AppSettings[key] = value; }

        public IChangeToken GetReloadToken() => new ReloadToken();

        public IEnumerable<IConfigurationSection> GetChildren() => throw new NotSupportedException();

        public IConfigurationSection GetSection(string key) => throw new NotSupportedException();

        class ReloadToken : IChangeToken, IDisposable
        {
            public bool HasChanged => false;
            public bool ActiveChangeCallbacks => false;
            void IDisposable.Dispose() { }

            public IDisposable RegisterChangeCallback(Action<object> callback, object state) => this;
        }
    }
}
