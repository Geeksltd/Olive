using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Olive.ApiProxy
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Contains("/debug"))
            {
                Console.Write("Waiting for debugger to attach...");
                while (!Debugger.IsAttached) Thread.Sleep(100);
                Console.WriteLine("Attached.");
            }

            if (!ParametersParser.Start(args)) return Helper.ShowHelp();

            try
            {
                ParametersParser.LoadParameters();

                Console.WriteLine("Generating Client SDK proxy from...");
                Console.WriteLine("Publisher service: " + Context.PublisherService);
                Console.WriteLine("Api assembly: " + Context.AssemblyFile);
                Console.WriteLine("Api controller: " + Context.ControllerName);
                Console.WriteLine("Temp folder: " + Context.TempPath);
                Console.WriteLine("------------------------");
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