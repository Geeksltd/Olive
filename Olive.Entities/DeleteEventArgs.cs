using System;

namespace Olive.Entities
{
    public class DeleteEventArgs : EventArgs
    {
    }

    public class GlobalDeleteEventArgs : DeleteEventArgs
    {
        public Type EntityType { get; private set;}
        public object EnityID { get; private set;}
        public IEntity Entity;
        public GlobalDeleteEventArgs(IEntity entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            EntityType = entity.GetType();
            EnityID = entity.GetId();
        }
    }
}