using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities
{
    /// <summary>
    /// Provides Sorting services for all entities.
    /// </summary>
    public static class Sorter
    {
        public const int INCREMENT = 10;
        static AsyncLock AsyncLock = new AsyncLock();
        static IDatabase Database => Context.Current.Database();

        public static async Task<ISortable> FindItemAbove(ISortable item) =>
            (await FindSiblings(item)).Except(item).Where(o => o.Order <= item.Order).WithMax(o => o.Order);

        public static async Task<ISortable> FindItemBelow(ISortable item) =>
            (await FindSiblings(item)).Except(item).Where(i => i.Order >= item.Order).WithMin(i => i.Order);

        public static bool CanMoveUp(ISortable item) => FindItemAbove(item) != null;

        public static bool CanMoveDown(ISortable item) => FindItemBelow(item) != null;

        /// <summary>
        /// Moves this item before a specified other item. If null is specified, it will be moved to the end of its siblings.
        /// </summary>
        public static async Task MoveBefore(ISortable item, ISortable before, SaveBehaviour saveBehaviour = SaveBehaviour.Default)
        {
            var newOrder = (before == null ? int.MaxValue : before.Order) - 1;

            if (newOrder < 0) newOrder = 0;

            item = await Database.Update(item, o => o.Order = newOrder, saveBehaviour);

            await JustifyOrders(item, saveBehaviour);
        }

        /// <summary>
        /// Moves this item after a specified other item. If null is specified, it will be moved to the beginning of its siblings.
        /// </summary>
        public static async Task MoveAfter(ISortable item, ISortable after, SaveBehaviour saveBehaviour = SaveBehaviour.Default)
        {
            var newOrder = (after == null ? 0 : after.Order) + 1;

            item = await Database.Update(item, o => o.Order = newOrder, saveBehaviour);

            await JustifyOrders(item, saveBehaviour);
        }

        /// <summary>
        /// Moves an item up among its siblings. Returns False if the item is already first in the list, otherwise true.
        /// </summary>
        public static async Task<bool> MoveUp(ISortable item, SaveBehaviour saveBehaviour = SaveBehaviour.Default)
        {
            using (await AsyncLock.Lock())
            {
                var above = await FindItemAbove(item);

                if (above == null) return false;

                if (above.Order == item.Order) above.Order--;

                await Swap(item, above, saveBehaviour);

                item = await Database.Reload(item);
                above = await Database.Reload(above);
            }

            await JustifyOrders(item, saveBehaviour);

            return true;
        }

        /// <summary>
        /// Moves an item up to first among its siblings. Returns False if the item is already first in the list, otherwise true.
        /// </summary>
        public static async Task<bool> MoveFirst(ISortable item, SaveBehaviour saveBehaviour = SaveBehaviour.Default)
        {
            using (await AsyncLock.Lock())
            {
                var first = (await FindSiblings(item)).Min(o => o.Order);

                if (first <= 0) return false;

                await Database.Update(item, o => o.Order = first - 1, saveBehaviour);
            }

            await JustifyOrders(item, saveBehaviour);
            return true;
        }

        /// <summary>
        /// Moves an item up to last among its siblings. Always returns true.
        /// </summary>
        public static async Task<bool> MoveLast(ISortable item, SaveBehaviour saveBehaviour = SaveBehaviour.Default)
        {
            using (await AsyncLock.Lock())
            {
                var last = (await FindSiblings(item)).Max(o => o.Order);

                await Database.Update(item, o => o.Order = last + 1, saveBehaviour);
            }

            await JustifyOrders(item, saveBehaviour);
            return true;
        }

        /// <summary>
        /// Moves an item down among its siblings. Returns False if the item is already last in the list, otherwise true.
        /// </summary>
        public static async Task<bool> MoveDown(ISortable item, SaveBehaviour saveBehaviour = SaveBehaviour.Default)
        {
            using (await AsyncLock.Lock())
            {
                var below = await FindItemBelow(item);

                if (below == null) return false;

                if (below.Order == item.Order) item.Order++;

                await Swap(item, below, saveBehaviour);
            }

            await JustifyOrders(item, saveBehaviour);

            return true;
        }

        /// <summary>
        /// Swaps the order of two specified items.
        /// </summary>
        static async Task Swap(ISortable one, ISortable two, SaveBehaviour saveBehaviour)
        {
            var somethingAboveAll = (await FindSiblings(one)).Max(i => i.Order) + 20;

            await Database.EnlistOrCreateTransaction(async () =>
            {
                var order1 = two.Order;
                var order2 = one.Order;

                await Database.Update(one, i => i.Order = order1, saveBehaviour);
                await Database.Update(two, i => i.Order = order2, saveBehaviour);
            });
        }

        /// <summary>
        /// Justifies the order of a specified item and its siblings. 
        /// The value of the "Order" property in those objects will be 10, 20, 30, ...
        /// </summary>
        public static async Task JustifyOrders(ISortable item, SaveBehaviour saveBehaviour = SaveBehaviour.Default)
        {
            using (await AsyncLock.Lock())
            {
                var changed = new List<Entity>();

                var order = 0;

                foreach (var sibling in (await FindSiblings(item)).OrderBy(i => i.Order).Distinct().ToArray())
                {
                    order += INCREMENT;
                    if (sibling.Order == order) continue;

                    var clone = sibling.Clone() as Entity;
                    (clone as ISortable).Order = order;
                    changed.Add(clone);
                }

                await Database.Save(changed, saveBehaviour);
            }
        }

        /// <summary>
        /// Discovers the siblings of the specified sortable object.
        /// </summary>
        static async Task<IEnumerable<ISortable>> FindSiblings(ISortable item)
        {
            var getSiblingsMethod = item.GetType().GetMethod("GetSiblings", BindingFlags.Public | BindingFlags.Instance);

            IEnumerable<ISortable> result;

            if (!IsAcceptable(getSiblingsMethod))
            {
                result = (await Database.Of(item.GetType()).GetList()).Cast<ISortable>();
            }
            else
            {
                var list = new List<ISortable>();

                try
                {
                    IEnumerable collection;
                    if (getSiblingsMethod.ReturnType.IsA<Task>())
                        collection = await (dynamic)getSiblingsMethod.Invoke(item, null) as IEnumerable<ISortable>;
                    else
                        collection = getSiblingsMethod.Invoke(item, null) as IEnumerable;

                    foreach (ISortable element in collection)
                        list.Add(element);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Services.Sorter Could not process the GetSiblings method from the {item.GetType().Name} instance.", ex);
                }

                result = list;
            }

            return result.OrderBy(i => i.Order).ToList();
        }

        static bool IsAcceptable(MethodInfo getSiblingsMethod)
        {
            if (getSiblingsMethod == null) return false;
            if (getSiblingsMethod.GetParameters().Any()) return false;

            var returnType = getSiblingsMethod.ReturnType;

            if (returnType.Implements<IEnumerable>()) return true;

            if (returnType.IsA<Task>() &&
                returnType.IsGenericType &&
                returnType.GetGenericArguments().Single().Implements<IEnumerable>())
                return true;

            return false;
        }

        /// <summary>
        /// Gets the Next order for an ISortable entity.
        /// The result will be 10 plus the largest order of its siblings.
        /// </summary>
        public static async Task<int> GetNewOrder(ISortable item)
        {
            using (await AsyncLock.Lock())
            {
                if (!item.IsNew)
                    throw new ArgumentException("Sorter.GetNewOrder() method needs a new ISortable argument (with IsNew set to true).", nameof(item));

                return INCREMENT + ((await FindSiblings(item)).LastOrDefault()?.Order ?? 0);
            }
        }
    }
}