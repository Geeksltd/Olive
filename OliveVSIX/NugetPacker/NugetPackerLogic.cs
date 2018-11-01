using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace OliveVSIX.NugetPacker
{
    public delegate void ExceptionHandler(object sender, Exception arg);

    static class NugetPackerLogic
    {
        const string NUSPEC_FILE_NAME = "Package.nuspec";
        const string NUGET_FILE_NAME = "nuget.exe";
        const string OUTPUT_FOLDER = "NugetPackages";
#pragma warning disable GCop412 // Never hardcode a path or drive name in code. Get the application path programmatically and use relative path. Use “AppDomain.CurrentDomain.GetPath” to get the physical path.
        const string API_KEY_CONTAINING_FILE = @"C:\Projects\NUGET-Publish-Key.txt";
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
            NugetExe = Path.Combine(SolutionPath, NUGET_FILE_NAME);
            ApiKey = File.ReadAllText(API_KEY_CONTAINING_FILE);
            NugetPackagesFolder = Path.Combine(SolutionPath, OUTPUT_FOLDER);

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
            var nuspecAddress = Path.Combine(projectPath, NUSPEC_FILE_NAME);

            if (!File.Exists(nuspecAddress))
            {
                GenerateNugetFromVSProject(proj.FileName);
            }
            else
            {
                GenerateNugetFromNuspec(nuspecAddress);
            }
        }

        static void GenerateNugetFromNuspec(string nuspecAddress)
        {
            var packageFilename = UpdateNuspecVersionThenReturnPackageName(nuspecAddress);

            if (TryPackNuget(nuspecAddress, out string packingMessage))
            {
                if (!TryPush(packageFilename, out string pushingMessage))
                    InvokeException(new Exception(pushingMessage));
            }
            else
                InvokeException(new Exception(packingMessage));
        }

        static void GenerateNugetFromVSProject(string projectAddress)
        {
            var packageFilename = UpdateVisualStudioPackageVersionThenReturnPackageName(projectAddress);

            if (TryPackDotnet(projectAddress, out string packingMessageDotnet))
            {
                if (!TryPushDotnet(packageFilename, out string pushingMessage))
                    InvokeException(new Exception(pushingMessage));
            }
            else
                InvokeException(new Exception(packingMessageDotnet));
        }

        static bool TryPushDotnet(string packageFilename, out string message)
        {
            if (!ExecuteNuget($"nuget push \"{NugetPackagesFolder}\\{packageFilename}\" -k {ApiKey} -s https://www.nuget.org/api/v2/package", out message, "dotnet"))
                return false;

            if (!ExecuteNuget($"nuget push \"{NugetPackagesFolder}\\{packageFilename}\" -k thisIsMyApiKey -s http://nuget.geeksms.uat.co/nuget", out message, "dotnet"))
                return false;

            return true;
        }

        static bool TryPush(string packageFilename, out string message)
        {
            if (!ExecuteNuget($"push \"{NugetPackagesFolder}\\{packageFilename}\" {ApiKey} -NonInteractive -Source https://www.nuget.org/api/v2/package", out message, NugetExe))
                return false;

            if (!ExecuteNuget($"push \"{NugetPackagesFolder}\\{packageFilename}\" thisIsMyApiKey -NonInteractive -Source http://nuget.geeksms.uat.co/nuget", out message, NugetExe))
                return false;

            return true;
        }

        static bool TryPackNuget(string nuspecAddress, out string message) =>
            ExecuteNuget($"pack \"{nuspecAddress}\" -OutputDirectory \"{NugetPackagesFolder}\"", out message, NugetExe);

        static bool TryPackDotnet(string projectAddress, out string message) =>
            ExecuteNuget($"pack \"{projectAddress}\" -o \"{NugetPackagesFolder}\"", out message, "dotnet");

        static bool ExecuteNuget(string arguments, out string message, string processStart)
        {
            var startInfo = new ProcessStartInfo(processStart)
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

        static string UpdateNuspecVersionThenReturnPackageName(string nuspecAddress)
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

        static string UpdateVisualStudioPackageVersionThenReturnPackageName(string projectAddress)
        {
            var doc = new XmlDocument();
            doc.Load(projectAddress);

            var projectFile = doc.SelectSingleNode("Project/PropertyGroup");
            var idNode = projectFile.SelectSingleNode("PackageId");
            var versionNode = projectFile.SelectSingleNode("Version");

            var versionParts = versionNode.InnerText.Split('.');
            var minorPart = int.Parse(versionParts.LastOrDefault()) + 1;

            var newVersion = string.Empty;
            for (var index = 0; index < versionParts.Length - 1; index++)
                newVersion += $"{versionParts[index]}.";

            versionNode.InnerText = $"{newVersion}{minorPart}";

            doc.Save(projectAddress);

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
