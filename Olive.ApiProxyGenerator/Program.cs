using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Olive.ApiProxy
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Current directory: " + Environment.CurrentDirectory);

            if (args.Contains("/debug"))
            {
                Console.Write("Waiting for debugger to attach...");
                while (!Debugger.IsAttached) Thread.Sleep(100);
                Console.WriteLine("Attached.");
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            if (!ParametersParser.Start(args)) return Helper.ShowHelp();

            try
            {
                ParametersParser.LoadParameters();

                Console.WriteLine("Generating Client SDK proxy from...");
                Console.WriteLine("Publisher service: " + Context.PublisherService);
                Console.WriteLine("Api assembly: " + Context.AssemblyFile);
                Console.WriteLine("Api controller: " + Context.ControllerName);
                Console.WriteLine("Temp folder: " + Context.TempPath);
                Context.LoadAssembly();
                Context.PrepareOutputDirectory();
                DtoTypes.FindAll();
                DtoDataProviderClassGenerator.ValidateRemoteDataProviderAttributes();

                new List<ProjectCreator> { new ProxyProjectCreator() };

                var proxyCreator = new ProxyProjectCreator();
                proxyCreator.Build();
                new NugetCreator(proxyCreator).Create();

                if (DtoTypes.All.Any())
                {
                    var projectCreators = new[] { new MSharpProjectCreator(), new MSharp46ProjectCreator() };
                    projectCreators.AsParallel().Do(x => x.Build());
                    new NugetCreator(projectCreators).Create();
                }

                Console.WriteLine("Add done");
                return 0;
            }
            catch (Exception ex)
            {
                ShowError(ex);
                return -1;
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var fileName = args.Name.Split(',').Select(x => x.Trim())
                .FirstOrDefault(x => x.HasValue())?.ToLower();

            if (fileName.IsEmpty()) return null;
            else fileName = fileName.ToLower();

            if (!fileName.EndsWith(".dll")) fileName += ".dll";

            var file = Path.Combine(Environment.CurrentDirectory, fileName);
            if (File.Exists(file))
            {
                return Assembly.LoadFile(file);
            }
            else
            {
                Console.WriteLine("Not found: " + file);
                return null;
            }
        }

        public static void ShowError(Exception error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR!");
            Console.WriteLine(error.Message);
            Console.ResetColor();
            Console.WriteLine(error.GetUsefulStack());
            Console.WriteLine("Press any key to end, or rerun the command with /debug.");
            Console.ReadKey();
        }
    }
}