namespace Olive.Entities
{
    [LogEvents(false)]
    [CacheObjects(false)]
    public interface IApplicationEvent : IEntity
    {
        string Data { get; set; }
        DateTime Date { get; set; }
        string Event { get; set; }
        string IP { get; set; }
        string ItemKey { get; set; }
        string ItemType { get; set; }
        string UserId { get; set; }
    }
}
