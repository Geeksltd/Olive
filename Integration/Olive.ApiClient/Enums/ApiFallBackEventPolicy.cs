namespace Olive
{
    public enum ApiFallBackEventPolicy
    {
        /// <summary>
        /// ApiClient.FallBack event can be raised.
        /// </summary>
        Silent,

        /// <summary>
        /// ApiClient.FallBack event should not be raised.
        /// </summary>
        Raise
    }
}