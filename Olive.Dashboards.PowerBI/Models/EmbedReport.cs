namespace Olive.Dashboards.PowerBI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class EmbedReport
    {
        // Id of Power BI to be embedded
        public Guid Id { get; set; }

        // Name of the report
        public string Name { get; set; }

        // Embed Url for the Power BI report
        public string EmbedUrl { get; set; }
    }
}
