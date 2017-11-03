using System.Collections.Generic;

namespace Olive.Entities
{
    /// <summary>
    /// This interface can be implemented on any entity which has a property of type Blob.
    /// </summary>
    public interface IPickyBlobContainer : IEntity
    {
        /// <summary>
        /// Gets the path to the physical folder containing files for the specified blob property.
        /// If you don't need to implement this specific method, simply return NULL.
        /// </summary>
        string GetPhysicalFolderPath(Blob blob);

        /// <summary>
        /// Gets the URL to the virtual folder containing files for the specified blob property.
        /// If you don't need to implement this specific method, simply return NULL.
        /// </summary>
        string GetVirtualFolderPath(Blob blob);

        /// <summary>
        /// Gets the name of the file used for the specified blob property, without extension.
        /// If you don't need to implement this specific method, simply return NULL.
        /// </summary>
        string GetFileNameWithoutExtension(Blob blob);

        /// <summary>
        /// Gets the fallback paths for the specified blob.
        /// </summary>
        IEnumerable<string> GetFallbackPaths(Blob blob);
    }
}