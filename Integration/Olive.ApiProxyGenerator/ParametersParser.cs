using System;
using System.Linq;
using System.IO;

namespace Olive.ApiProxy
{
    class ParametersParser
    {
        static string[] Args;

        internal static bool Start(string[] args)
        {
            Args = args;
            Context.Source = AppDomain.CurrentDomain.GetBaseDirectory();
            if (Param("controller").IsEmpty()) return false;
            return Param("out").HasValue() || Param("push").HasValue();
        }

        public static void LoadParameters()
        {
            Context.ControllerName = Param("controller");
            Context.Output = Param("out")?.AsDirectory();

            if (Context.Output?.Exists == false)
                throw new Exception("The specified output folder does not exist.");

            Context.NugetServer = Param("push");
            Context.NugetApiKey = Param("apiKey");

            Context.PublisherService = GetServiceName();
            Context.AssemblyFile = Context.Source.GetFile(Param("assembly").Or("Website.dll"));
            if (!Context.AssemblyFile.Exists())
                throw new Exception(Context.AssemblyFile.FullName + " does not exist.");

            Context.TempPath = Path.GetTempPath().AsDirectory()
                .GetOrCreateSubDirectory("api-proxy").CreateSubdirectory(Guid.NewGuid().ToString());
        }

        //static bool LoadFromControllerFile(FileInfo file)
        //{
        //    if (file == null || !file.Exists)
        //    {
        //        Console.WriteLine(file?.FullName + " controller file does not exist!");
        //        return false;
        //    }

        //    var lines = file.ReadAllText().ToLines().Trim();
        //    var @namespace = lines.FirstOrDefault(x => x.StartsWith("namespace "))?.TrimBefore("namespace ", trimPhrase: true);
        //    var @class = lines.FirstOrDefault(x => x.StartsWith("public class "))?
        //        .TrimBefore("public class ", trimPhrase: true).TrimAfter(" ");

        //    Context.ControllerName = @namespace.WithSuffix(".") + @class;

        //    var directory = file.Directory;
        //    while (directory.Name.ToLower() != "website")
        //    {
        //        directory = directory.Parent;
        //        if (directory.Root == directory) return false;
        //    }

        //    return LoadFromWebsite(directory);
        //}

        //static bool LoadFromWebsite(DirectoryInfo websiteFolder)
        //{
        //    if (!websiteFolder.Exists)
        //    {
        //        Console.WriteLine(websiteFolder.FullName + " does not exist!");
        //        return false;
        //    }

        //    Context.TempPath = websiteFolder.CreateSubdirectory("obj\\api-proxy");

        //    Console.WriteLine("Processing " + websiteFolder.FullName + "...");

        //    Context.AssemblyFile = websiteFolder.GetFile("bin\\Debug\\netcoreapp2.0\\Website.dll");
        //    Directory.SetCurrentDirectory(websiteFolder.FullName);

        //    Context.PublisherService = websiteFolder.GetFile("appSettings.json").ReadAllText().ToLines().Trim()
        //          .SkipWhile(x => !x.StartsWith("\"Microservice\":"))
        //          .FirstOrDefault(x => x.StartsWith("\"Name\":"))
        //          ?.TrimBefore(":", trimPhrase: true).TrimEnd(",").Trim(' ', '\"');

        //    if (Context.PublisherService.IsEmpty())
        //    {
        //        Console.WriteLine("Setting of Microservice:Name under appSettings.json was not found.");
        //        return false;
        //    }

        //    return true;
        //}

        static string GetServiceName()
        {
            var value = Param("serviceName");
            if (value.HasValue()) return value;

            var appSettings = FindAppSettings();

            value = appSettings.ReadAllText().ToLines().Trim()
                    .SkipWhile(x => !x.StartsWith("\"Microservice\":"))
                    .SkipWhile(x => !x.StartsWith("\"Me\":"))
                    .FirstOrDefault(x => x.StartsWith("\"Name\":"))
                    ?.TrimBefore(":", trimPhrase: true).TrimEnd(",").Trim(' ', '\"');

            if (value.IsEmpty())
                throw new Exception("Failed to find Microservice:Me:Name in " + appSettings.FullName);

            return value;
        }

        static FileInfo FindAppSettings()
        {
            var dir = Context.Source.Parent;
            while (dir.Root.FullName != dir.FullName)
            {
                var result = dir.GetFile("appSettings.json");
                if (result.Exists()) return result;
                dir = dir.Parent;
            }
            throw new Exception("Failed to find appSettings.json in any of the parent directories.");
        }

        static string Param(string key)
        {
            var decorateKey = "/" + key + ":";
            return Args.FirstOrDefault(x => x.StartsWith(decorateKey))?.TrimStart(decorateKey).OrNullIfEmpty();
        }
    }
}