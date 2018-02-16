using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Olive.Web;

namespace Olive.Mvc
{
    public class ColumnSelection
    {
        List<string> Current;
        string cookieKey, Prefix;

        public ColumnSelection(string prefix = null) => Prefix = prefix;

        public List<string> Options { get; set; }

        public IEnumerable<SelectListItem> GetListOptions()
        {
            var result = new List<SelectListItem>();
            result.AddRange(Options);
            return result;
        }

        public async Task<List<string>> GetCurrent()
        {
            if (Current != null) return Current;

            SetSelection((await CookieProperty.Get(CookieKey)).OrEmpty().Split('|'));

            return Current;
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

        public bool Contains(string column) => Current.Contains(column);

        internal void SetSelection(IEnumerable<string> selectedColumns)
        {
            selectedColumns = selectedColumns.Or(new string[0]).Intersect(Options);

            Current = selectedColumns.Or(Default).ToList();

            if (Current.IsEquivalentTo(Default))
                CookieProperty.Remove(CookieKey);
            else
                CookieProperty.Set(CookieKey, Current.ToString("|"));
        }
    }
}