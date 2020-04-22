using Olive.Entities;
using System;

namespace Olive.Audit
{
    [LogEvents(false), CacheObjects(false)]
    public interface IAuditEvent : IEntity
    {
        DateTime Date { get; set; }
        string Event { get; set; }

        string ItemId { get; set; }
        string ItemType { get; set; }
        string ItemData { get; set; }
        string ItemGroup { get; set; }

        string UserId { get; set; }
        string UserIp { get; set; }
    }
}