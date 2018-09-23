using System;
using System.IO;
using System.Linq;

namespace PushForLocalTesting
{
    class Program
    {
        static bool HasError;

        public static void Main()
        {
            var root = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            foreach (var t in new[] { "netstandard2.0", "netcoreapp2.1" })
                Deploy(new DirectoryInfo(Path.Combine(root.FullName, t)));

            if (HasError)
            {
                Console.WriteLine("Press Enter key to continue....");
                //   Console.ReadLine();
            }
        }

        static void Deploy(DirectoryInfo sourceDirectory)
        {
            var packages = sourceDirectory.GetFiles("Olive*.dll").Select(x => x.Name.Remove(x.Name.Length - 4));

            var nuget = new DirectoryInfo(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget\\packages"));

            if (!nuget.Exists) throw new Exception("Nuget folder not found at " + nuget.FullName);

            foreach (var pack in packages)
            {
                Console.ForegroundColor = ConsoleColor.White;

                var cleanName = pack.Replace(".Services.", ".").Replace("Olive.Mvc.Testing", "Olive.Testing");

                var packFolder = new DirectoryInfo(Path.Combine(nuget.FullName, cleanName));
                if (!packFolder.Exists)
                {
                    // Error("Skipped " + pack + " - Package folder not found:" + packFolder.FullName);
                    continue; // Not installed. 
                }

                var latest = packFolder.GetDirectories().OrderByDescending(x => ToVersion(x.Name)).FirstOrDefault();
                if (latest == null)
                {
                    // Error("Skipped " + pack + " - Package folder has no version installed! " + packFolder.FullName);
                    continue;
                }

                var target = new DirectoryInfo(Path.Combine(latest.FullName, "lib", sourceDirectory.Name));
                if (!target.Exists)
                {
                    // Error("Skipped " + pack + " - lib folder not found at " + target.FullName);
                    continue;
                }

                Update(pack, sourceDirectory, target);
            }
        }

        static void Error(string message)
        {
            HasError = true;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static string ToVersion(string name) => string.Join(".", name.Split('.').Select(x => x.PadLeft(4, '0')));

        static void Update(string package, DirectoryInfo sourceDirectory, DirectoryInfo target)
        {
            Console.Write("Copying " + package + " to local NuGet packages...");

            foreach (var ext in "dll,xml,pdb".Split(','))
            {
                var source = new FileInfo(Path.Combine(sourceDirectory.FullName, package + "." + ext));
                if (!source.Exists)
                    Error("Skipped missing file: " + source.FullName);
                else
                {
                    File.Copy(source.FullName, Path.Combine(target.FullName, source.Name), overwrite: true);
                }
            }

            Console.WriteLine("Done.");
        }
    }
}