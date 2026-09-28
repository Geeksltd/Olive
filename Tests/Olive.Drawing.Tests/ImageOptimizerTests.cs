using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SkiaSharp;
using System;
using System.IO;
using System.Text;

namespace Olive.Drawing.Tests
{
    /// <summary>
    /// Tests for ImageOptimizer.Optimize(byte[], ...) across every input image type,
    /// for each of the three output modes: toJpeg, toWebp, and keeping the source format.
    /// </summary>
    [TestFixture]
    public class ImageOptimizerTests
    {
        const int MAX_WIDTH = 900, MAX_HEIGHT = 700, QUALITY = 80;

        [OneTimeSetUp]
        public void Setup() => Log.Init(NullLoggerFactory.Instance);

        static ImageOptimizer CreateOptimizer() => new ImageOptimizer(MAX_WIDTH, MAX_HEIGHT, QUALITY);

        #region Image builders

        static SKBitmap CreateBitmap(int width, int height, bool transparent = false)
        {
            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var alpha = transparent && x < width / 2 ? (byte)0 : (byte)255;
                    bitmap.SetPixel(x, y, new SKColor((byte)(x * 255 / width), (byte)(y * 255 / height), 128, alpha));
                }

            return bitmap;
        }

        static byte[] CreateImage(string extension, int width, int height, bool transparent = false)
        {
            switch (extension.ToLowerInvariant())
            {
                case "jpg":
                case "jpeg":
                case "jfif": return Encode(CreateBitmap(width, height), SKEncodedImageFormat.Jpeg);
                case "png": return Encode(CreateBitmap(width, height, transparent), SKEncodedImageFormat.Png);
                case "webp": return Encode(CreateBitmap(width, height, transparent), SKEncodedImageFormat.Webp);
                case "bmp": return CreateBmp(width, height);
                case "ico": return CreateIco(width, height);
                case "gif": return CreateGif();
                default: throw new NotSupportedException(extension);
            }
        }

