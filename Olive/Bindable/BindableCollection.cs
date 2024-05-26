using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Olive
{
    public class BindableCollection<T> : Bindable<IList<T>>, IBindableCollection<T>, IEnumerable<T>
    {
        public event Action<T> Added;

        public BindableCollection() : base(new List<T>()) { }
        protected void FireAdded(T item) => Added?.Invoke(item);

        public event Action<T> Removing;
        protected void FireRemoving(T item) => Removing?.Invoke(item);

        public override void ClearBindings()
        {
            base.ClearBindings();
            Added = null;
            Removing = null;
        }

        /// <summary>
        /// Inserts a single item and then fires Changed, to update the UI. 
        /// It's an expensive call. Avoid calling it multiple times to create a list.
        /// </summary> 
        public void Insert(int index, T item)
        {
            if (item is null) return;
            lock (Value) Value.Insert(index, item);
            FireAdded(item);
            ApplyBindings();
            FireChanged();
        }

        /// <summary>
        /// Adds a single item and then fires Changed, to update the UI. 
        /// It's an expensive call. Avoid calling it multiple times to create a list.
        /// </summary> 
        public void Add(T item)
        {
            if (item is null) return;
            lock (Value) Value.Add(item);
            FireAdded(item);
            ApplyBindings();
            FireChanged();
        }

        public void Add(IEnumerable<T> items)
        {
            var validItems = items.OrEmpty().ExceptNull().ToArray();

            lock (Value)
                Value.AddRange(validItems);

            validItems.Do(FireAdded);
            ApplyBindings();
            FireChanged();
        }

        /// <summary>
        /// Removes a single item and then fires Changed, to update the UI. 
        /// It's an expensive call. Avoid calling it multiple times to clean a list.
        /// </summary> 
        public void Remove(T item)
        {
            if (item == null) return;
            FireRemoving(item);

            lock (Value)
                Value.Remove(item);

            ApplyBindings();
            FireChanged();
        }

        /// <summary>
        /// Removes a single item by its index and then fires Changed, to update the UI. 
        /// It's an expensive call. Avoid calling it multiple times to clean a list.
        /// </summary> 
        public void RemoveAt(int index) => Remove(this[index]);

        /// <summary>
        /// Removes all items and then fires Changed, to update the UI. 
        /// Consider using Replace() instead.
        /// </summary> 
        public void Clear()
        {
            ClearCore();
            ApplyBindings();
            FireChanged();
        }

        /// <summary>
        /// It's similar to calling Clear() and then Add() but it's faster.
        /// Because it clears the items, and adds the newly provided items all in one go, and then fires the Changed event once to update the UI.        
        /// </summary>
        public void Replace(IEnumerable<T> items)
        {
            var additions = items.Except(Value);
            var deletions = Value.Except(items);

            if (items.Count() > 500)
                ReplaceCompletely(items, additions, deletions);
            else
                ReplacePartially(items, additions, deletions);
        }

        void ReplaceCompletely(IEnumerable<T> items, IEnumerable<T> additions, IEnumerable<T> deletions)
        {
            if (additions.None() && deletions.None())
                return;

            ClearCore();
            Add(items);
        }

        void ReplacePartially(IEnumerable<T> items, IEnumerable<T> additions, IEnumerable<T> deletions)
        {
            bool HasChanged() => additions.HasAny() || deletions.HasAny();

            if (!HasChanged())
                return;

            lock (Value)
            {
                if (deletions.HasAny())
                    Value.RemoveWhere(x => deletions.Contains(x));

                Value.AddRange(additions);

                if (HasChanged())
                    Value = Value.OrderBy(x => items.IndexOf(x)).ToList();
            }

            if (HasChanged())
            {
                ApplyBindings();
                FireChanged();
            }
        }

        /// <summary>
        /// It's similar to calling Clear() and then Add() but it's faster.
        /// Because it clears the items, and adds the newly provided item all in one go, and then fires the Changed event once to update the UI.        
        /// </summary>
        public void Replace(T item) => Replace(new[] { item });

        void ClearCore()
        {
            lock (Value)
            {
                Value.ToArray().Do(x => FireRemoving(x));
                Value.Clear();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetItemsEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            lock (Value)
                return ((IEnumerable<T>)Value.ToArray()).GetEnumerator();
        }

        protected virtual IEnumerator GetItemsEnumerator()
        {
            lock (Value)
                return Value.ToArray().GetEnumerator();
        }

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        public T this[int index] => Value[index];
    }
}