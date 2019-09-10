using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace Olive.Drawing
{
    public static class GraphicExtensions
    {
        const double HALF_PI = Math.PI / 2.00;
        const double RADIANS = Math.PI / 180.0;

        /// <summary>
        /// Gets an image which is a column of this image at the specified index.
        /// </summary>
        public static Bitmap GetColumn(this Bitmap image, int columnIndex)
        {
            var height = image.Height;

            var result = new Bitmap(1, height, PixelFormat.Format32bppArgb);

            for (int i = 0; i < height; i++)
                result.SetPixel(0, i, image.GetPixel(columnIndex, i));

            return result;
        }

        /// <summary>
        /// Gets the width of a specified text in this font.
        /// </summary>
        public static int GetWidth(this Font font, string text, bool useAntialias)
        {
            using (var tempImage = new Bitmap(1, 1))
            {
                using (var tempGraphics = Graphics.FromImage(tempImage))
                {
                    if (useAntialias)
                        tempGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                    var result = (int)(tempGraphics.MeasureString(text, font).Width);

                    // Measure string trims the text by default. God knows why:
                    var spaces = text.Length - text.TrimEnd(' ').Length;

                    if (spaces > 0)
                    {
                        result += spaces * (int)(tempGraphics.MeasureString(" ", font).Width);
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Inserts the specified image at the specified column inside this host image.
        /// </summary>
        public static Bitmap Insert(this Bitmap host, int columnIndex, Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            if (image.Height != host.Height)
                throw new ArgumentException("The height of the specified image is different from the host image.");

            var height = host.Height;
            var width = host.Width + image.Width;
            var right = columnIndex + image.Width;
            var result = new Bitmap(width, height);

            Color pixel;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < columnIndex)
                        // Left section:
                        pixel = host.GetPixel(x, y);

                    else if (x < right)
                        // Middle section (inserting image)
                        pixel = image.GetPixel(x - columnIndex, y);

                    else
                        // Right section
                        pixel = host.GetPixel(x - image.Width, y);

                    if (pixel.A == 0)
                        // Transparent
                        result.SetPixel(x, y, Color.Transparent);
                    else
                        result.SetPixel(x, y, pixel);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a rotated version from this image.
        /// </summary>
        /// <param name="degrees">The number of degrees to rotate this image. Direction of rotation will be clock-wise.</param>
        public static Bitmap Rotate(this Image image, double degrees)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            // Use radians:
            degrees *= RADIANS;

            var originalWidth = (double)image.Width;
            var originalHeight = (double)image.Height;

            double newWidth, newHeight, topLeft, topRight, bottomLeft, bottomRight;
            int resultWidth, resultHeight;

            if ((degrees >= 0.0 && degrees < HALF_PI) || (degrees < (Math.PI + HALF_PI) && degrees >= Math.PI))
            {
                topLeft = originalWidth * Math.Abs(Math.Cos(degrees));
                topRight = originalWidth * Math.Abs(Math.Sin(degrees));

                bottomLeft = originalHeight * Math.Abs(Math.Cos(degrees));
                bottomRight = originalHeight * Math.Abs(Math.Sin(degrees));
            }
            else
            {
                topLeft = originalHeight * Math.Abs(Math.Sin(degrees));
                topRight = originalHeight * Math.Abs(Math.Cos(degrees));

                bottomLeft = originalWidth * Math.Abs(Math.Sin(degrees));
                bottomRight = originalWidth * Math.Abs(Math.Cos(degrees));
            }

            newWidth = bottomRight + topLeft;
            newHeight = topRight + bottomLeft;

            resultWidth = (int)Math.Ceiling(newWidth);
            resultHeight = (int)Math.Ceiling(newHeight);

            var result = new Bitmap(resultWidth, resultHeight);

            using (var graph = Graphics.FromImage(result))
            {
                Point[] points;

                if (degrees >= 0.0 && degrees < HALF_PI)
                    points = new[] { new Point((int)bottomRight, 0), new Point(resultWidth, (int)topRight), new Point(0, (int)bottomLeft) };
                else if (degrees < Math.PI && degrees >= HALF_PI)
                    points = new[] { new Point(resultWidth, (int)topRight), new Point((int)topLeft, resultHeight), new Point((int)bottomRight, 0) };
                else if (degrees < (Math.PI + HALF_PI) && degrees >= Math.PI)
                    points = new[] { new Point((int)topLeft, resultHeight), new Point(0, (int)bottomLeft), new Point(resultWidth, (int)topRight) };
                else
                    points = new[] { new Point(0, (int)bottomLeft), new Point((int)bottomRight, 0), new Point((int)topLeft, resultHeight) };

                graph.DrawImage(image, points);
            }

            return result;
        }

        /// <summary>
        /// Stretches the specified image.
        /// </summary>
        public static Bitmap Stretch(this Bitmap image, int width)
        {
            if (image == null || image.Width != 1)
                throw new Exception("Bitmap.Stretch() should be called on an image with one column only.");

            var result = new Bitmap(width, image.Height);
            for (int column = 0; column < image.Height; column++)
                for (int row = 0; row < width; row++)
                    result.SetPixel(row, column, image.GetPixel(0, column));

            return result;
        }

        public static void SaveAsGif(this Bitmap image, string path, bool transparent)
        {
            var processor = new GifProcessor(image);
            processor.Save(path, transparent);
        }
    }
}