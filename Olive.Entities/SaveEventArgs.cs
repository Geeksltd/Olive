using System;

namespace Olive.Entities
{
    public class SaveEventArgs : EventArgs
    {
        public SaveEventArgs(SaveMode mode) => Mode = mode;
        public SaveMode Mode { get; private set; }
    }

    public enum SaveMode { Update, Insert }

    public class GlobalSaveEventArgs : SaveEventArgs
    {
        public IEntity Entity;
        public GlobalSaveEventArgs(IEntity entity, SaveMode mode) : base(mode)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }
    }
}