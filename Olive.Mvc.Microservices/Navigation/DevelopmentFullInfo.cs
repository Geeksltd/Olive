using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc.Microservices
{
    internal class DevelopmentFullInfo
    {
        internal Service Service { set; get; }
        internal Feature[] Features { set; get; }
        internal string[] BoardSources { set; get; }
        internal bool GlobalySearchable { set; get; }
    }
}
