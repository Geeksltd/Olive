using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc.Microservices
{
    class MicroserviceJavascriptService : JavascriptService
    {
        public string ServiceConfigurationUrl { get; }

        public MicroserviceJavascriptService(string configuration, JavascriptService service) : base(service.ServiceKey, service.Function, service.Arguments)
        {
            ServiceConfigurationUrl = configuration;
        }
    }
}
