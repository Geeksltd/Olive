using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Olive.Entities;
using System.Xml.Linq;
using System.Reflection;

namespace Olive.Audit
{
    public class EntityProcessor
    {
        /// <summary>
        /// Gets the changes XML for a specified object. That object should be in its OnSaving event state.
        /// </summary>
        public static async Task<string> GetChangesXml(IEntity entityBeingSaved)
        {
            var original = await Entity.Database.Get(entityBeingSaved.GetId(), entityBeingSaved.GetType());
            var changes = Entity.Database.GetProvider(entityBeingSaved.GetType()).GetUpdatedValues(original, entityBeingSaved);

            return ToChangeXml(changes);
        }

        public static string GetDataXml(IEntity record)
        {
            var data = GetDataToLog(record);
            return new XElement("Data", data.Select(kv => new XElement(kv.Key, kv.Value))).ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Gets the data of a specified object's properties in a dictionary.
        /// </summary>
        public static Dictionary<string, string> GetDataToLog(IEntity entity)
        {
            var result = new Dictionary<string, string>();

            var type = entity.GetType();
            var propertyNames = type.GetProperties().Select(p => p.Name).Distinct().Trim().ToArray();

            Func<string, PropertyInfo> getProperty = name => type.GetProperties()
            .Except(p => p.IsSpecialName)
            .Except(p => p.GetGetMethod().IsStatic)
            .Except(p => p.Name == "ID")
            .Where(p => p.GetSetMethod() != null && p.GetGetMethod().IsPublic)
            .OrderByDescending(x => x.DeclaringType == type)
            .FirstOrDefault(p => p.Name == name);

            var dataProperties = propertyNames.Select(getProperty).ExceptNull()
                                                            .Except(x => CalculatedAttribute.IsCalculated(x))
                                                            .Where(x => LogEventsAttribute.ShouldLog(x))
                                                            .ToArray();

            foreach (var p in dataProperties)
            {
                var propertyType = p.PropertyType;

                string propertyValue;

                try
                {
                    if (propertyType == typeof(IList<Guid>))
                        propertyValue = (p.GetValue(entity) as IList<Guid>).ToString(",");
                    else if (propertyType.IsGenericType)
                        propertyValue = (p.GetValue(entity) as IEnumerable<object>).ToString(", ");
                    else
                        propertyValue = p.GetValue(entity).ToStringOrEmpty();

                    if (propertyValue.IsEmpty()) continue;
                }
                catch
                {
                    // No log needed
                    continue;
                }

                result.Add(p.Name, propertyValue);
            }

            return result;
        }

        public static string ToChangeXml(IDictionary<string, Tuple<string, string>> changes)
        {
            if (changes.None()) return null;

            var r = new StringBuilder("<DataChange>");

            r.Append("<old>");

            foreach (var key in changes.Keys)
                r.AppendFormat("<{0}>{1}</{0}>", key, changes[key].Item1.XmlEncode());

            r.Append("</old>");

            r.Append("<new>");
            foreach (var key in changes.Keys)
            {
                var value = changes[key];
                r.AppendFormat("<{0}>{1}</{0}>", key, value.Item2.XmlEncode());
            }

            r.Append("</new>");
            r.Append("</DataChange>");
            return r.ToString();
        }

        /// <summary>
        /// Loads the item recorded in this event.
        /// </summary>
        public static async Task<IEntity> LoadItem(IAuditEvent applicationEvent)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(applicationEvent.ItemType)).ExceptNull().FirstOrDefault();

            if (type == null)
                throw new Exception("Could not load the type " + applicationEvent.ItemType);

            if (applicationEvent.Event == "Update" || applicationEvent.Event == "Insert")
                return await Entity.Database.Get(applicationEvent.ItemId.To<Guid>(), type);

            if (applicationEvent.Event == "Delete")
            {
                var result = Activator.CreateInstance(type) as GuidEntity;
                result.ID = applicationEvent.ItemId.To<Guid>();

                foreach (var p in XElement.Parse(applicationEvent.ItemData).Elements())
                {
                    var old = p.Value;
                    var property = type.GetProperty(p.Name.LocalName);
                    property.SetValue(result, old.To(property.PropertyType));
                }

                return result;
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the changes applied to the specified object.
        /// Each item in the result will be {PropertyName, { OldValue, NewValue } }.
        /// </summary>
        public virtual IDictionary<string, Tuple<string, string>> GetChanges(IEntity original, IEntity updated)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (updated == null) throw new ArgumentNullException(nameof(updated));
            if (updated.GetType() != original.GetType())
                throw new ArgumentException($"GetChanges() expects two instances of the same type, while {original.GetType().FullName} is not the same as {updated.GetType().FullName}.");

            return Entity.Database.GetProvider(original.GetType()).GetUpdatedValues(original, updated);
        }
    }
}
