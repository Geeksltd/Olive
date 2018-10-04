using System;
using System.IO;
using System.Linq;

namespace MSharp.Build
{
    class Program
    {
        static bool InstallTools, Log;


        static int Main(string[] args)
        {
            InstallTools = !args.Contains("-fast");
            Log = args.Contains("-log");

            var root = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var solution = new OliveSolution(root, publish: args.Contains("-publish"));

            var buildTools = new BuildTools();

            try
            {
                if (InstallTools) buildTools.Build();
                solution.Build();

                if (Log)
                {
                    buildTools.PrintLog();
                    solution.PrintLog();
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();

                buildTools.PrintLog();
                solution.PrintLog();
                return -1;
            }
        }
    }
}
