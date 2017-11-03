using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace OliveVSIX.NugetPacker
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class NugetPacker
    {
        bool ExceptionOccurred;

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("090581b5-cfbb-40d7-9ff4-bdc7f81edef5");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="NugetPacker"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        NugetPacker(Package package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);

                NugetPackerLogic.OnCompleted += NugetPackerLogic_OnCompleted;
                NugetPackerLogic.OnException += NugetPackerLogic_OnException;
            }
        }

        void NugetPackerLogic_OnException(object sender, Exception arg)
        {
            ExceptionOccurred = true;

            VsShellUtilities.ShowMessageBox(
                ServiceProvider,
                arg.Message,
                arg.GetType().ToString(),
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        void NugetPackerLogic_OnCompleted(object sender, EventArgs e)
        {
            VsShellUtilities.ShowMessageBox(
                ServiceProvider,
                ExceptionOccurred ? "The process completed with error(s)." : "The selected projects are updated.",
                "Nuget updater",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static NugetPacker Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        IServiceProvider ServiceProvider => package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new NugetPacker(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        void MenuItemCallback(object sender, EventArgs e)
        {
            ExceptionOccurred = false;
            var dte2 = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
            NugetPackerLogic.Pack(dte2);
        }
    }
}
