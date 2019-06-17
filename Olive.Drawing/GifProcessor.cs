namespace Olive.Drawing
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;

    class GifProcessor
    {
        static object SyncLock = new object();

        Bitmap Source;
        ColorPalette CorrectPallete;

        public GifProcessor(Bitmap sourceImage)
        {
            Source = sourceImage ?? throw new ArgumentNullException("sourceImage");
            CorrectPallete = GifPalleteGenerator.GeneratePallete(sourceImage);
        }

        Bitmap CreateBitmapWithIndexedColors(Bitmap source)
        {
            var width = source.Width;
            var height = source.Height;

            var result = new Bitmap(width, height, PixelFormat.Format8bppIndexed)
            {
                Palette = CorrectPallete
            };

            var bitmapData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            var pixels = bitmapData.Scan0;

            unsafe
            {
                var pointer = (byte*)pixels.ToPointer();
                if (bitmapData.Stride <= 0)
                    pointer += bitmapData.Stride * (height - 1);

                var stride = (uint)Math.Abs(bitmapData.Stride);

                for (uint x = 0; x < width; ++x)
                {
                    for (uint y = 0; y < height; ++y)
                    {
                        var pixel = source.GetPixel((int)x, (int)y);

                        byte* newPointer = x + pointer + y * stride;

                        // Set that byte to the color in the new pallet
                        *newPointer = FindPalleteEntryIndex(pixel);
                    }
                }
            }

            // Unlock the relevant area of the bitmap so the changes can be committed.
            result.UnlockBits(bitmapData);

            return result;
        }

        static Bitmap CreateCopy(Bitmap source) =>
             source.Clone(new Rectangle(0, 0, source.Width, source.Height), PixelFormat.Format32bppArgb);

        public void Save(string filename, bool transparent)
        {
            var width = Source.Width;
            var height = Source.Height;

            lock (SyncLock)
            {
                using (var copy = CreateCopy(Source))
                {
                    var toSave = CreateBitmapWithIndexedColors(copy);

                    toSave.Save(filename, System.Drawing.Imaging.ImageFormat.Gif);
                }
            }
        }

        /// <summary>
        /// Finds the index of the relevant entry in the new pallete to the specified color.
        /// </summary>
        byte FindPalleteEntryIndex(Color color)
        {
            if (color.A == 0)
                // First entry in the pallete is "Transparent"
                return 0;

            // If there is an exact match in the entries for this color, then obviously that's our entry:
            for (byte index = 0; index < byte.MaxValue; index++)
                if (CorrectPallete.Entries[index] == color) return index;

            // Otherwise, find the nearest color:
            return CorrectPallete.Entries.Select((c, i) => new { Index = (byte)i, Color = c }).WithMin(entry => GetDifference(color, entry.Color)).Index;
        }

        /// <summary>
        /// Gets the difference between the 2 specified colors.
        /// </summary>
        static int GetDifference(Color c1, Color c2)
        {
            return
                  Math.Abs(c1.A - c2.A) +
                  Math.Abs(c1.R - c2.R) +
                  Math.Abs(c1.G - c2.G) +
                  Math.Abs(c1.B - c2.B);
        }
    }
}