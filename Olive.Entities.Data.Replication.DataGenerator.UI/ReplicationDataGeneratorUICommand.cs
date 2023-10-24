namespace Olive.Data.Replication.DataGenerator.UI
{
    using Olive.Entities.Replication;
    using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Olive.Data.Replication.DataGenerator.UI.ModuleGenerators;
    using System.Net;
    using System.Reflection;

    public class ReplicationDataGeneratorUICommand : IDevCommand 
	{
		private DestinationEndpoint _Endpoint { get; set; }

		private string _TypeName { get; set; }
        private string _NameSpace { get; set; }
        public ReplicationDataGeneratorUICommand(string nameSpace, string typeName)
		{
			_TypeName = typeName;
			_NameSpace = nameSpace;
		}
		public void SetEndpoint(DestinationEndpoint endpoint) => _Endpoint = endpoint;

        public string Name => $"generate-{_NameSpace}-{_TypeName}";

		public string Title => $"generate-{_TypeName}";
        HttpRequest Request => Context.Current.Request();

        HttpResponse Response => Context.Current.Response();
        string Action => Request.Param("action").ToStringOrEmpty().ToLower();

        public bool IsEnabled() => _Endpoint != null;
        public async Task<string> Run()
		{
            var domainType = _Endpoint.GetSubscriber(_TypeName)?.DomainType;

            string content = string.Empty;

            if(Request.IsPost())
                content = await new FormSubmitGenerator(domainType, _Endpoint).Render(_NameSpace, _TypeName);

            else if (Action == "new")
                content =  await new FormModuleGenerator(domainType, _Endpoint).Render(_NameSpace, _TypeName);
            else
                content =  await new ListModuleGenerator(domainType, _Endpoint).Render(_NameSpace, _TypeName);
           await Response.WriteAsync(content);

            return content;

		}

	}
}