        static byte[] Encode(SKBitmap bitmap, SKEncodedImageFormat format)
        {
            using (bitmap)
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(format, 100))
                return data.ToArray();
        }

        /// <summary>
        /// Skia cannot encode BMP, so write a 24-bit uncompressed one by hand.
        /// </summary>
        static byte[] CreateBmp(int width, int height)
        {
            var rowSize = (width * 3 + 3) / 4 * 4;
            var pixelDataSize = rowSize * height;

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            // File header
            writer.Write((byte)'B'); writer.Write((byte)'M');
            writer.Write(14 + 40 + pixelDataSize);
            writer.Write(0);
            writer.Write(14 + 40);

            // BITMAPINFOHEADER
            writer.Write(40);
            writer.Write(width);
            writer.Write(height);
            writer.Write((short)1);
            writer.Write((short)24);
            writer.Write(0);
            writer.Write(pixelDataSize);
            writer.Write(2835); writer.Write(2835);
            writer.Write(0); writer.Write(0);

            var padding = new byte[rowSize - width * 3];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    writer.Write((byte)128);
                    writer.Write((byte)(y * 255 / height));
                    writer.Write((byte)(x * 255 / width));
                }

                writer.Write(padding);
            }

            return stream.ToArray();
        }

        /// <summary>
        /// An ICO file holding a single PNG image.
        /// </summary>
        static byte[] CreateIco(int width, int height)
        {
            var png = CreateImage("png", width, height);

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write((short)0);
            writer.Write((short)1); // Icon
            writer.Write((short)1); // One image
            writer.Write((byte)(width >= 256 ? 0 : width));
            writer.Write((byte)(height >= 256 ? 0 : height));
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((short)1);
            writer.Write((short)32);
            writer.Write(png.Length);
            writer.Write(6 + 16);
            writer.Write(png);

            return stream.ToArray();
        }

        /// <summary>
        /// Skia cannot encode GIF, so use a known 1x1 transparent GIF.
        /// </summary>
        static byte[] CreateGif() => Convert.FromBase64String("R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==");

        static (SKEncodedImageFormat Format, int Width, int Height) Inspect(byte[] data)
        {
            using var codec = SKCodec.Create(new SKMemoryStream(data));
            Assert.That(codec, Is.Not.Null, "The output is not a decodable image.");
            return (codec.EncodedFormat, codec.Info.Width, codec.Info.Height);
        }

        static SKColor PixelAt(byte[] data, int x, int y)
        {
            using var bitmap = SKBitmap.Decode(data);
            return bitmap.GetPixel(x, y);
        }

        #endregion

        #region toJpeg (the default)

        [TestCase("jpg")]
        [TestCase("jpeg")]
        [TestCase("jfif")]
        [TestCase("png")]
        [TestCase("webp")]
        [TestCase("bmp")]
        [TestCase("ico")]
        [TestCase("gif")]
        public void ToJpeg_converts_every_type_to_jpeg(string extension)
        {
            var source = CreateImage(extension, 64, 48);

            var result = CreateOptimizer().Optimize(source, extension);

            Assert.That(Inspect(result).Format, Is.EqualTo(SKEncodedImageFormat.Jpeg));
        }

        #endregion

        #region toWebp

        [TestCase("jpg")]
        [TestCase("jpeg")]
        [TestCase("jfif")]
        [TestCase("png")]
        [TestCase("webp")]
        [TestCase("bmp")]
        [TestCase("ico")]
        [TestCase("gif")]
        public void ToWebp_converts_every_type_to_webp(string extension)
        {
            var source = CreateImage(extension, 64, 48);

            var result = CreateOptimizer().Optimize(source, extension, toJpeg: true, toWebp: true);

            Assert.That(Inspect(result).Format, Is.EqualTo(SKEncodedImageFormat.Webp));
        }

        [TestCase("png")]
        [TestCase("webp")]
        public void ToWebp_keeps_transparency(string extension)
        {
            var source = CreateImage(extension, 64, 48, transparent: true);

            var result = CreateOptimizer().Optimize(source, extension, toJpeg: false, toWebp: true);

            Assert.That(PixelAt(result, 5, 5).Alpha, Is.EqualTo(0));
            Assert.That(PixelAt(result, 60, 5).Alpha, Is.EqualTo(255));
        }

        #endregion

        #region Keeping the source format (toJpeg: false)

        // This is the case that used to throw NullReferenceException: "jpg" did not parse
        // to SKEncodedImageFormat.Jpeg, so it fell back to Wbmp, which Skia cannot encode.
        [TestCase("jpg", SKEncodedImageFormat.Jpeg)]
        [TestCase("jpeg", SKEncodedImageFormat.Jpeg)]
        [TestCase("jfif", SKEncodedImageFormat.Jpeg)]
        [TestCase("png", SKEncodedImageFormat.Png)]
        [TestCase("webp", SKEncodedImageFormat.Webp)]
        [TestCase("bmp", SKEncodedImageFormat.Png)]
        [TestCase("ico", SKEncodedImageFormat.Png)]
        [TestCase("gif", SKEncodedImageFormat.Png)]
        public void Keeping_format_encodes_to_the_matching_or_fallback_format(string extension, SKEncodedImageFormat expected)
        {
            var source = CreateImage(extension, 64, 48);

            var result = CreateOptimizer().Optimize(source, extension, toJpeg: false);

            Assert.That(result, Is.Not.SameAs(source), "The image was not optimized.");
            Assert.That(Inspect(result).Format, Is.EqualTo(expected));
        }

        [TestCase("JPG", SKEncodedImageFormat.Jpeg)]
        [TestCase(".jpg", SKEncodedImageFormat.Jpeg)]
        [TestCase(".PNG", SKEncodedImageFormat.Png)]
        [TestCase("WebP", SKEncodedImageFormat.Webp)]
        [TestCase("", SKEncodedImageFormat.Png)]
        [TestCase(null, SKEncodedImageFormat.Png)]
        public void Keeping_format_ignores_case_dot_and_missing_extension(string extension, SKEncodedImageFormat expected)
        {
            var source = CreateImage("png", 64, 48);

            var result = CreateOptimizer().Optimize(source, extension, toJpeg: false);

            Assert.That(Inspect(result).Format, Is.EqualTo(expected));
        }

        [TestCase("png")]
        [TestCase("webp")]
        public void Keeping_format_keeps_transparency(string extension)
        {
            var source = CreateImage(extension, 64, 48, transparent: true);

            var result = CreateOptimizer().Optimize(source, extension, toJpeg: false);

            Assert.That(PixelAt(result, 5, 5).Alpha, Is.EqualTo(0));
            Assert.That(PixelAt(result, 60, 5).Alpha, Is.EqualTo(255));
        }

        #endregion

        #region Resizing

        [TestCase("jpg")]
        [TestCase("png")]
        [TestCase("webp")]
        [TestCase("bmp")]
        public void Wide_image_is_scaled_down_to_max_width(string extension)
        {
            var source = CreateImage(extension, 1800, 600);

            var result = CreateOptimizer().Optimize(source, extension, toJpeg: false);

            var info = Inspect(result);
            Assert.That(info.Width, Is.EqualTo(900));
            Assert.That(info.Height, Is.EqualTo(300));
        }

        [TestCase("jpg")]
        [TestCase("png")]
        [TestCase("webp")]
        [TestCase("bmp")]
        public void Tall_image_is_scaled_down_to_max_height(string extension)
        {
            var source = CreateImage(extension, 700, 1400);

            var result = CreateOptimizer().Optimize(source, extension, toJpeg: false);

            var info = Inspect(result);
            Assert.That(info.Width, Is.EqualTo(350));
            Assert.That(info.Height, Is.EqualTo(700));
        }

        [Test]
        public void Image_too_wide_and_too_tall_fits_both_bounds()
        {
            var source = CreateImage("jpg", 2000, 1800);

            var result = CreateOptimizer().Optimize(source, "jpg");

            var info = Inspect(result);
            Assert.That(info.Width, Is.LessThanOrEqualTo(MAX_WIDTH));
            Assert.That(info.Height, Is.EqualTo(MAX_HEIGHT));
        }

        [TestCase("jpg")]
        [TestCase("png")]
        [TestCase("webp")]
        [TestCase("bmp")]
        [TestCase("ico")]
        [TestCase("gif")]
        public void Small_image_is_not_upscaled(string extension)
        {
            var width = extension == "gif" ? 1 : 64;
            var height = extension == "gif" ? 1 : 48;
            var source = CreateImage(extension, width, height);

            var result = CreateOptimizer().Optimize(source, extension);

            var info = Inspect(result);
            Assert.That(info.Width, Is.EqualTo(width));
            Assert.That(info.Height, Is.EqualTo(height));
        }

        #endregion

        #region Data that cannot be decoded

        [TestCase("svg", "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"10\" height=\"10\"><rect width=\"10\" height=\"10\"/></svg>")]
        [TestCase("jpg", "this is not an image")]
        [TestCase("heic", "\0\0\0\u0018ftypheic\0\0\0\0mif1heic")]
        public void Undecodable_data_returns_the_source_unchanged(string extension, string content)
        {
            var source = Encoding.UTF8.GetBytes(content);

            foreach (var (toJpeg, toWebp) in new[] { (true, false), (false, false), (false, true) })
            {
                var result = CreateOptimizer().Optimize(source, extension, toJpeg, toWebp);
                Assert.That(result, Is.SameAs(source), $"toJpeg: {toJpeg}, toWebp: {toWebp}");
            }
        }

        [Test]
        public void Truncated_jpeg_does_not_throw()
        {
            var full = CreateImage("jpg", 200, 150);
            var source = full.AsSpan(0, 20).ToArray();

            Assert.DoesNotThrow(() => CreateOptimizer().Optimize(source, "jpg"));
        }

        [Test]
        public void Empty_data_returns_the_source_unchanged()
        {
            var source = new byte[0];

            var result = CreateOptimizer().Optimize(source, "jpg");

            Assert.That(result, Is.SameAs(source));
        }

        #endregion
    }
}
