using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract class ReplicationEventArgs
    {
        public ReplicateDataMessage Message { get; private set; }
        public IEntity Instance { get; private set; }

        protected ReplicationEventArgs(ReplicateDataMessage message, IEntity instance)
        {
            Message = message;
            Instance = instance;
        }
    }

    public class ReplicationDeleteMessageReceivedEventArgs : ReplicationEventArgs
    {
        public bool Cancel { get; set; }
        internal ReplicationDeleteMessageReceivedEventArgs(ReplicateDataMessage message, IEntity instance) : base(message, instance) { }
    }

    public class ReplicationSaveMessageReceivedEventArgs : ReplicationEventArgs
    {
        public SaveMode Mode { get; private set; }
        public bool Cancel { get; set; }
        internal ReplicationSaveMessageReceivedEventArgs(ReplicateDataMessage message, IEntity instance, SaveMode mode) : base(message, instance)
        {
            Mode = mode;
        }
    }

    public class ReplicationDeleteMessageProcessedEventArgs : ReplicationEventArgs
    {
        internal ReplicationDeleteMessageProcessedEventArgs(ReplicateDataMessage message, IEntity instance) : base(message, instance) { }
    }

    public class ReplicationSaveMessageProcessedEventArgs : ReplicationEventArgs
    {
        public SaveMode Mode { get; private set; }
        internal ReplicationSaveMessageProcessedEventArgs(ReplicateDataMessage message, IEntity instance, SaveMode mode) : base(message, instance)
        {
            Mode = mode;
        }
    }

    partial class DestinationEndpoint
    {
        public event AwaitableEventHandler<ReplicationSaveMessageReceivedEventArgs> Saving;
        public event AwaitableEventHandler<ReplicationSaveMessageProcessedEventArgs> Saved;
        public event AwaitableEventHandler<ReplicationDeleteMessageReceivedEventArgs> Deleting;
        public event AwaitableEventHandler<ReplicationDeleteMessageProcessedEventArgs> Deleted;

        internal async Task<bool> OnDeleting(ReplicateDataMessage message, IEntity entity)
        {
            if (Deleting is null) return true;
            var args = new ReplicationDeleteMessageReceivedEventArgs(message, entity);
            await Deleting.Raise(args);
            return !args.Cancel;
        }

        internal async Task OnDeleted(ReplicateDataMessage message, IEntity entity)
        {
            if (Deleted != null)
                await Deleted.Raise(new ReplicationDeleteMessageProcessedEventArgs(message, entity));
        }

        internal async Task<bool> OnSaving(ReplicateDataMessage message, IEntity entity, SaveMode mode)
        {
            if (Saving is null) return true;
            var args = new ReplicationSaveMessageReceivedEventArgs(message, entity, mode);
            await Saving.Raise(args);
            return !args.Cancel;
        }

        internal async Task OnSaved(ReplicateDataMessage message, IEntity entity, SaveMode mode)
        {
            if (Saved != null)
                await Saved.Raise(new ReplicationSaveMessageProcessedEventArgs(message, entity, mode));
        }
    }
}