using System;

namespace Olive
{
    /// <summary>
    /// Represents a globally unique identifier (GUID) with a  shorter string value.
    /// </summary>
    public struct ShortGuid
    {
        const int Thirty = 30;
        const int TwentyTwo = 22;

        Guid _guid;
        string _value;

        /// <summary>
        /// Gets/sets the underlying Guid
        /// </summary>
        public Guid Guid
        {
            get => _guid;
            set
            {
                if (value != _guid)
                {
                    _guid = value;
                    _value = Encode(value);
                }
            }
        }

        /// <summary>
        /// Gets/sets the underlying base64 encoded string
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                if (value != _value)
                {
                    _value = value;
                    _guid = Decode(value);
                }
            }
        }

        /// <summary>
        /// Equivalent to Guid.Empty.
        /// </summary>
        public static readonly ShortGuid Empty = new ShortGuid(Guid.Empty);

        /// <summary>
        /// Parses a specified text (that is either a normal Guid or a short guid).
        /// </summary>
        public static ShortGuid Parse(string text)
        {
            if (text.IsEmpty()) return Empty;
            if (text.Length >= Thirty)
                return new ShortGuid(new Guid(text));
            return new ShortGuid(text);
        }

        /// <summary>
        /// Creates a ShortGuid from a base64 encoded string
        /// </summary>
        /// <param name="value">The encoded guid as a 
        /// base64 string</param>
        public ShortGuid(string value)
        {
            _value = value;
            _guid = Decode(value);
        }

        /// <summary>
        /// Creates a ShortGuid from a Guid
        /// </summary>
        /// <param name="guid">The Guid to encode</param>
        public ShortGuid(Guid guid)
        {
            _value = Encode(guid);
            _guid = guid;
        }

        /// <summary>
        /// Returns the base64 encoded guid as a string
        /// </summary>
        public override string ToString() => _value;

        /// <summary>
        /// Returns a value indicating whether this instance and a 
        /// specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        public override bool Equals(object obj)
        {
            if (obj is ShortGuid)
                return _guid.Equals(((ShortGuid)obj)._guid);
            if (obj is Guid) return _guid.Equals((Guid)obj);
            if (obj is string)
                return _guid.Equals(((ShortGuid)obj)._guid);
            return false;
        }

        /// <summary>
        /// Returns the HashCode for underlying Guid.
        /// </summary>
        public override int GetHashCode() => _guid.GetHashCode();

        /// <summary>
        /// Initialises a new instance of the ShortGuid class
        /// </summary>
        public static ShortGuid NewGuid() => new ShortGuid(Guid.NewGuid());

        /// <summary>
        /// Creates a new instance of a Guid using the string value, 
        /// then returns the base64 encoded version of the Guid.
        /// </summary>
        /// <param name="value">An actual Guid string (i.e. not a ShortGuid)</param>
        public static string Encode(string value)
        {
            var guid = new Guid(value);
            return Encode(guid);
        }

        /// <summary>
        /// Encodes the given Guid as a base64 string that is 22 
        /// characters long.
        /// </summary>
        /// <param name="guid">The Guid to encode</param>
        public static string Encode(Guid guid)
        {
            var encoded = Convert.ToBase64String(guid.ToByteArray());
            encoded = encoded
                .Replace("/", "_")
                .Replace("+", "-");
            return encoded.Substring(0, TwentyTwo);
        }

        /// <summary>
        /// Decodes the given base64 string
        /// </summary>
        /// <param name="value">The base64 encoded string of a Guid</param>
        /// <returns>A new Guid</returns>
        public static Guid Decode(string value)
        {
            value = value
                .Replace("_", "/")
                .Replace("-", "+");
            var buffer = Convert.FromBase64String(value + "==");
            return new Guid(buffer);
        }

        /// <summary>
        /// Determines if both ShortGuids have the same underlying Guid value.
        /// </summary>
        public static bool operator ==(ShortGuid me, ShortGuid other)
        {
            if ((object)me == null) return (object)other == null;
            return me._guid == other._guid;
        }

        /// <summary>
        /// Determines if both ShortGuids do not have the 
        /// same underlying Guid value.
        /// </summary>
        public static bool operator !=(ShortGuid me, ShortGuid other) => !(me == other);

        /// <summary>
        /// Implicitly converts the ShortGuid to it's string equivilent
        /// </summary>
        public static implicit operator string(ShortGuid shortGuid) => shortGuid._value;

        /// <summary>
        /// Implicitly converts the ShortGuid to it's Guid equivilent
        /// </summary>
        public static implicit operator Guid(ShortGuid shortGuid) => shortGuid._guid;

        /// <summary>
        /// Implicitly converts the string to a ShortGuid
        /// </summary>
        public static implicit operator ShortGuid(string shortGuid) => new ShortGuid(shortGuid);

        /// <summary>
        /// Implicitly converts the Guid to a ShortGuid 
        /// </summary>
        public static implicit operator ShortGuid(Guid guid) => new ShortGuid(guid);
    }
}