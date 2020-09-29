using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MSharp.Build.Installers
{
    class NodeJs : Installer
    {
        public NodeJs(string name, string installCommand = null) : base(name, installCommand ?? "install -g " + name)
        {
        }

        protected override FileInfo Executable => Commands.NodeJs;
    }
}
