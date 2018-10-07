namespace Olive
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class AsyncLockDeque<T> : IEnumerable<T>
    {
        T[] Buffer;
        int Offset;

        public AsyncLockDeque(int capacity = 5) => Buffer = new T[capacity];

        int Capacity => Buffer.Length;

        public int Count { get; private set; }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            var count = Count;
            for (var i = 0; i != count; ++i)
                yield return Buffer[(i + Offset) % Capacity];
        }

        void GrowBy(int items)
        {
            var newBuffer = new T[Capacity + items];

            if (Offset > (Capacity - Count))
            {
                var length = Capacity - Offset;
                Array.Copy(Buffer, Offset, newBuffer, 0, length);
                Array.Copy(Buffer, 0, newBuffer, length, Count - length);
            }
            else Array.Copy(Buffer, Offset, newBuffer, 0, Count);

            Buffer = newBuffer;
            Offset = 0;
        }

        public void Enqueue(T value)
        {
            if (Count == Capacity) GrowBy(5);

            Buffer[(Count + Offset) % Capacity] = value;
            ++Count;
        }

        public T Deque()
        {
            --Count;

            var index = Offset;
            Offset = (Offset + 1) % Capacity;

            return Buffer[index];
        }
    }
}