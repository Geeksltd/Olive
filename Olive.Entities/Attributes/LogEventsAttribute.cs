using System;
using System.Reflection;

namespace Olive.Entities
{
    /// <summary>
    /// When applied to a class, indicates whether data access events should be logged for instances of that type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public sealed class LogEventsAttribute : Attribute
    {
        static LogEventsAttribute @default;
        static LogEventsAttribute Default => @default ??= LoadDefault();

        public bool InsertAction { get; }
        public bool InsertData { get; }
        public bool UpdateAction { get; }
        public bool UpdateData { get; }
        public bool DeleteAction { get; }
        public bool DeleteData { get; }

        /// <summary>
        /// Creates a new LogEventsAttribute instance.
        /// </summary>
        public LogEventsAttribute(bool shouldLog) : this(shouldLog, shouldLog, shouldLog, shouldLog, shouldLog, shouldLog) { }

        /// <summary>
        /// Creates a new LogEventsAttribute instance.
        /// </summary>
        public LogEventsAttribute(bool insertAction = true,
            bool insertData = true,
            bool updateAction = true,
            bool updateData = true,
            bool deleteAction = true,
            bool deleteData = true)
        {
            InsertAction = insertAction;
            InsertData = insertData;
            UpdateAction = updateAction;
            UpdateData = updateData;
            DeleteAction = deleteAction;
            DeleteData = deleteData;
        }

        static bool Setting(string @event, string info) => Config.Get($"Database:Audit:{@event}:{info}", defaultValue: false);

        static LogEventsAttribute LoadDefault()
        {
            return new LogEventsAttribute(
                Setting("Insert", "Action"), Setting("Insert", "Data"),
                Setting("Update", "Action"), Setting("Update", "Data"),
                Setting("Delete", "Action"), Setting("Delete", "Data"));
        }

        public static LogEventsAttribute For(IEntity instance) => For(instance.GetType());

        public static LogEventsAttribute For(Type type)
        {
            return type.GetCustomAttribute<LogEventsAttribute>(inherit: true) ?? Default;
        }

        public static bool ShouldLog(PropertyInfo property) =>
            property.GetCustomAttribute<LogEventsAttribute>(inherit: true)?.InsertAction /*Does not matter which property*/ ?? true;
    }
}
