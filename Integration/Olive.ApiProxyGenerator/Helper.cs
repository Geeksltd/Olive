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
            ShowLine("Usage pattern:\n", ConsoleColor.Red);
            Show("   dotnet ", ConsoleColor.White);
            Show(Assembly.GetExecutingAssembly().GetName().Name + ".dll ", ConsoleColor.Green);
            ShowLine("[/assembly:...] [/serviceName:...] /controller:... /out:...", ConsoleColor.Yellow);
            Console.WriteLine();

            ShowLine("PARAMETERS: ", ConsoleColor.Red);
            Console.WriteLine("--------------------");

            Param("assembly", @"File name of the assembly containing the Api. It should be in the same folder as Olive.ApiProxyGenerator.dll. If not specified, 'Website.dll' will be used.");
            Console.WriteLine();

            Param("serviceName", @"The name of the microservice that publishes this api. If not specified the appSettings value of ""Microservice:Me:Name"" will be used. The namespace of the generated proxy classes will be this value suffixed by 'Service'.");
            Console.WriteLine();

            Param("controller", @"The name of the Api controller class, e.g. ""MyConsumerApi"". If there are multiple classes with the same name, provide the full name with namespace.");
            Console.WriteLine();

            Param("out", @"The full path to a directory to publish the generated nuget packages. e.g. C:\Projects\my-solution\PrivatePackages");
            Console.WriteLine();
            return -1;
        }
    }
}