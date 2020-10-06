using Olive;
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

            var result = 0;

            if (args.Lacks("-notools"))
            {
                var buildTools = new BuildTools();
                result = Run(() => buildTools.Build(), buildTools.PrintLog);
                if (result != 0) return result;
            }

            if (args.Lacks("-tools"))
            {
                var root = new DirectoryInfo(Environment.CurrentDirectory.TrimEnd('\\'));

                Console.WriteLine("Build started for: " + root.FullName);
                Console.WriteLine();

                var solution = new OliveSolution(root, publish: args.Contains("-publish"));
                result = Run(() => solution.Build(), solution.PrintLog);
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
                Console.WriteLine(ex.ToLogString());
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