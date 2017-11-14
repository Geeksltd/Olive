using Olive;
using System;
using System.Linq;
using System.IO;

namespace PushForLocalTesting
{
    class Program
    {
        static DirectoryInfo Root;

        static void Main(string[] args)
        {
            Root = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            var packages = Root.GetFiles("Olive*.dll").Select(x => x.Name.TrimEnd(".dll"));

            var nuget = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).AsDirectory()
                .GetSubDirectory(".nuget\\packages");

            if (!nuget.Exists()) throw new Exception("Nuget folder not found at " + nuget.FullName);

            foreach (var pack in packages)
            {
                Console.ForegroundColor = ConsoleColor.White;
                var packFolder = nuget.GetSubDirectory(pack.Replace(".Services.", "."));
                if (!packFolder.Exists())
                    continue; // Not installed. 

                var latest = packFolder.GetDirectories().WithMax(x => ToVersion(x.Name));
                if (latest == null)
                {
                    Error("Skipped " + pack + " - Package folder has no version installed! " + packFolder.FullName);
                    continue;
                }

                var target = latest.GetSubDirectory("lib\\netcoreapp2.0");
                if (!target.Exists())
                {
                    Error("Skipped " + pack + " - lib folder not found at " + target.FullName);
                    continue;
                }

                Update(pack, target);
            }
        }

        static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static string ToVersion(string name)
        {
            return name.Split('.').Select(x => x.PadLeft(4, '0')).ToString(".");
        }

        static void Update(string package, DirectoryInfo target)
        {
            Console.Write("Copying " + package + " to local NuGet packages...");

            foreach (var ext in "dll,xml,pdb".Split(','))
            {
                var source = Root.GetFile(package + "." + ext);
                if (!source.Exists())
                    Error("Skipped missing file: " + source.FullName);
                else
                    source.CopyTo(target, overwrite: true).Wait();
            }

            Console.WriteLine("Done.");
        }
    }
}
