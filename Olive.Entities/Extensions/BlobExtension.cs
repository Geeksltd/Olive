using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Entities
{
    public static class BlobExtension
    {
        /// <summary>
        /// Determines whether this blob is an image.
        /// </summary>
        public static bool IsImage(this Blob doc)
        {
            return doc.FileExtension.ToLower().TrimStart(".")
                .IsAnyOf("jpg", "jpeg", "png", "bmp", "gif", "webp", "tiff", "svg");
        }

        public static string GetKey(this Blob document) => (document.FolderName + "/" + document.OwnerId()).KeepReplacing("//", "/").TrimStart("/");

    }
}
