using Olive.Entities;
using SkiaSharp;
using System.IO;
using System.Threading.Tasks;

namespace Olive.Drawing
{
    public static class Extensions
    {
        const int IMAGE_DEFAULT_QUALITY = 70;

        /// <summary>
        /// Optimizes the image based on the settings in the arguments.
        /// </summary>
        public static Task OptimizeImage(this Blob blob, int maxWidth, int maxHeight) =>
            blob.OptimizeImage(maxWidth, maxHeight, IMAGE_DEFAULT_QUALITY);

        /// <summary>
        /// Optimizes the image based on the settings in the arguments.
        /// When toWebp is true, the image is converted to WebP whatever toJpeg says, and the file extension is changed to .webp.
        /// WebP keeps transparency, and an image that cannot be decoded (e.g. SVG) is left unchanged with its original extension.
        /// A WebP that is already within the bounds keeps its data unless re-encoding makes it smaller,
        /// so an image optimized before being assigned does not lose quality to a second lossy pass for nothing.
        /// </summary>
        public static async Task OptimizeImage(this Blob blob, int maxWidth, int maxHeight, int quality, bool toJpeg = true, bool toWebp = false)
        {
            if (!Blob.HasFileDataInMemory(blob)) return;
            var data = await blob.GetFileDataAsync();
           // if (data.Length < 80) return;

            var optimizer = new ImageOptimizer(maxWidth, maxHeight, quality);
            var result = optimizer.Optimize(data, blob.FileExtension.TrimStart('.'), toJpeg, toWebp);

            if (!toWebp)
            {
                blob.SetData(result);
                return;
            }

            if (ReferenceEquals(result, data)) return; // Could not be converted.

            if (result.Length < data.Length || !IsWebpWithin(data, maxWidth, maxHeight))
                blob.SetData(result);

            blob.FileName = Path.ChangeExtension(blob.FileName, ".webp");
        }

        static bool IsWebpWithin(byte[] data, int maxWidth, int maxHeight)
        {
            using var codec = SKCodec.Create(new SKMemoryStream(data));
            return codec?.EncodedFormat == SKEncodedImageFormat.Webp &&
                codec.Info.Width <= maxWidth && codec.Info.Height <= maxHeight;
        }
    }
}
