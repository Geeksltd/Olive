using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Olive;

namespace MSharp.Build.Installers
{
    abstract class Installer
    {
        Installer()
        {
        }

        public Installer(string name, string installCommand)
        {
            Name = name;
            InstallCommand = installCommand;
        }

        protected abstract FileInfo Executable { get; }
        protected string InstallCommand { get; private set; }
        protected string Name { get; private set; }

        internal virtual string Install() => Executable.Execute(InstallCommand);

        internal FileInfo Find()
        {
            try
            {
                return Commands.FindExe(Name);
            }
            catch
            {
                if (Executable != null && File.Exists(Executable.FullName))
                {
                    return Executable;
                }
                return null;
            }
        }

        internal bool IsInstalled() => Find()?.Exists ?? false;

        internal void AddToPath()
        {
            var toCheck = Find() ?? throw new Exception("Failed to locate the installed tool: " + Name);

            var parts = Environment.GetEnvironmentVariable("PATH").TrimOrEmpty().Split(';')
            .Concat(new[] { toCheck.Directory.FullName })
            .Select(x => x + "\\")
            .Select(x => x.Replace("\\\\", "\\"))
            .Distinct()
            .OrderBy(x => x.Contains("\\."));

            var newPath = string.Join(";", parts);

            Environment.SetEnvironmentVariable("PATH", newPath);
            Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);

            try
            {
                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Machine);
            }
            catch
            {
            }
        }
    }
}
