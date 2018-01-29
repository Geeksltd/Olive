namespace Olive.SMS
{
    /// <summary>
    /// Represents a component that actually delivers SMS messages.
    /// This should be implemented for any 3rd party SMS gateway.
    /// </summary>
    public interface ISMSSender
    {
        /// <summary>
        /// Delivers the specified SMS message.
        /// The implementation of this method should not handle exceptions. Any exceptions will be logged by the engine.
        /// </summary>
        void Deliver(ISmsQueueItem sms);
    }
}
