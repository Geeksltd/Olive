using MSharp.Build.Installers;
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
        protected abstract Installer WindowsInstaller { get; }
        protected abstract Installer LinuxInstaller { get; }
        protected Installer Installer
        {
            get
            {
                return new[]
                {
                    new { OS = OSPlatform.Windows , Installer = WindowsInstaller},
                    new { OS = OSPlatform.Linux , Installer = LinuxInstaller},
                }
                .FirstOrDefault(i => i.OS == Runtime.OS)?.Installer ?? throw new NotSupportedException(Runtime.OS.ToString());
            }
        }
        protected abstract string Name { get; }

        public FileInfo Path { get; set; }

        protected virtual void OnInstalled() { }

        protected virtual bool AlwaysInstall => false;

        public List<string> Logs = new List<string>();

        public void Install()
        {
            if (!AlwaysInstall)
                if (Installer.IsInstalled()) ;

            var log = Execute();
            Logs.Add(log);

            Installer.AddToPath();

            OnInstalled();

            if (!Installer.IsInstalled())
                throw new Exception($"Failed to install {Name}. Install it manually.");
        }

        protected virtual string Execute()
        {
            return Installer.Install();
        }

        internal FileInfo GetActualPath()
        {
            if (Installer.IsInstalled()) return Path;
            throw new Exception(Name + " does not seem to be installed");
        }
    }
}