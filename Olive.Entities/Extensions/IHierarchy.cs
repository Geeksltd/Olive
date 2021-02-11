using Olive.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Olive
{
    partial class OliveEntitiesExtensions
    { /// <summary>
      /// Gets the full path of this hirarchical entity, seperated by " > ".
      /// </summary>
        public static string GetFullPath(this IHierarchy node) => node.GetFullPath(" > ");

        /// <summary>
        /// Gets whether this node is a root hierarchy node.
        /// </summary>
        public static bool IsRootNode(this IHierarchy node) => node.GetParent() == null;

        /// <summary>
        /// Gets the full path of this hirarchical entity, seperated by a specified seperation string.
        /// </summary>
        public static string GetFullPath(this IHierarchy hierarchy, string seperator)
        {
            if (hierarchy == null) return null;
            if (hierarchy.GetParent() == null || hierarchy.GetParent() == hierarchy)
                return hierarchy.Name;
            else return hierarchy.GetParent().GetFullPath(seperator) + seperator + hierarchy.Name;
        }

        /// <summary>
        /// Gets this node as well as all its children hierarchy.
        /// </summary>
        public static IEnumerable<IHierarchy> WithAllChildren(this IHierarchy parent) =>
            parent.GetAllChildren().Concat(parent).OrderBy(i => i.GetFullPath()).ToArray();

        /// <summary>
        /// Gets all children hierarchy of this node.
        /// </summary>
        public static IEnumerable<IHierarchy> GetAllChildren(this IHierarchy @this)
        {
            return @this.GetChildren()
                .Except(@this)
                .SelectMany(c => c.WithAllChildren())
                .OrderBy(i => i.GetFullPath())
                .ToArray();
        }

        /// <summary>
        /// Gets this node as well as all its parents hierarchy.
        /// </summary>
        public static IEnumerable<IHierarchy> WithAllParents(this IHierarchy child) =>
            child.GetAllParents().Concat(child).OrderBy(i => i.GetFullPath()).ToArray();

        /// <summary>
        /// Gets all parents hierarchy of this node.
        /// </summary>
        public static IEnumerable<IHierarchy> GetAllParents(this IHierarchy child)
        {
            var parent = child.GetParent();

            if (parent == null || parent == child) return new IHierarchy[0];
            else return parent.WithAllParents().OrderBy(i => i.GetFullPath()).ToArray();
        }

        /// <summary>
        /// Gets this node as well as all its parents hierarchy.
        /// </summary>
        public static IEnumerable<T> WithAllParents<T>(this T child) where T : IHierarchy =>
            (child as IHierarchy).WithAllParents().Cast<T>().ToArray();

        /// <summary>
        /// Gets all parents hierarchy of this node.
        /// </summary>
        public static IEnumerable<T> GetAllParents<T>(this IHierarchy child) where T : IHierarchy =>
            (child as IHierarchy).GetAllParents().Cast<T>().ToArray();
    }
}