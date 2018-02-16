using System;
using System.Linq;
using System.IO;

namespace Olive.ApiProxy
{
    class ParametersParser
    {
        static string[] Args;

        public static bool LoadParameters(string[] args)
        {
            Args = args;
            if (Args.None()) return false;

            var controllerName = Param("controller") ?? Param("file");
            if ((Context.ControllerName = controllerName) == null) return false;

            if ((Context.Output = Param("out")?.AsDirectory()) == null) return false;
            if (!Context.Output.Exists)
            {
                Console.WriteLine("The specified output folder does not exist.");
                return false;
            }

            var websiteFolder = Param("website")?.AsDirectory();
            if (websiteFolder != null) return LoadFromWebsite(websiteFolder);

            var controllerFile = Param("file")?.AsFile();
            if (controllerFile != null) return LoadFromControllerFile(controllerFile);

            return LoadFromDirectParameters();
        }

        static bool LoadFromControllerFile(FileInfo file)
        {
            if (file == null || !file.Exists)
            {
                Console.WriteLine(file?.FullName + " controller file does not exist!");
                return false;
            }

            var lines = file.ReadAllText().ToLines().Trim();
            var @namespace = lines.FirstOrDefault(x => x.StartsWith("namespace "))?.TrimBefore("namespace ", trimPhrase: true);
            var @class = lines.FirstOrDefault(x => x.StartsWith("public class "))?
                .TrimBefore("public class ", trimPhrase: true).TrimAfter(" ");

            Context.ControllerName = @namespace.WithSuffix(".") + @class;

            var directory = file.Directory;
            while (directory.Name.ToLower() != "website")
            {
                directory = directory.Parent;
                if (directory.Root == directory) return false;
            }

            return LoadFromWebsite(directory);
        }

        static bool LoadFromWebsite(DirectoryInfo websiteFolder)
        {
            if (!websiteFolder.Exists)
            {
                Console.WriteLine(websiteFolder.FullName + " does not exist!");
                return false;
            }

            Context.TempPath = websiteFolder.CreateSubdirectory("obj\\api-proxy");

            Console.WriteLine("Processing " + websiteFolder.FullName + "...");

            Context.AssemblyFile = websiteFolder.GetFile("bin\\Debug\\netcoreapp2.0\\Website.dll");
            Directory.SetCurrentDirectory(websiteFolder.FullName);

            Context.PublisherService = websiteFolder.GetFile("appSettings.json").ReadAllText().ToLines().Trim()
                  .SkipWhile(x => !x.StartsWith("\"Microservice\":"))
                  .FirstOrDefault(x => x.StartsWith("\"Name\":"))
                  ?.TrimBefore(":", trimPhrase: true).TrimEnd(",").Trim(' ', '\"');

            if (Context.PublisherService.IsEmpty())
            {
                Console.WriteLine("Setting of Microservice:Name under appSettings.json was not found.");
                return false;
            }

            return true;
        }

        static bool LoadFromDirectParameters()
        {
            if ((Context.PublisherService = Param("serviceName")) == null) return false;
            if ((Context.AssemblyFile = Param("assembly")?.AsFile()) == null) return false;

            Context.TempPath = Path.GetTempPath().AsDirectory()
                .GetOrCreateSubDirectory("api-proxy").CreateSubdirectory(Guid.NewGuid().ToString());

            return true;
        }

        static string Param(string key)
        {
            var decorateKey = "/" + key + ":";
            return Args.FirstOrDefault(x => x.StartsWith(decorateKey))?.TrimStart(decorateKey).OrNullIfEmpty();
        }
    }
}