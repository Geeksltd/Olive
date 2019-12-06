using Domain;
using Olive;
using Olive.Mvc;
using System.Linq;

namespace ViewModel
{
    partial class GlobalSearch
    {
        public string GetSearchSources()
        {
            var olive = "Hub,Tasks,People".Split(',')
                .Select(Service.FindByName)
                .Select(s => $"{s.GetAbsoluteImplementationUrl("/api/global-search")}#{s.Icon}");

            var webForms = "Accounting,Dashboard,HR,CRM,CaseStudies,Training".Split(',')
                .Select(Service.FindByName)
                .Select(s => $"{s.GetAbsoluteImplementationUrl("/global-search.axd")}#{s.Icon}");

            return olive.Concat(webForms).ToString(";");
        }
    }
}