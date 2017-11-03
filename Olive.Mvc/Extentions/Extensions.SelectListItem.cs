using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Olive;
using Olive.Entities;
using Olive.Services.Globalization;

namespace Olive.Mvc
{
    partial class OliveMvcExtensions
    {
        public static void AddRange<T>(this IList<SelectListItem> listItems, IEnumerable<T> items, Func<T, object> displayExpression) where T : IEntity
        {
            foreach (var item in items)
                listItems.Add(new SelectListItem
                {
                    Text = displayExpression(item).ToStringOrEmpty(),
                    Value = item.GetId().ToString()
                });
        }

        public static void AddRange<T>(this IList<SelectListItem> listItems, IEnumerable<T> items, T selectedItem, Func<T, object> displayExpression) where T : IEntity
        {
            foreach (var item in items)
            {
                if (item == null) continue;

                listItems.Add(new SelectListItem
                {
                    Text = displayExpression(item).ToStringOrEmpty(),
                    Value = item.GetId().ToString(),
                    Selected = item.Equals(selectedItem)
                });
            }
        }

        public static void AddRange<T>(this IList<SelectListItem> listItems, IEnumerable<T> items,
            Func<T, object> displayExpression,
            Func<T, object> valueExpression)
        {
            foreach (var item in items)
            {
                var value = valueExpression(item);

                if (value is IEntity) value = (value as IEntity).GetId();

                listItems.Add(new SelectListItem
                {
                    Text = displayExpression(item).ToStringOrEmpty(),
                    Value = value.ToStringOrEmpty()
                });
            }
        }

        public static void AddRange(this IList<SelectListItem> listItems, IEnumerable items) =>
            AddRange(listItems, items, (IEnumerable)null);

        public static void AddRange(this IList<SelectListItem> listItems, IEnumerable items, IEntity selectedItem) =>
            AddRange(listItems, items, new[] { selectedItem });

        public static void AddRange<T>(this IList<SelectListItem> listItems, IEnumerable<T> items, IEnumerable<T> selectedItems, Func<T, object> displayExpression) where T : IEntity
        {
            foreach (var item in items)
            {
                if (item == null) continue;

                listItems.Add(new SelectListItem
                {
                    Text = displayExpression(item).ToString(),
                    Value = item.GetId().ToString(),
                    Selected = selectedItems != null && selectedItems.Contains(item)
                });
            }
        }

        //public static async Task Add(this IList<SelectListItem> items, IEntity entity, ILanguage language)
        //{
        //    items.Add(new SelectListItem
        //    {
        //        Text = await entity.ToString(language),
        //        Value = entity.GetId().ToString()
        //    });
        //}

        public static void Add(this IList<SelectListItem> items, IEntity entity) =>
            items.Add(new SelectListItem { Text = entity.ToString(), Value = entity.GetId().ToString() });

        public static void Add(this IList<SelectListItem> items, object text, object value) =>
            items.Add(new SelectListItem { Text = text.ToStringOrEmpty(), Value = value.ToStringOrEmpty() });

        public static void AddRange(this IList<SelectListItem> listItems, IEnumerable items, IEnumerable selectedItems)
        {
            var selected = selectedItems?.OfType<object>();

            foreach (var item in items)
            {
                if (item == null) continue;
                else if (item is IEntity) listItems.Add((IEntity)item);
                else if (item is SelectListItem) listItems.Add((SelectListItem)item);
                else listItems.Add(new SelectListItem { Text = item.ToStringOrEmpty(), Value = item.ToStringOrEmpty() });

                if (selected != null && selected.Contains(item))
                    listItems[listItems.Count - 1].Selected = true;
            }
        }

        public static string GetSelected(this SelectListItem item)
        {
            if (item.Selected) return "selected=\"selected\"";
            else return null;
        }

        public static List<SelectListItem> Clone(this IEnumerable<SelectListItem> items)
        {
            return items.Select(x => new SelectListItem
            {
                Text = x.Text,
                Selected = x.Selected,
                Value = x.Value,
                Group = x.Group,
                Disabled = x.Disabled
            }).ToList();
        }

        public static void SetSelected(this IEnumerable<SelectListItem> items, params string[] selectedValues)
        {
            if (selectedValues.None()) return;

            items.Where(x => selectedValues.Contains(x.Value.Or(x.Text))).Do(x => x.Selected = true);
        }

        public static List<SelectListItem> ToSelectListItems(this IEnumerable<IEntity> items)
        {
            var result = new List<SelectListItem>();
            result.AddRange(items);

            return result;
        }

        public static void AddRange<TEnum>(this List<SelectListItem> items) where TEnum : struct, IConvertible
        {
            var type = typeof(TEnum);

            if (!type.IsEnum) throw new ArgumentException("TEnum must be an enumerated type");

            foreach (var item in Enum.GetValues(type))
                items.Add(item.ToString(), (int)item);
        }
    }
}