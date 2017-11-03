using Microsoft.AspNetCore.Mvc.Rendering;

namespace Olive.Mvc
{
    public class EmptyListItem : SelectListItem
    {
        public EmptyListItem() : this("---Select---") { }

        public EmptyListItem(string text)
        {
            Text = text;
            Value = string.Empty;
        }

        public EmptyListItem(string text, string value)
        {
            Text = text;
            Value = value.OrEmpty();
        }
    }
}