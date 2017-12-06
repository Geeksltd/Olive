namespace Olive.Entities
{
    public interface IHierarchy : IEntity
    {
        IHierarchy GetParent();

        IEnumerable<IHierarchy> GetChildren();

        string Name { get; }
    }
}
