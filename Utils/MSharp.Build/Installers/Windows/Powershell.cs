using Olive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MSharp.Build.Installers.Windows
{
    class Powershell : Installer
    {
        public Powershell(string name, string installCommand) : base(name, installCommand)
        {

        }

        protected override FileInfo Executable => Commands.Powershell;
    }
}
