using System.Collections.Generic;

namespace Olive
{
    public abstract class BackgroundJobsPlan
    {
        internal static Dictionary<string, BackgroundJob> Jobs { get; } = new Dictionary<string, BackgroundJob>();

        public abstract void Initialize();

        public void Register(BackgroundJob job) => Jobs[job.Name.ToPascalCaseId()] = job;
    }
}
