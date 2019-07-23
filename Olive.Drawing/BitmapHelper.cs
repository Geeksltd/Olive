using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Olive.Drawing
{
    public static class BitmapHelper
    {
        static EncoderParameters GenerateEncoderParameters(int quality) =>
             new EncoderParameters(1) { Param = { [0] = new EncoderParameter(Encoder.Quality, quality) } };

        static ImageCodecInfo GenerateCodecInfo(ImageFormat format)
        {
            var allCodecs = ImageCodecInfo.GetImageEncoders();
            return allCodecs.FirstOrDefault(a => a.FormatID == format.Guid) ?? allCodecs.ElementAt(1); // Defauilt JPEG
        }

        public static byte[] ToBuffer(this Image image)
        {
            if (image.RawFormat.Guid == ImageFormat.MemoryBmp.Guid)
                throw new ArgumentException("For a MemoryBMP the actual image format must be specified. Use the other overload of the ToBuffer method.");

            return image.ToBuffer(image.RawFormat);
        }

        /// <summary>
        /// Gets the binary data of this image.
        /// </summary>
        public static byte[] ToBuffer(this Image image, ImageFormat format, int quality = 100)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, GenerateCodecInfo(format), GenerateEncoderParameters(quality));
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Converts the specified binary data to a bitmap.
        /// </summary>
        public static Image FromBuffer(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return Bitmap.FromStream(stream);
            }
        }

        /// <summary>
        /// Determines whether the specified binary data is for a valid image.
        /// </summary>
        public static bool IsValidImage(byte[] buffer)
        {
            try
            {
                return FromBuffer(buffer) != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a resized version of this image.
        /// </summary>
        public static Image Resize(this Image source, int newWidth, int newHeight)
        {
            var result = new Bitmap(newWidth, newHeight);

            using (var grraphics = Graphics.FromImage(result))
            {
                grraphics.DrawImage(source, 0, 0, newWidth, newHeight);
                return result;
            }
        }

        /// <summary>
        /// Brightens or darkens this image to the specified level. Level should be between 0 and 255.
        /// 0 Means totally dark and 255 means totally bright.
        /// </summary>
        public static Image Brighten(this Image source, int level)
        {
            if (level < 0 || level > 255)
                throw new ArgumentException("Level must be between 0 and 255.");

            var result = new Bitmap(source);

            using (var gr = Graphics.FromImage(result))
            {
                using (var brush = new SolidBrush(Color.FromArgb(level, Color.White)))
                {
                    gr.FillRectangle(brush, new Rectangle(Point.Empty, source.Size));
                    return result;
                }
            }
        }

        /// <summary>
        /// Creates a graphics object for this image.
        /// </summary>
        public static Graphics CreateGraphics(this Image image)
        {
            var result = Graphics.FromImage(image);

            result.PageUnit = GraphicsUnit.Pixel;
            result.RenderingOrigin = Point.Empty;
            result.PageScale = 1f;
            result.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            result.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            return result;
        }

        /// <summary>
        /// Crops this image with the specified rectangle.
        /// </summary>
        public static Image Crop(this Image image, Rectangle rectangle)
        {
            //var result = new Bitmap(image, rectangle.Size);
            var result = new Bitmap(rectangle.Width, rectangle.Height);

            using (var gr = result.CreateGraphics())
            {
                gr.DrawImage(image, new Rectangle(Point.Empty, result.Size), rectangle, GraphicsUnit.Pixel);
            }

            return result;
        }
    }
}
