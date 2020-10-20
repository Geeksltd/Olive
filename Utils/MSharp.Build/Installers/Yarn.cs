using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MSharp.Build.Installers
{
    class Yarn : Installer
    {
        public Yarn(string name, string installCommand = null) : base(name, installCommand ?? "global add " + name)
        {
        }

        protected override FileInfo Executable => Commands.Yarn;
    }
}
