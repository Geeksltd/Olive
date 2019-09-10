namespace Olive.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;

    /// <summary>
    /// Generates a color pallete for a GIF image.
    /// </summary>
    public static class GifPalleteGenerator
    {
        const int STANDARD_COLOR_PALLETTE_SIZE = 256;

        static readonly Color TRANSPARENT = Color.FromArgb(0, 0, 0, 0);

        /// <summary>
        /// Generates a color pallete based on the colors used in a specified image.
        /// </summary>
        public static ColorPalette GeneratePallete(Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            var result = CreateEmptyPallette();

            var usedColours = FindAllColours(image);

            result.Entries[0] = TRANSPARENT;

            for (int i = 1; i < STANDARD_COLOR_PALLETTE_SIZE; i++)
                if (usedColours.Count >= i)
                    result.Entries[i] = usedColours[i - 1].Key;
                else result.Entries[i] = Color.White;

            return result;
        }

        static ColorPalette CreateEmptyPallette()
        {
            using (var temp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                return temp.Palette;
            }
        }

        /// <summary>
        /// Finds all colours used in the specified image.
        /// The result will be the list of colours sorted by then umber of times that is used.
        /// </summary>
        static List<KeyValuePair<Color, int>> FindAllColours(Bitmap image)
        {
            var colors = new Dictionary<Color, int>();

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    var pixel = image.GetPixel(x, y);

                    if (colors.ContainsKey(pixel))
                        colors[pixel]++;
                    else
                        colors.Add(pixel, 1);
                }

            var result = from item in colors
                         orderby item.Value descending
                         select new KeyValuePair<Color, int>(item.Key, item.Value);

            return result.ToList();
        }
    }
}