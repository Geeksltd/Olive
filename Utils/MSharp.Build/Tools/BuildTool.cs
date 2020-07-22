using Olive;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        protected virtual void OnInstalled() { }

        protected virtual bool AlwaysInstall => false;

        public List<string> Logs = new List<string>();

        public FileInfo Install()
        {
            if (!AlwaysInstall)
                if (IsInstalled()) return Path;

            var log = Execute();
            Logs.Add(log);

            AddToPath();

            OnInstalled();

            if (IsInstalled()) return Path;
            else throw new Exception($"Failed to install {Name}. Install it manually.");
        }

        protected virtual string Execute()
        {
            if (Installer == WindowsCommand.Chocolaty)
            {
                var wrappedCommand = $"-noprofile -command \"&{{ start-process -FilePath '{Installer.FullName}' -ArgumentList '{InstallCommand} -y' -Wait -Verb RunAs}}\"";

                Logs.Add("Elavating to powershell command: " + wrappedCommand);

                return WindowsCommand.Powershell.Execute(wrappedCommand);
            }
            else
            {
                return Installer.Execute(InstallCommand);
            }
        }

        protected bool IsInstalled()
        {
            try
            {
                Path = WindowsCommand.FindExe(Name);
                return true;
            }
            catch
            {
                if (ExpectedPath != null && File.Exists(ExpectedPath.FullName))
                {
                    Path = ExpectedPath;
                    return true;
                }

                Path = null;
                return false;
            }
        }

        internal FileInfo GetActualPath()
        {
            if (IsInstalled()) return Path;
            throw new Exception(Name + " does not seem to be installed");
        }

        private FileInfo FindPath()
        {
            var output = ExecuteShell("where " + Name);
            throw new NotImplementedException();
        }

        public static string ExecuteShell(string command)
        {
            var output = new StringBuilder();

            var process = new Process
            {
                EnableRaisingEvents = true,

                StartInfo = new ProcessStartInfo
                {
                    FileName = GetShellFileName(),
                    CreateNoWindow = true,
                    Arguments = ToShellCommand(command),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data.HasValue()) lock (output) output.AppendLine(e.Data);
            };
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null) lock (output) output.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();


            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"Error running '{command}':{output}");
            else process.Dispose();


            return output.ToString();
        }

        static bool IsRunningOnLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        private static string GetShellFileName()
        {
            if (IsRunningOnLinux())
                return "/bin/bash";

            return "cmd";
        }

        private static string ToShellCommand(string command)
        {
            if (IsRunningOnLinux())
                return "-c " + command;

            return command;
        }

        void AddToPath()
        {
            var toCheck = new[] { Path, ExpectedPath }.Where(x => x != null).ToArray();
            var path = toCheck.FirstOrDefault(x => File.Exists(x.FullName));

            Path = path ?? throw new Exception("Failed to locate the installed tool: " +
                Name + Environment.NewLine + "Searched:\r\n" + string.Join("\r\n", toCheck.Select(x => x.FullName)));

            var parts = Environment.GetEnvironmentVariable("PATH").TrimOrEmpty().Split(';')
            .Concat(new[] { path.Directory.FullName })
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