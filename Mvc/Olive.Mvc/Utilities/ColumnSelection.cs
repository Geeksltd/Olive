using Microsoft.AspNetCore.Mvc.Rendering;
using Olive.Web;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public class ColumnSelection
    {
        List<string> current;
        string cookieKey, Prefix;

        public ColumnSelection(string prefix = null) => Prefix = prefix;

        public List<string> Options { get; set; }

        public IEnumerable<SelectListItem> GetListOptions()
        {
            var result = new List<SelectListItem>();
            result.AddRange(Options);
            return result;
        }

        public IEnumerable<string> Current
        {
            get
            {
                if (current == null)
                {
                    var fromCookie = Task.Factory.RunSync(() => CookieProperty.Get(CookieKey));
                    SetSelection(fromCookie.OrEmpty().Split('|'));
                }

                return current;
            }
        }

        public List<string> Default { get; set; }

        [ReadOnly(true)]
        public string CookieKey
        {
            get
            {
                if (cookieKey == null)
                    cookieKey = Context.Current.ActionContext().RouteData.Values["action"] +
                        Prefix.WithPrefix(".") + ".Columns";

                return cookieKey;
            }
            set
            {
                cookieKey = value;
            }
        }

        public bool Contains(string column) => current.Contains(column);

        internal void SetSelection(IEnumerable<string> selectedColumns)
        {
            selectedColumns = selectedColumns.Or(new string[0]).Intersect(Options);

            current = selectedColumns.Or(Default).ToList();

            if (current.IsEquivalentTo(Default))
                CookieProperty.Remove(CookieKey);
            else
                CookieProperty.Set(CookieKey, current.ToString("|"));
        }
    }
}