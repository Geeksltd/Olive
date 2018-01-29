using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
// using System.Drawing.Imaging;

namespace Olive.Drawing
{
    /// <summary>
    /// A utility to resize and optimise image files.
    /// </summary>    
    public class ImageOptimizer
    {
        const int DEFAULT_MAX_WIDTH = 900, DEFAULT_MAX_HEIGHT = 700, DEFAULT_QUALITY = 80;
        /// <summary>
        /// Creates a new instance of ImageOptimizer class with default settings.
        /// </summary>
        public ImageOptimizer() : this(DEFAULT_MAX_WIDTH, DEFAULT_MAX_HEIGHT, DEFAULT_QUALITY) { }

        /// <summary>
        /// Creates a new instance of ImageOptimizer class.
        /// </summary>		
        public ImageOptimizer(int maxWidth, int maxHeight, int quality)
        {
            MaximumWidth = maxWidth;
            MaximumHeight = maxHeight;
            Quality = quality;
            OutputFormat = ImageFormat.Jpeg;
        }

        public int MaximumWidth { get; set; }
        public int MaximumHeight { get; set; }
        public int Quality { get; set; }
        public ImageFormat OutputFormat { get; set; }

        /// <summary>
        /// Gets the available output image formats.
        /// </summary>
        public enum ImageFormat { Bmp = 0, Jpeg = 1, Gif = 2, Png = 4 }

        /// <summary>
        /// Applies the settings of this instance on a specified source image, and provides an output optimized/resized image.
        /// </summary>
        public SKBitmap Optimize(SKBitmap source)
        {
            // Calculate the suitable width and heigth for the output image:
            var width = source.Width;
            var height = source.Height;

            if (width > MaximumWidth)
            {
                height = (int)(height * (1.0 * MaximumWidth) / width);
                width = MaximumWidth;
            }

            if (height > MaximumHeight)
            {
                width = (int)(width * (1.0 * MaximumHeight) / height);
                height = MaximumHeight;
            }

            if (width == source.Width && height == source.Height)
                return source;

            var result = new SKBitmap(width, height);

            SKBitmap.Resize(result, source, SKBitmapResizeMethod.Lanczos3);

            return result;
        }

        /// <summary>
        /// Optimizes the specified source image and returns the binary data of the output image.
        /// </summary>
        public byte[] Optimize(byte[] sourceData, bool toJpeg = true)
        {
            try
            {
                using (var source = SKBitmap.Decode(sourceData))
                {
                    using (var resultBitmap = Optimize(source))
                    {
                        using (SKImage image = SKImage.FromBitmap(resultBitmap))
                        {
                            return image.Encode(toJpeg ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png, Quality).ToArray();
                        }
                    }
                }
            }
            catch
            {
                // No Logging Needed
                return sourceData;
            }
        }

        /// <summary>
        /// Applies optimization settings on a a source image file on the disk and saves the output to another file with the specified path.
        /// </summary>
        public async Task Optimize(string souceImagePath, string optimizedImagePath)
        {
            if (!File.Exists(souceImagePath))
                throw new Exception("Could not find the file: " + souceImagePath);

            SKBitmap source;

            try
            {
                source = SKBitmap.Decode(souceImagePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not obtain bitmap data from the file: {0}.".FormatWith(souceImagePath), ex);
            }

            using (source)
            {
                using (var optimizedImage = SKImage.FromBitmap(Optimize(source)))
                {
                    var encoded = optimizedImage.Encode(SKEncodedImageFormat.Jpeg, Quality).ToArray();
                    await optimizedImagePath.AsFile().WriteAllBytesAsync(encoded);
                }
            }
        }

        /// <summary>
        /// Applies optimization settings on a source image file.
        /// Please note that the original file data is lost (overwritten) in this overload.
        /// </summary>
        public Task Optimize(string imagePath) => Optimize(imagePath, imagePath);

        // EncoderParameters GenerateEncoderParameters()
        // {
        //    var result = new EncoderParameters(1);
        //    result.Param[0] = new EncoderParameter(Encoder.Quality, Quality);
        //    return result;
        // }

        // ImageCodecInfo GenerateCodecInfo() => ImageCodecInfo.GetImageEncoders()[(int)OutputFormat];
    }
}