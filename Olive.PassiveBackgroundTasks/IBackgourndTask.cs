using Olive.Entities;
using System;

namespace Olive.PassiveBackgroundTasks
{
    public interface IBackgourndTask : IEntity
    {
        string Name { get; set; }
        Guid? ExecutingInstance { get; set; }
        DateTime? Heartbeat { get; set; }
        DateTime? LastExecuted { get; set; }
        int IntervalInMinutes { get; set; }
        int TimeoutInMinutes { get; set; }
    }
}
