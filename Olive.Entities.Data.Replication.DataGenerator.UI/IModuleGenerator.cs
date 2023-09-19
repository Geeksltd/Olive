namespace Olive.Data.Replication.DataGenerator.UI
{
    using Olive.Entities;
    using System;
	using System.Collections.Generic;
	using System.Text;
    using System.Threading.Tasks;

    internal interface IModuleGenerator
	{
		string Action { get; }
		Task<string> Render(string nameSpace, string typeName);


	}
}
