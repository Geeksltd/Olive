namespace Olive.MFA
{
    using Olive.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [LogEvents(false)]
    [CacheObjects(false)]
    public interface ITemporaryLogin : IEntity
    {
        DateTime CreatedAt { get; set; }
        int ExpiryMinutes { get; set; }
        string MFACode { get; set; }

    }
}
