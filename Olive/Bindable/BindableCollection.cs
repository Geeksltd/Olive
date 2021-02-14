using System;
using System.Collections.Generic;
using System.Collections;

namespace Olive
{
    public abstract class BindableCollection : IEnumerable
    {
        public event Action Changed;
        protected void FireChanged() => Changed?.Invoke();

        IEnumerator IEnumerable.GetEnumerator() => GetItemsEnumerator();
        protected abstract IEnumerator GetItemsEnumerator();

        public virtual void ClearBindings() => Changed = null;
    }

    public class BindableCollection<T> : BindableCollection, IEnumerable<T>
    {
        readonly List<T> Items = new List<T>();

        public event Action<T> Added;
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
        /// Adds a single item and then fires Changed, to update the UI. 
        /// It's an expensive call. Avoid calling it multiple times to create a list.
        /// </summary> 
        public void Add(T item)
        {
            Items.Add(item ?? throw new ArgumentNullException(nameof(item)));
            FireAdded(item);
            FireChanged();
        }

        public void Add(IEnumerable<T> items)
        {
            Items.AddRange(items.ExceptNull());
            items.Do(x => FireAdded(x));
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
            Items.Remove(item);
            FireChanged();
        }

        /// <summary>
        /// Removes all items and then fires Changed, to update the UI. 
        /// Consider using Replace() instead.
        /// </summary> 
        public void Clear()
        {
            ClearCore();
            FireChanged();
        }

        /// <summary>
        /// It's similar to calling Clear() and then Add() but it's faster.
        /// Because it clears the items, and adds the newly provided items all in one go, and then fires the Changed event once to update the UI.        
        /// </summary>
        public void Replace(IEnumerable<T> items)
        {
            ClearCore();
            Add(items);
        }

        /// <summary>
        /// It's similar to calling Clear() and then Add() but it's faster.
        /// Because it clears the items, and adds the newly provided item all in one go, and then fires the Changed event once to update the UI.        
        /// </summary>
        public void Replace(T item) => Replace(new[] { item });

        void ClearCore()
        {
            Items.Do(x => FireRemoving(x));
            Items.Clear();
        }

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
        protected override IEnumerator GetItemsEnumerator() => Items.GetEnumerator();

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        public T this[int index] => Items[index];
    }
}