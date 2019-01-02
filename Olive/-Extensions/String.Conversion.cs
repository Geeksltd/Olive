using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Olive
{
    partial class OliveExtensions
    {
        public delegate bool TryParseProvider(string text, Type type, out object result);

        public static List<TryParseProvider> TryParseProviders = new List<TryParseProvider>();

        /// <summary>
        /// Determines whether this string can be converted to the specified type.
        /// </summary>
        public static bool Is<T>(this string text) where T : struct => text.TryParseAs<T>().HasValue;

        /// <summary>
        /// Tries to parse this text to the specified type.
        /// Returns null if parsing is not possible.
        /// </summary>
        [EscapeGCop("It is ok for trying methods to have out param.")]
        public static T? TryParseAs<T>(this string text) where T : struct
        {
            if (text.IsEmpty()) return default(T?);

            // Check common types first, for performance:
            if (TryParseToCommonTypes<T>(text, out var result))
                return result;

            foreach (var parser in TryParseProviders)
                if (parser(text, typeof(T), out var result2))
                    return (T)result2;

            try { return (T)Convert.ChangeType(text, typeof(T)); }
            catch
            {
                // No logging is needed
                return null;
            }
        }

        [EscapeGCop("It is ok for trying methods to have out param.")]
        static bool TryParseToCommonTypes<T>(this string text, out T? result) where T : struct
        {
            result = null;

            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(text, out var tempResult)) result = (T)(object)tempResult;

                return true;
            }

            if (typeof(T) == typeof(double))
            {
                if (double.TryParse(text, out var tempResult)) result = (T)(object)tempResult;

                return true;
            }

            if (typeof(T) == typeof(decimal))
            {
                if (decimal.TryParse(text, out var tempResult)) result = (T)(object)tempResult;

                return true;
            }

            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(text, out var tempResult)) result = (T)(object)tempResult;

                return true;
            }

            if (typeof(T) == typeof(DateTime))
            {
                if (DateTime.TryParse(text, out var tempResult)) result = (T)(object)tempResult;

                return true;
            }

            if (typeof(T) == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(text, out var tempResult)) result = (T)(object)tempResult;

                return true;
            }

            if (typeof(T) == typeof(Guid))
            {
                if (Guid.TryParse(text, out var tempResult)) result = (T)(object)tempResult;

                return true;
            }

            if (typeof(T).IsEnum)
            {
                if (Enum.TryParse<T>(text, ignoreCase: true, result: out var tempResult)) result = (T)(object)tempResult;

                return true;
            }

            if (typeof(T) == typeof(ShortGuid))
            {
                try { result = (T)(object)ShortGuid.Parse(text); }
                catch
                {
                    // No logging is needed
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// It converts this text to the specified data type. 
        /// It supports all primitive types, Enums, Guid, XElement, XDocument, Color, ...
        /// </summary>
        public static T To<T>(this string text) => (T)To(text, typeof(T));

        /// <summary>
        /// Converts the value of this string object into the specified target type.
        /// It supports all primitive types, Enums, Guid, XElement, XDocument, Color, ...
        /// </summary>
        public static object To(this string text, Type targetType)
        {
            try
            {
                return ChangeType(text, targetType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not convert \"{text}\" to type { targetType.FullName}.", ex);
            }
        }

        [EscapeGCop("I AM the solutino to this GCop rule.")]
        static object ChangeType(string text, Type targetType)
        {
            if (targetType == typeof(string)) return text;

            if (text.IsEmpty()) return targetType.GetDefaultValue();

            // Check common types first, for performance:
            if (TryParseToCommonTypes(text, targetType, out var result))
                return result;

            if (targetType.IsEnum) return Enum.Parse(targetType, text);

            if (targetType == typeof(XElement)) return XElement.Parse(text);

            if (targetType == typeof(XDocument)) return XDocument.Parse(text);

            if (targetType == typeof(ShortGuid)) return ShortGuid.Parse(text);

            foreach (var parser in TryParseProviders)
                if (parser(text, targetType, out var result2))
                    return result2;

            return Convert.ChangeType(text, targetType);
        }

        [EscapeGCop("It is ok for trying methods to have out param.")]
        static bool TryParseToCommonTypes(string text, Type targetType, out object result)
        {
            var actualTargetType = targetType;

            var isNullable = targetType.IsNullable();

            if (isNullable)
                targetType = targetType.GetGenericArguments().Single();

            result = null;

            try
            {
                if (targetType == typeof(int)) result = int.Parse(text);

                if (targetType == typeof(long)) result = long.Parse(text);

                if (targetType == typeof(double)) result = double.Parse(text);

                if (targetType == typeof(decimal)) result = decimal.Parse(text);

                if (targetType == typeof(bool)) result = bool.Parse(text);

                if (targetType == typeof(DateTime)) result = DateTime.Parse(text);

                if (targetType == typeof(Guid)) result = new Guid(text);

                if (targetType == typeof(TimeSpan))
                {
                    if (text.Is<long>()) result = TimeSpan.FromTicks(text.To<long>());
                    else result = TimeSpan.Parse(text);
                }

                return result != null;
            }
            catch
            {
                if (targetType.IsAnyOf(typeof(int), typeof(long)))
                {
                    if (text.Contains(".") && text.RemoveBeforeAndIncluding(".", caseSensitive: true).All(x => x == '0'))
                        result = text.RemoveFrom(".").To(actualTargetType);
                }

                if (isNullable)
                    return true;
                else
                    throw; // Although it is a try method, it is ok to raise an exception.
            }
        }
    }
}