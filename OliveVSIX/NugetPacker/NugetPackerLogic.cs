using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OliveVSIX.NugetPacker
{
    public delegate void ExceptionHandler(object sender, Exception arg);

    static class NugetPackerLogic
    {
        const string NuspecFileName = "Package.nuspec";
        const string NugetFileName = "nuget.exe";
        const string OutputFolder = "NugetPackages";
#pragma warning disable GCop412 // Never hardcode a path or drive name in code. Get the application path programmatically and use relative path. Use “AppDomain.CurrentDomain.GetPath” to get the physical path.
        const string ApiKeyContainingFile = @"C:\Projects\NUGET-Publish-Key.txt";
#pragma warning restore GCop412 // Never hardcode a path or drive name in code. Get the application path programmatically and use relative path. Use “AppDomain.CurrentDomain.GetPath” to get the physical path.

        static DTE2 Dte2;
        static string NugetExe;
        static string SolutionPath;
        static string ApiKey;
        static string NugetPackagesFolder;

        static System.Threading.Thread Thread;

        public static event EventHandler OnCompleted;
        public static event ExceptionHandler OnException;

        public static void Pack(DTE2 dte2)
        {
            Dte2 = dte2;

            SolutionPath = Path.GetDirectoryName(Dte2.Solution.FullName);
            NugetExe = Path.Combine(SolutionPath, NugetFileName);
            ApiKey = File.ReadAllText(ApiKeyContainingFile);
            NugetPackagesFolder = Path.Combine(SolutionPath, OutputFolder);
            
            var start = new System.Threading.ThreadStart(() =>
            {
                try
                {
                    foreach (var item in GetSelectedProjectPath())
                        PackSingleProject(item);
                }
                catch (Exception exception)
                {
                    InvokeException(exception);
                }

                OnCompleted?.Invoke(null, EventArgs.Empty);
            });

            Thread = new System.Threading.Thread(start) { IsBackground = true };
            Thread.Start();
        }

        static void InvokeException(Exception exception) => OnException?.Invoke(null, exception);

        static void PackSingleProject(Project proj)
        {
            var projectPath = Path.GetDirectoryName(proj.FullName);
            var nuspecAddress = Path.Combine(projectPath, NuspecFileName);

            var packageFilename = UpdateVersionThenReturnPackageName(nuspecAddress);

            if (TryPack(nuspecAddress, out string packingMessage))
            {
                if (!TryPush(packageFilename, out string pushingMessage))
                    InvokeException(new Exception(pushingMessage));
            }
            else
                InvokeException(new Exception(packingMessage));
        }

        static bool TryPush(string packageFilename, out string message)
        {
            return ExecuteNuget($"push \"{NugetPackagesFolder}\\{packageFilename}\" {ApiKey} -NonInteractive -Source https://www.nuget.org/api/v2/package", out message);
        }

        static bool TryPack(string nuspecAddress, out string message) =>
            ExecuteNuget($"pack \"{nuspecAddress}\" -OutputDirectory \"{NugetPackagesFolder}\"", out message);

        static bool ExecuteNuget(string arguments, out string message)
        {
            var startInfo = new ProcessStartInfo(NugetExe)
            {
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var process = System.Diagnostics.Process.Start(startInfo);

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                message = null;
                return true;
            }
            else
            {
                message = $"{arguments}:\r\n{process.StandardError.ReadToEnd()}";
                return false;
            }
        }

        static string UpdateVersionThenReturnPackageName(string nuspecAddress)
        {
            var doc = new XmlDocument();
            doc.Load(nuspecAddress);

            // TODO: This is not a proper way to access to xml nodes. It is just a in rush code.
            var idNode = doc.ChildNodes[1].ChildNodes[0].ChildNodes[0];
            var versionNode = doc.ChildNodes[1].ChildNodes[0].ChildNodes[1];

            var versionParts = versionNode.InnerText.Split('.');
            var minorPart = int.Parse(versionParts.LastOrDefault()) + 1;

            var newVersion = string.Empty;
            for (var index = 0; index < versionParts.Length - 1; index++)
                newVersion += $"{versionParts[index]}.";

            versionNode.InnerText = $"{newVersion}{minorPart}";

            doc.Save(nuspecAddress);

            return $"{idNode.InnerText}.{versionNode.InnerText}.nupkg";
        }

        static IEnumerable<Project> GetSelectedProjectPath()
        {
            var uih = Dte2.ToolWindows.SolutionExplorer;
            var selectedItems = (Array)uih.SelectedItems;

            if (selectedItems != null)
                foreach (UIHierarchyItem selItem in selectedItems)
                    if (selItem.Object is Project proj)
                        yield return proj;
        }
    }
}
