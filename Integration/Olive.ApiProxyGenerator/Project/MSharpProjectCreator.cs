using System;

namespace Olive.ApiProxy
{
    class MSharpProjectCreator : ProjectCreator
    {
        public MSharpProjectCreator() : base("MSharp") { }

        protected override string Framework => "netcoreapp2.1";

        protected override string IconUrl => "http://licensing.msharp.co.uk/images/icon.png";

        protected override string[] References => new[] { "Olive", "MSharp" };

        protected override void AddFiles()
        {
            foreach (var type in DtoTypes.All)
            {
                Console.Write("Adding M# entity definition class " + type.Name + "...");
                Folder.GetFile(type.Name + ".cs").WriteAllText(new MSharpModelProgrammer(type).Generate());
                Console.WriteLine("Done");
            }
        }
    }
}