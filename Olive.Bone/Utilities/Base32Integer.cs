namespace Olive
{
    using System;
    using System.Text;

    public sealed class Base32Integer
    {
        // the valid chars for the encoding
        static readonly string ValidChars = "1AZ2WSX3" + "EDC4RFV5" + "TGB6YHN7" + "UJM8K9LP";

        public int Value { get; }

        /// <summary>
        /// Creates a new Base32Integer instance.
        /// </summary>
        public Base32Integer(int value) => Value = value;

        /// <summary>
        /// Creates a new Base32Integer instance.
        /// </summary>
        public Base32Integer(string base32Integer) => Value = FromBase32String(base32Integer);

        public override string ToString() => ToBase32String(Value);

        /// <summary>
        /// Converts an array of bytes to a Base32-k string.
        /// </summary>
        public static string ToBase32String(int value)
        {
            var r = new StringBuilder();

            do
            {
                var mod = value % 32;
                r.Insert(0, ValidChars[mod]);
                value = (value - mod) / 32;
            }
            while (value > 0);

            return r.ToString();
        }

        /// <summary>
        /// Converts a Base32-k string into an array of bytes.
        /// </summary>        
        public static int FromBase32String(string valueString)
        {
            var result = 0;

            for (var figureIndex = valueString.Length - 1; figureIndex >= 0; figureIndex--)
            {
                var figureChar = valueString[figureIndex];
                var figureValue = ValidChars.IndexOf(figureChar);

                result += (int)Math.Pow(32, valueString.Length - figureIndex - 1) * figureValue;
            }

            return result;
        }

        public static implicit operator int(Base32Integer value) => value.Value;

        public static implicit operator Base32Integer(int value) => new Base32Integer(value);
    }
}