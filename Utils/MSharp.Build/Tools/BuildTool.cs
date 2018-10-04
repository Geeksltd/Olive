using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MSharp.Build.Tools
{
    abstract class BuildTool
    {
        protected abstract FileInfo Installer { get; }
        protected abstract string Name { get; }
        protected abstract string InstallCommand { get; }

        public FileInfo Path { get; set; }
        public abstract FileInfo ExpectedPath { get; }

      public  List<string> Logs = new List<string>();

        public FileInfo Install()
        {
            if (IsInstalled()) return Path;

            var log = Installer.Execute(InstallCommand);
            Logs.Add(log);

            AddToPath();

            if (IsInstalled()) return Path;
            else throw new Exception($"Failed to install {Name}. Install it manually.");
        }

        protected bool IsInstalled()
        {
            try
            {
                Path = WindowsCommand.Where.Execute(Name).Trim().ToLines()
                    .Select(x => x.AsFile()).First(x => x.Extension.HasValue());
                return true;
            }
            catch
            {
                Path = null;
                return false;
            }
        }

        void AddToPath()
        {
            var toCheck = new[] { Path, ExpectedPath }.Where(x => x != null).ToArray();
            var path = toCheck.FirstOrDefault(x => File.Exists(x.FullName));

            Path = path ??
                     throw new Exception("Failed to locate the installed tool: " + Name + Environment.NewLine +
                    "Searched:\r\n" + string.Join("\r\n", toCheck.Select(x => x.FullName)));

            var parts = Environment.GetEnvironmentVariable("PATH").TrimOrEmpty().Split(';')
            .Concat(new[] { path.Directory.FullName })
            .Distinct();

            Environment.SetEnvironmentVariable("PATH", string.Join(";", parts));
        }
    }
}
