using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Olive.ApiProxy
{
    class Helper
    {
        static void ShowLine(string text, ConsoleColor color) => Show(text + Environment.NewLine, color);

        static void Show(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(" " + text);
            Console.ResetColor();
        }

        public static int ShowHelp()
        {
            Console.WriteLine("===================================");
            ShowLine("Usage pattern 1:", ConsoleColor.Red);
            Show("   dotnet ", ConsoleColor.White);
            Show(Assembly.GetExecutingAssembly().GetName().Name + ".dll ", ConsoleColor.Green);

            ShowLine("/assembly:... /serviceName:... /controller:... /output:...", ConsoleColor.Yellow);
            Console.WriteLine();

            ShowLine("PARAMETERS: ", ConsoleColor.White);
            Console.WriteLine("--------------------");
            Show("assembly: ", ConsoleColor.Yellow);
            Console.WriteLine(@"Path of the dll containing the Api, e.g.C:\Projects\MyProject\Website\bin\Debug\netcoreapp2.0\website.dll");
            Show("serviceName: ", ConsoleColor.Yellow);
            Console.WriteLine(@"The name of the microservice that publishes this api as specified in appSettings under ""Microservice:Name""");
            Show("controller: ", ConsoleColor.Yellow);
            Console.WriteLine(@"The full name of the Api controller class, e.g. ""MyPublisherService.MyConsumerApi""");
            Show("output: ", ConsoleColor.Yellow);
            Console.WriteLine(@"The full path to a directory inside which the proxy project will be generated. A new folder will be created named <serviceName>.<controller> will be created under <output>. If it already exists, it will be deleted first.");
            Console.WriteLine();
            Console.WriteLine("===================================");
            ShowLine("Usage pattern 2:", ConsoleColor.Red);
            Show("   dotnet ", ConsoleColor.White);
            Show(Assembly.GetExecutingAssembly().GetName().Name + ".dll ", ConsoleColor.Green);

            ShowLine("/website:... /controller:...", ConsoleColor.Yellow);
            Console.WriteLine();

            return -1;
        }

    }
}
