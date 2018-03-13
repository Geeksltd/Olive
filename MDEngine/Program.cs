using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Olive.Markdown;
using System.Xml.Linq;

namespace MDEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                if (!options.ConsoleIn && options.InputFile == null
                    || !options.ConsoleOut && options.OutputFile == null)
                {
                    Console.WriteLine(options.GetUsage());
                    return;
                }
                // consume Options instance properties
                var inReader = options.ConsoleIn
                    ? Console.In
                    : new StreamReader(options.InputFile);
                using (var outWriter = options.ConsoleIn
                    ? Console.Out
                    : new StreamWriter(options.OutputFile)
                    )
                {
                    try
                    {
                        var xml = inReader.ReadToEnd();
                        var doc = XDocument.Parse(xml);
                        var md = doc.Root.ToMarkDown();
                        outWriter.Write(md);
                        outWriter.Close();
                        Console.WriteLine("Operation sucessful! MD file is in: " + options.OutputFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"Operation failed becuase of an error:
                        {ex.Message}
                        {ex.Data}
                        {ex.StackTrace}
                        ");

                    }

                }
            }
        }
    }
}
