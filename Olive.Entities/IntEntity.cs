namespace Olive.Entities
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;

    public class IntEntity : Entity<int>
    {
        bool IsIdLoaded;
        int id;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Func<Type, Task<int>> NewIdGenerator = DefaultNewIdGenerator;

        /// <summary>
        /// Gets a unique Identifier for this instance. In the database, 
        /// this will be the primary key of this object.
        /// </summary>
        public override int ID
        {
            get
            {
                if (!IsIdLoaded)
                    if (GetType().Defines<IdByDatabaseAttribute>(inherit: true))
                            throw new InvalidOperationException($"ID is not avialable for instances of '{GetType().Name}' before being saved to the database.");

                return id;
            }
            set
            {
                if (IsNew && GetType().Defines<IdByDatabaseAttribute>(inherit: true))
                    throw new InvalidOperationException($"ID is not avialable for instances of '{GetType().Name}' before being saved to the database.");

                id = value;
                IsIdLoaded = true;
            }
        }

        static ConcurrentDictionary<Type, int> LastUsedIds = new ConcurrentDictionary<Type, int>();

        static async Task<int> DefaultNewIdGenerator(Type type)
        {
            // One generator per hierarchy
            if (type.BaseType != typeof(IntEntity))
                return await DefaultNewIdGenerator(type.BaseType);

            async Task<int> initialize(Type typ)
            {
                if (TransientEntityAttribute.IsTransient(typ)) return 1;

                var biggestId = await Context.Current.Database()
                    .Of(typ).OrderBy("ID", descending: true)
                .Top(1).GetList().Select(x => x.GetId()).FirstOrDefault();

                if (biggestId != null) return 1 + (int)biggestId;
                else return 1;
            }

            var value = await initialize(type);

            return LastUsedIds.AddOrUpdate(type, value, (t, old) => old + 1);
        }

        public static bool operator !=(IntEntity entity, int? id) => entity?.ID != id;

        public static bool operator ==(IntEntity entity, int? id) => entity?.ID == id;

        public static bool operator !=(IntEntity entity, int id) => entity?.ID != id;

        public static bool operator ==(IntEntity entity, int id) => entity?.ID == id;

        public static bool operator !=(int? id, IntEntity entity) => entity?.ID != id;

        public static bool operator ==(int? id, IntEntity entity) => entity?.ID == id;

        public static bool operator !=(int id, IntEntity entity) => entity?.ID != id;

        public static bool operator ==(int id, IntEntity entity) => entity?.ID == id;

        public override bool Equals(Entity other) => GetType() == other?.GetType() && ID == (other as IntEntity)?.ID;

        public override bool Equals(object other) => Equals(other as Entity);

        public override int GetHashCode() => ID.GetHashCode();
    }
}