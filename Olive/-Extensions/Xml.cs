using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Gets an Element with the specified path. For example "Tree/Branch1/Branch2".
        /// </summary>
        public static XElement GetElement(this XContainer parent, string path) => FindNode(parent, path) as XElement;

        /// <summary>
        /// Gets a node with the specified path. For example "Tree/Branch1/Branch2".
        /// </summary>
        [Obsolete("Use FindNode instead")]
        public static XObject GetNode(this XContainer parent, string path) => parent.FindNode(path);

        /// <summary>
        /// Finds a node with the specified path. For example "Tree/Branch1/Branch2".
        /// </summary>
        public static XObject FindNode(this XContainer parent, string path)
        {
            if (path.IsEmpty())
                throw new ArgumentNullException(nameof(path));

            var node = parent;

            foreach (var part in path.Split('/'))
            {
                if (part.StartsWith("@"))
                {
                    // Attribute:
                    var element = node as XElement;
                    if (element == null) return null;
                    else
                    {
                        var attributeName = part.Substring(1);
                        var withXName = element.Attribute(attributeName);
                        if (withXName != null) return withXName;
                        else
                            return element.Attributes().FirstOrDefault(a => a.Name != null && a.Name.LocalName == attributeName);
                    }
                }
                else
                {
                    var withXName = node.Element(part);
                    if (withXName != null) node = withXName;
                    else
                    {
                        node = node.Elements().FirstOrDefault(e => e.Name != null && e.Name.LocalName == part);
                        if (node == null) return null;
                    }
                }
            }

            return node;
        }

        /// <summary>
        /// Gets the value of an attribute or inner text of an element with the specified path. For example "Tree/Branch1/Branch2".
        /// </summary>
        public static T GetValue<T>(this XContainer parent, string path)
        {
            string value = null;

            var node = parent.FindNode(path);

            if (node is XElement) value = (node as XElement).Value;
            else if (node is XAttribute) value = (node as XAttribute).Value;
            else if (node != null)
                throw new Exception("The provided path (" + path + ") points to an invalid Xml node (" + node.GetType() + ").");

            if (value.IsEmpty()) return default;

            if (typeof(T) == typeof(string)) return (T)(object)value;

            return value.To<T>();
        }

        /// <summary>
        /// Adds this node to a specified container and returns it back to be used as fluent API.
        /// </summary>
        public static T AddTo<T>(this T node, XContainer container) where T : XNode
        {
            container.Add(node);
            return node;
        }

        /// <summary>
        /// Removes all namespaces from this element.
        /// </summary>
        public static XElement RemoveNamespaces(this XElement node)
        {
            var result = new XElement(node.Name.LocalName);

            foreach (var attribute in node.Attributes())
                result.Add(new XAttribute(attribute.Name.LocalName, attribute.Value));

            if (node.HasElements)
                foreach (var child in node.Elements())
                    result.Add(child.RemoveNamespaces());
            else
                result.Value = node.Value;

            return result;
        }

        public static XmlReader ToXmlReader(this string xmlString) => XmlReader.Create(new StringReader(xmlString));

        public static string Serialize(this XmlSerializer serializer, object item)
        {
            using var textWriter = new StringWriter();
            serializer.Serialize(textWriter, item);
            return textWriter.ToString();
        }

        public static T Deserialize<T>(this XmlSerializer serializer, string xml)
        {
            using var reader = new StringReader(xml);
            return (T)serializer.Deserialize(reader);
        }

        // public static XmlElement ToXmlElement(this XElement element)
        // {
        //    var doc = new XmlDocument();
        //    doc.Load(element.CreateReader());
        //    return doc.DocumentElement;
        // }
    }
}