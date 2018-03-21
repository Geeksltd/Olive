namespace Olive.Entities
{
    public class StringEntity : Entity<string>
    {
        public static bool operator !=(StringEntity entity, string id) => entity?.ID != id;

        public static bool operator ==(StringEntity entity, string id) => entity?.ID == id;

        public static bool operator !=(string id, StringEntity entity) => entity?.ID != id;

        public static bool operator ==(string id, StringEntity entity) => entity?.ID == id;

        public override bool Equals(Entity other) => GetType() == other?.GetType() && ID == (other as StringEntity)?.ID;

        public override bool Equals(object other) => Equals(other as Entity);

        public override int GetHashCode() => ID?.GetHashCode() ?? 0;
    }
}