using System;
using System.Linq;

namespace Olive.ApiProxy
{
    class Program
    {
        static int Main(string[] args)
        {
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

                new ProxyProjectCreator().Build();
                if (DtoTypes.All.Any()) new MSharpProjectCreator().Build();

                Console.WriteLine("Add done");
                return 0;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return -1;
            }
        }

        public static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR!");
            Console.WriteLine(message);
            Console.ResetColor();
            Console.WriteLine("Press any key to end...");
            Console.ReadLine();
        }
    }
}