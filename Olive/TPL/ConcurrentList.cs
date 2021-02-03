using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Olive
{
    public class ConcurrentList<T> : IList<T>
    {
        readonly List<T> List;
        readonly object Lock = new object();
        bool IsVirgin = true; // For performance  
        T FirstItem;

        public ConcurrentList() => List = new List<T>();

        public ConcurrentList(int capacity)
        {
            Lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            List = new List<T>(capacity);
        }

        public ConcurrentList(IEnumerable<T> items)
        {
            List = new List<T>(items);
            IsVirgin = false;
        }

        public override string ToString() => "ConcurrentList<" + typeof(T).GetProgrammingName() + "> [" + Count + "]";

        public void Add(T item)
        {
            lock (Lock)
            {
                IsVirgin = false;
                if (IsVirgin) FirstItem = item;
                List.Add(item);
            }
        }

        /// <summary>
        /// Adds an object only if it doesn't already exist in the list.
        /// </summary> 
        public void AddUnique(T item)
        {
            lock (Lock)
            {
                if (List.Contains(item)) return;
                IsVirgin = false;
                if (IsVirgin) FirstItem = item;
                List.Add(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (Lock)
            {
                IsVirgin = false;
                if (index == 0) FirstItem = item;
                List.Insert(index, item);
            }
        }

        public bool Remove(T item)
        {
            lock (Lock)
            {
                var result = List.Remove(item);
                FirstItem = List.FirstOrDefault();
                return result;
            }
        }

        public void RemoveAt(int index)
        {
            lock (Lock)
            {
                List.RemoveAt(index);
                if (index == 0) FirstItem = List.FirstOrDefault();
            }
        }

        public int IndexOf(T item)
        {
            lock (Lock)
            {
                if (IsVirgin) return -1;
                return List.IndexOf(item);
            }
        }

        public void Clear()
        {
            lock (Lock)
            {
                if (IsVirgin) return;
                List.Clear();
                FirstItem = default;
            }
        }

        public bool Contains(T item)
        {
            lock (Lock)
            {
                if (IsVirgin) return false;
                return List.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (Lock)
            {
                if (IsVirgin) return;
                List.CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            T[] copy;

            lock (Lock)
            {
                if (IsVirgin) return Enumerable.Empty<T>().GetEnumerator();
                copy = List.ToArray();
            }

            return ((IEnumerable<T>)copy).GetEnumerator();
        }

        public bool Any()
        {
            lock (Lock)
            {
                if (IsVirgin) return false;
                return List.Any();
            }
        }

        public IEnumerable<TType> OfType<TType>() where TType : T
        {
            lock (Lock)
            {
                if (IsVirgin) return Enumerable.Empty<TType>();
                return List.OfType<TType>();
            }
        }

        public bool AnyOfType<TType>() where TType : T
        {
            lock (Lock)
            {
                if (IsVirgin) return false;
                return List.Any(x => x is TType);
            }
        }

        public bool AnyOfType<TType>(Func<TType, bool> criteria) where TType : T
        {
            lock (Lock)
            {
                if (IsVirgin) return false;
                return List.Any(x => x is TType t && criteria(t));
            }
        }

        public bool Any(Func<T, bool> criteria)
        {
            lock (Lock)
            {
                if (IsVirgin) return false;
                return List.Any(criteria);
            }
        }

        public bool None()
        {
            lock (Lock)
            {
                if (IsVirgin) return true;
                return List.None();
            }
        }

        public T FirstOrDefault()
        {
            lock (Lock) return FirstItem;
        }

        public T FirstOrDefault(Func<T, bool> criteria)
        {
            lock (Lock)
            {
                if (IsVirgin) return default;
                if (FirstItem is T && criteria(FirstItem)) return FirstItem;
                return List.FirstOrDefault(criteria);
            }
        }

        public TType FirstOrDefaultOfType<TType>() where TType : T
        {
            lock (Lock)
            {
                if (IsVirgin) return default;
                if (FirstItem is TType t) return t;
                return List.OfType<TType>().FirstOrDefault();
            }
        }

        public TType FirstOrDefaultOfType<TType>(Func<TType, bool> criteria) where TType : T
        {
            lock (Lock)
            {
                if (IsVirgin) return default;
                if (FirstItem is TType t && criteria(t)) return t;
                return List.OfType<TType>().FirstOrDefault(criteria);
            }
        }

        public T LastOrDefault()
        {
            lock (Lock)
            {
                if (IsVirgin) return default;
                var index = List.Count - 1;
                if (index == -1) return default;
                return List[index];
            }
        }

        public T LastOrDefault(Func<T, bool> criteria = null)
        {
            lock (Lock)
            {
                if (IsVirgin) return default;

                for (var i = List.Count - 1; i >= 0; i--)
                {
                    var item = List[i];
                    if (criteria(item)) return item;
                }

                return default;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T this[int index]
        {
            get
            {
                lock (Lock) return List[index];
            }
            set
            {
                lock (Lock) List[index] = value;
            }
        }

        public int Count
        {
            get
            {
                lock (Lock)
                {
                    if (IsVirgin) return 0;
                    return List.Count;
                }
            }
        }

        public bool IsReadOnly => false;
    }
}
