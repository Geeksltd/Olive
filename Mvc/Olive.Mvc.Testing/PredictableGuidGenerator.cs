using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Olive.Mvc.Testing
{
    class PredictableGuidGenerator
    {
        static Dictionary<Type, int> UsedNumbers = new Dictionary<Type, int>();
        static object SyncLock = new object();
        static string CurrentTestName;

        internal static void Reset(string testName)
        {
            UsedNumbers = new Dictionary<Type, int>();
            CurrentTestName = testName;
        }

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
                var asText = string.Concat(CurrentTestName.Or("N/A"), type.Name, Next(type));
                var data = MD5.Create().ComputeHash(asText.ToBytes(Encoding.UTF8));
                return new Guid(data);
            }
        }
    }
}