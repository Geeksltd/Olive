using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Olive.Entities;

namespace Olive.Audit
{
    public class EntityProcessor
    {
        static IDatabase Database => Context.Current.Database();

        /// <summary>
        /// Gets the changes XML for a specified object. That object should be in its OnSaving event state.
        /// </summary>
        public static async Task<string> GetChangesXml(IEntity entityBeingSaved)
        {
            var original = await Database.Get(entityBeingSaved.GetId(), entityBeingSaved.GetType());
            var changes = Database.GetProvider(entityBeingSaved.GetType()).GetUpdatedValues(original, entityBeingSaved);

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
                                                            .Except(x => CalculatedAttribute.IsCalculated(x) || ComputedColumnAttribute.IsComputedColumn(x))
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
                    else if (propertyType == typeof(Blob))
                    {
                        var blob = p.GetValue(entity) as Blob;
                        propertyValue = blob.IsEmpty() ? null : blob.FileName;
                    }
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
        public static async Task<IEntity> LoadItem(IAuditEvent applicationEvent, bool newItem = false)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(applicationEvent.ItemType))
                .ExceptNull().FirstOrDefault();

            if (type == null)
                throw new("Could not load the type " + applicationEvent.ItemType);

            if (applicationEvent.Event.IsAnyOf("Update", "Insert"))
            {
                var item = await Context.Current.Database().GetOrDefault(applicationEvent.ItemId.To<Guid>(), type);

                if (item == null)
                    return MapAuditToInstance(applicationEvent, type, newItem);

                return item;
            }

            if (applicationEvent.Event == "Delete")
                return MapAuditToInstance(applicationEvent, type, newItem);

            throw new NotSupportedException();
        }

        static GuidEntity MapAuditToInstance(IAuditEvent applicationEvent, Type type, bool newItem)
        {
            var item = Activator.CreateInstance(type) as GuidEntity;
            item.ID = applicationEvent.ItemId.To<Guid>();

            string parentNode;

            if (applicationEvent.Event == "Insert") parentNode = null;
            else if (applicationEvent.Event == "Update" && newItem) parentNode = "new";
            else parentNode = "old";

            var elements = XElement.Parse(applicationEvent.ItemData).Elements();
            elements = parentNode.HasValue() ? elements.FirstOrDefault(x => x.Name.LocalName == parentNode)?.Elements() : elements;

            foreach (var element in elements.ToArray())
            {
                var eValue = element.Value;
                var property = type.GetProperty(element.Name.LocalName);

                if (property.IsEntity(item)) continue;
                else if (property.PropertyType == typeof(Blob))
                {
                    property.SetValue(item, (new Blob()).Attach(item, property.Name));
                }
                else
                {
                    property.SetValue(item, eValue.To(property.PropertyType));
                }
            }

            return item;
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

            return Database.GetProvider(original.GetType()).GetUpdatedValues(original, updated);
        }
    }
}