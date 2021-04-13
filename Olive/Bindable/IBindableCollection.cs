using System;

namespace Olive
{
    public interface IBindableCollection : IBindable
    {
        public event Action Changed;
    }

    public interface IBindableCollection<T> : IBindableCollection
    {
        public event Action<T> Added;
        public event Action<T> Removing;
    }
}