using Olive.Entities;
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
        /// </summary>
        public static async Task OptimizeImage(this Blob blob, int maxWidth, int maxHeight, int quality, bool toJpeg = true)
        {
            if (!Blob.HasFileDataInMemory(blob)) return;
            var data = await blob.GetFileDataAsync();
            if (data.Length < 80) return;

            var optimizer = new ImageOptimizer(maxWidth, maxHeight, quality);
            blob.SetData(optimizer.Optimize(data,blob.FileExtension.TrimStart('.'), toJpeg));
        }
    }
}
