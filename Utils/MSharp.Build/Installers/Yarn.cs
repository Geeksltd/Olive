using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MSharp.Build.Installers
{
    class Yarn : Installer
    {
        public Yarn(string name, string installCommand) : base(name, installCommand)
        {
        }

        protected override FileInfo Executable => Commands.Yarn;
    }
}
