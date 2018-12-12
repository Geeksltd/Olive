namespace Olive.Entities.Replication
{
    public abstract class ReplicatedFullData<TDomain> : ReplicatedData<TDomain> where TDomain : IEntity
    {
        protected internal override void Define() => ExportAll();
    }
}