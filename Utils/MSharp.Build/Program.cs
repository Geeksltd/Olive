using System;
using System.IO;
using System.Linq;

namespace MSharp.Build
{
    class Program
    {
        static int Main(string[] args)
        {
            var root = new DirectoryInfo(Environment.CurrentDirectory.TrimEnd('\\'));

            Console.WriteLine("Build started for: " + root.FullName);
            Console.WriteLine();

            var buildTools = new BuildTools();
            var solution = new OliveSolution(root,
                publish: args.Contains("-publish"),
                reportGCopWarnings: args.Contains("-gcop"));

            try
            {
                buildTools.Build();
                solution.Build();

                if (args.Contains("-log"))
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