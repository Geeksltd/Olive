namespace Olive.Mvc
{
    public class MenuItem
    {
        public string Key { get; set; }

        public string Url { get; set; }

        public MenuItem(string key, string url)
        {
            Key = key;
            Url = url;
        }

        static string CurrentUrl() => Context.Current.Request().ToPathAndQuery().UrlDecode();

        public bool MatchesCurrentUrl()
        {
            return CurrentUrl().StartsWith(Url.OrEmpty(), caseSensitive: false);
        }

        public bool MatchesCurrentUrlWithoutQuery()
        {
            return CurrentUrl().RemoveFrom("?").StartsWith(Url.OrEmpty().RemoveFrom("?"), caseSensitive: false);
        }


        
    }
}