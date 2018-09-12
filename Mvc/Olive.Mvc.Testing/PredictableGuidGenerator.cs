using System;
using System.Collections.Generic;

namespace Olive.Mvc.Testing
{
    class PredictableGuidGenerator
    {
        static Dictionary<Type, int> UsedNumbers = new Dictionary<Type, int>();
        static object SyncLock = new object();

        internal static void Reset() => UsedNumbers = new Dictionary<Type, int>();

        static int Next(Type type)
        {
            if (UsedNumbers.TryGetValue(type, out var current))
                return UsedNumbers[type] = current + 1;
            else return UsedNumbers[type] = 1;
        }

        public static Guid Generate(Type type)
        {
            lock (SyncLock)
            {
                var parts = new[] {
                    WebTestConfig.TestName.Or("N/A").GetHashCode(), // current test
                    type.GetHashCode(), // type
                    Next(type) // object
                };

                var bytes = new byte[16];
                for (var i = 0; i < 3; i++)
                {
                    BitConverter.GetBytes(parts[i]).CopyTo(bytes, i * 4);
                }

                return new Guid(bytes);
            }
        }
    }
}