using System;
using System.Collections.Generic;

namespace Olive.Security
{
    public interface ILoginInfo
    {
        string DisplayName { get; }
        string ID { get; }
        string Email { get; }
        IEnumerable<string> GetRoles();
        TimeSpan Timeout { get; }
    }

    public class GenericLoginInfo : ILoginInfo
    {
        public IEnumerable<string> Roles { get; set; }
        public string DisplayName { get; set; }
        public string ID { get; set; }
        public string Email { get; set; }
        public TimeSpan Timeout { get; set; }
        IEnumerable<string> ILoginInfo.GetRoles() => Roles;
    }
}
