namespace Olive.Entities.Replication
{
    public abstract class ReplicatedFullData<TDomain> : ReplicatedData<TDomain> where TDomain : IEntity
    {
        public override void Define() => ExportAll();
    }
}