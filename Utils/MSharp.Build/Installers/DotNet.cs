using System;
using System.Collections.Generic;
using System.IO;
using Olive;

namespace MSharp.Build.Installers
{
    class DotNet : Installer
    {
        internal DotNet(string name, string installCommand = null) : base(name, installCommand ?? "tool install -g " + name)
        {
        }

        protected override FileInfo Executable => Commands.DotNet;

        internal override string Install()
        {
            try
            {
                return base.Install();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already installed"))
                    return string.Empty;
                throw;
            }
        }
    }
}
