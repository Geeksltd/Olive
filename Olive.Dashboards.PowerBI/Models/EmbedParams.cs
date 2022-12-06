namespace Olive.Dashboards.PowerBI.Models
{
    using Microsoft.PowerBI.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class EmbedParams
    {
        // Type of the object to be embedded
        public string Type { get; set; }

        // Report to be embedded
        public List<EmbedReport> EmbedReport { get; set; }

        // Dashboard to be embedded
        public EmbedDashboard EmbedDashboard { get; set; }

        public EmbedToken EmbedToken { get; set; }
    }
}
