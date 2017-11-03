namespace Olive.Entities
{
    public interface IPickyBlobUrlContainer : IEntity
    {
        /// <summary>
        /// Gets the url of the specified blob.
        /// </summary>
        string GetUrl(Blob blob);
    }
}