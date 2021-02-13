using System.Collections.Generic;

namespace System
{
    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] left, byte[] right)
        {
            if (left.Length != right.Length) return false;

            for (var i = 0; i < left.Length; i++)
                if (left[i] != right[i]) return false;

            return true;
        }

        public int GetHashCode(byte[] key)
        {
            var sum = 0;
            foreach (var cur in key) sum += cur;
            return sum;
        }
    }
}