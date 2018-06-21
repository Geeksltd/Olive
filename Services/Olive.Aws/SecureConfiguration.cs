using Microsoft.Extensions.DependencyInjection;
using Olive.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System;

namespace Olive
{
    internal class SecureConfiguration : IConfiguration
    {
        Dictionary<string, string> Secrets = new Dictionary<string, string>();

        public SecureConfiguration()
        {
            // TODO: Download the secrets into the dictionary. 
            // See https://docs.google.com/document/d/1CRvhWy5uN3dIw-agmqTjhdl8aC4bkWYsFPS45XLWick/edit#
        }

        public string this[string key]
        {
            get => Secrets[key];
            set => throw new InvalidOperationException();
        }

        public IEnumerable<IConfigurationSection> GetChildren()
            => throw new InvalidOperationException();

        public IChangeToken GetReloadToken() => null;

        public IConfigurationSection GetSection(string key)
            => throw new InvalidOperationException();
    }
}