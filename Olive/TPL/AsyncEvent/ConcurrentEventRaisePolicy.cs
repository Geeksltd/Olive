namespace Olive
{
    /// <summary>
    /// Determines how concurrent attempts to raise an event should be handled.
    /// </summary>
    public enum ConcurrentEventRaisePolicy
    {
        /// <summary>
        /// A new concurrent attempt to raise this event should be ignored while the previous raise is still running.
        /// </summary>
        Ignore,

        /// <summary>
        /// A new concurrent attempt to raise this event should be queued to run after the previous raise is still running.
        /// </summary>
        Queue,

        /// <summary>
        /// A new concurrent attempt to raise this event should run immediately irrespective of any unfinished previous raise.
        /// </summary>
        Parallel
    }
}
