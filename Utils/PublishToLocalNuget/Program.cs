using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace PushForLocalTesting
{
    class Program
    {
        public static void Main()
        {
            var root = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            foreach (var t in new[] { "netstandard2.0", "netcoreapp2.0" })
                Deploy(new DirectoryInfo(Path.Combine(root.FullName, t)));
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
                var packFolder = new DirectoryInfo(Path.Combine(nuget.FullName, pack.Replace(".Services.", ".")));
                if (!packFolder.Exists)
                    continue; // Not installed. 

                var latest = packFolder.GetDirectories().OrderByDescending(x => ToVersion(x.Name)).FirstOrDefault();
                if (latest == null)
                {
                    Error("Skipped " + pack + " - Package folder has no version installed! " + packFolder.FullName);
                    continue;
                }

                var target = new DirectoryInfo(Path.Combine(latest.FullName, "lib", sourceDirectory.Name));
                if (!target.Exists)
                {
                    Error("Skipped " + pack + " - lib folder not found at " + target.FullName);
                    continue;
                }

                Update(pack, sourceDirectory, target);
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
            return string.Join(".", name.Split('.').Select(x => x.PadLeft(4, '0')));
        }

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