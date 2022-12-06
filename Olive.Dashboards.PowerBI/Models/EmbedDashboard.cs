namespace Olive.Dashboards.PowerBI.Models
{
    using Microsoft.PowerBI.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class EmbedDashboard
    {
        // Id of Power BI to be embedded
        public Guid Id { get; set; }

        // Name of the dashboard
        public string Name { get; set; }

        // Embed Url for the Power BI dashboard
        public string EmbedUrl { get; set; }
    }
}
