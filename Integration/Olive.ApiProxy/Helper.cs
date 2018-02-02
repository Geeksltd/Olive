using System;
using System.Reflection;

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

        static void Param(string name, string description)
        {
            Show(name + ": ", ConsoleColor.Yellow);
            Console.WriteLine(description);
        }

        public static int ShowHelp()
        {
            Console.WriteLine("===================================");

            ShowLine("Usage pattern 1:", ConsoleColor.Red);
            Show("   dotnet ", ConsoleColor.White);
            Show(Assembly.GetExecutingAssembly().GetName().Name + ".dll ", ConsoleColor.Green);
            ShowLine("/file:... /out:...", ConsoleColor.Yellow);
            Console.WriteLine();

            ShowLine("Usage pattern 2:", ConsoleColor.Red);
            Show("   dotnet ", ConsoleColor.White);
            Show(Assembly.GetExecutingAssembly().GetName().Name + ".dll ", ConsoleColor.Green);
            ShowLine("/website:... /controller:... /out:...", ConsoleColor.Yellow);
            Console.WriteLine();

            ShowLine("Usage pattern 3:", ConsoleColor.Red);
            Show("   dotnet ", ConsoleColor.White);
            Show(Assembly.GetExecutingAssembly().GetName().Name + ".dll ", ConsoleColor.Green);
            ShowLine("/assembly:... /serviceName:... /controller:... /out:...", ConsoleColor.Yellow);
            Console.WriteLine();

            ShowLine("PARAMETERS: ", ConsoleColor.Red);
            Console.WriteLine("--------------------");
            Param("file", @"The full path to controller file which is expected to be inside a Website\Api[\...] folder.");
            Param("website", @"The full path to a the website directory");
            Param("controller", @"The name of the Api controller class, e.g. ""MyConsumerApi"". If there are multiple classes with the same name, provide the full name with namespace.");
            Param("out", @"The full path to a directory inside which the proxy nuget package will be generated, e.g. C:\Projects\my-solution\PrivatePackages");

            Param("assembly", @"Path of the dll containing the Api, e.g.C:\Projects\MyProject\Website\bin\Debug\netcoreapp2.0\website.dll");

            Param("serviceName", @"The name of the microservice that publishes this api as specified in appSettings under ""Microservice:Name""");

            return -1;
        }
    }
}