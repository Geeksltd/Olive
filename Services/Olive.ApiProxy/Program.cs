using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Olive.ApiProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            ProxyDLLGenerator.AssemblyFile = new FileInfo(Param(args, "assembly"));
            ProxyDLLGenerator.PublisherService = Param(args, "serviceName");
            ProxyDLLGenerator.ControllerName = Param(args, "controller");
            ProxyDLLGenerator.Output = new DirectoryInfo(Param(args, "output"));

            try
            {
                Console.WriteLine("Generating Client SDK proxy from...");
                Console.WriteLine("Publisher service: " + ProxyDLLGenerator.PublisherService);
                Console.WriteLine("Api assembly: " + ProxyDLLGenerator.AssemblyFile);
                Console.WriteLine("Api Controller: " + ProxyDLLGenerator.ControllerName);
                Console.WriteLine("---------------------");

                ProxyDLLGenerator.Generate();
                Console.WriteLine("Add done");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var fileName = args.Name.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x))
               .FirstOrDefault()?.ToLower();

            if (string.IsNullOrEmpty(fileName)) return null;
            fileName = fileName.ToLower();
            if (!fileName.EndsWith(".dll")) fileName += ".dll";

            var file = Path.Combine(ProxyDLLGenerator.AssemblyFile.Directory.FullName, fileName);

            if (File.Exists(file)) return Assembly.LoadFile(file);
            else throw new Exception("File not found: " + file);
        }

        static string Param(string[] args, string key)
        {
            var decorateKey = "/" + key + ":";
            var result = args.FirstOrDefault(x => x.StartsWith(decorateKey)) ?? string.Empty;
            if (result != null) result = result.Substring(decorateKey.Length);

            if (string.IsNullOrEmpty(result))
                throw new Exception("Expected parameter is not provided: " + decorateKey);

            return result;
        }
    }
}


