namespace Olive.ApiProxy
{
    class MSharp46ProjectCreator : MSharpProjectCreator
    {
        public MSharp46ProjectCreator() : base()
        {
            Folder = Folder.Parent.GetSubDirectory(Folder.Name + "46");
        }

        internal override string Name => base.Name.TrimEnd("46");

        protected override string Framework => "net46";

        protected override string[] References => new[] { "MSharp" };
    }
}