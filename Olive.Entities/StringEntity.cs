namespace Olive.Entities
{
    public class StringEntity : Entity<string>
    {
        public static bool operator !=(StringEntity entity, string id)
        {
            if (id is null) return !(entity is null);
            return entity?.ID != id;
        }

        public static bool operator ==(StringEntity entity, string id)
        {
            if (id is null) return entity is null;
            return entity?.ID == id;
        }

        public static bool operator !=(string id, StringEntity entity)
        {
            if (id is null) return !(entity is null);
            return entity?.ID != id;
        }

        public static bool operator ==(string id, StringEntity entity)
        {
            if (id is null) return entity is null;
            return entity?.ID == id;
        }

        public override bool Equals(Entity other)
        {
            if (other is StringEntity se)
                return GetType() == se.GetType() && ID == se.ID;
            else
                return false;
        }

        public override bool Equals(object other) => Equals(other as Entity);

        public override int GetHashCode() => ID?.GetHashCode() ?? 0;
    }
}