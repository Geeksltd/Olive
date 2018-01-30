using System;
using System.Collections.Generic;

namespace Olive
{
    partial class OliveExtensions
    {
        const string CHAR_SET_READABLE = "ACEFGHJKLMNPQRTUVWXYZ0123456789";
        const string CHAR_SET_FULL = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        /// <summary>
        /// Gets a random boolean value with the specified chance (0-100).
        /// </summary>
        public static bool NextBoolean(this Random random, double chance)
        {
            if (chance < 0 || chance > 100) throw new Exception("Chance should be between 0 and 100 percent.");

            return random.NextDouble() >= 1 - (chance / 100.0);
        }

        /// <summary>
        /// Gets a random boolean value.
        /// </summary>
        public static bool NextBoolean(this Random random) => NextBoolean(random, 50);

        /// <summary>
        /// Generates and returns a Random alphanumeric string.
        /// </summary>
        /// <param name="rng">Random instance.</param>
        /// <param name="length">Length of string to return</param>
        /// <param name="omitConfusableCharacters">Pass true to miss-out letters that can be confused with numbers (BDIOS)</param>
        /// <returns>String instance containing random alphanumeric characters.</returns>
        public static string NextAlphaNumericString(this Random rng, int length, bool omitConfusableCharacters = false)
        {
            if (length < 1) throw new ArgumentOutOfRangeException(nameof(length), "Length should be 1 or more.");

            var charSet = omitConfusableCharacters ? CHAR_SET_READABLE : CHAR_SET_FULL;

            var buffer = new char[length];
            var chLen = charSet.Length;
            for (var i = 0; i < length; i++)
                buffer[i] = charSet[rng.Next(chLen)];

            return new string(buffer);
        }

        /// <summary>
        /// Returns [quantity] number of unique random integers within the given range.
        /// </summary>
        public static List<int> PickNumbers(this Random randomProvider, int quantity, int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentException("Invalid min and Max value specified.");

            var possibleMaxQuantity = (maxValue - minValue) + 1;

            if (quantity > possibleMaxQuantity)
                throw new ArgumentException($"There are not {quantity} unique numbers between {minValue} and {maxValue}.");

            var result = new List<int>();

            var quantityPicked = 0;

            while (quantityPicked < quantity)
            {
                var candidate = randomProvider.Next(minValue, maxValue + 1);

                if (result.Contains(candidate)) continue;

                result.Add(candidate);
                quantityPicked++;
            }

            return result;
        }
    }
}