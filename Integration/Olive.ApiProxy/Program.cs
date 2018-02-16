using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Olive.ApiProxy
{
    class Program
    {
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            if (!ParametersParser.LoadParameters(args)) return Helper.ShowHelp();

            if (!Context.AssemblyFile.Exists)
            {
                Console.WriteLine(Context.Assembly.FullName + " does not exist!");
                return -1;
            }

            try
            {
                Console.WriteLine("Generating Client SDK proxy from...");
                Console.WriteLine("Publisher service: " + Context.PublisherService);
                Console.WriteLine("Api assembly: " + Context.AssemblyFile);
                Console.WriteLine("Api controller class: " + Context.ControllerName);
                Console.WriteLine("Temp folder: " + Context.TempPath);
                Console.WriteLine("------------------------");
                Context.LoadAssembly();
                Context.PrepareOutputDirectory();
                DtoTypes.FindAll();
                new ProxyProjectCreator().Build();
                if (DtoTypes.All.Any()) new MSharpProjectCreator().Build();

                Console.WriteLine("Add done");
                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR!");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.WriteLine("Press any key to end...");
                Console.ReadLine();
                return -1;
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var fileName = args.Name.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x))
               .FirstOrDefault()?.ToLower();

            if (string.IsNullOrEmpty(fileName)) return null;
            fileName = fileName.ToLower();
            if (!fileName.EndsWith(".dll")) fileName += ".dll";

            var file = Path.Combine(Context.AssemblyFile.Directory.FullName, fileName);

            if (File.Exists(file)) return Assembly.LoadFile(file);
            else throw new Exception("File not found: " + file);
        }
    }
}