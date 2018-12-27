namespace Olive.Entities.Replication
{
    public abstract class NakedExposedType<TDomain> : ExposedType<TDomain> where TDomain : class, IEntity
    {
        public override void Define() => ExposeEverything();
    }
}