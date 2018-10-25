using System;
using System.IO;
using System.Linq;

namespace MSharp.Build
{
    class Program
    {
        static bool Log;

        static int Main(string[] args)
        {
            Log = args.Contains("-log");

            var buildTools = new BuildTools();
            var result = Run(buildTools.Build, buildTools.PrintLog);
            if (result != 0) return result;

            if (!args.Contains("-tools"))
            {
                var root = new DirectoryInfo(Environment.CurrentDirectory.TrimEnd('\\'));

                Console.WriteLine("Build started for: " + root.FullName);
                Console.WriteLine();

                var solution = new OliveSolution(root,
                               publish: args.Contains("-publish"),
                               reportGCopWarnings: args.Contains("-gcop"));
                result = Run(solution.Build, solution.PrintLog);
            }

            return result;
        }

        static int Run(Action work, Action printLog)
        {
            try
            {
                work();
                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                return -1;
            }
            finally
            {
                if (Log) printLog();
            }
        }
    }
}