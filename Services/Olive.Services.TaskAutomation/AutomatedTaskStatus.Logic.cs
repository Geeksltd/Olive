namespace Olive.Services.TaskAutomation
{
    partial class AutomatedTaskStatus
    {
        /// <summary>
        /// Creates a new AutomatedTaskStatus instance.
        /// </summary>
        public AutomatedTaskStatus(string name) => Name = name;

        internal static AutomatedTaskStatus AwaitingFirstRun = new AutomatedTaskStatus("Awaiting First Run");
        internal static AutomatedTaskStatus Running = new AutomatedTaskStatus("Running");
        internal static AutomatedTaskStatus WaitingForLock = new AutomatedTaskStatus("Waiting For Lock");
        internal static AutomatedTaskStatus CompletedAwaitingNextRun = new AutomatedTaskStatus("Completed, Awaiting Next Run");
        internal static AutomatedTaskStatus FailedAwaitingNextRun = new AutomatedTaskStatus("Failed, Awaiting Next Run");
    }
}