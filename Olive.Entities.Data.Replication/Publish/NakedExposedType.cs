namespace Olive.Entities.Replication
{
    public abstract class NakedExposedType<TDomain> : ExposedType<TDomain> where TDomain : IEntity
    {
        public override void Define() => ExposeEverything();
    }
}