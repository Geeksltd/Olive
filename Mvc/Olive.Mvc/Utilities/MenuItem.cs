﻿namespace Olive.Mvc
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

        public bool MatchesCurrentUrl()
        {
            var currentUrl = Context.Current.Request().ToPathAndQuery();

            return currentUrl.StartsWith(Url.OrEmpty().TrimAfter("?", trimPhrase: true), caseSensitive: false);
        }
    }
}