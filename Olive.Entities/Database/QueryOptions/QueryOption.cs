namespace Olive.Entities
{
    public abstract class QueryOption
    {
        /// <summary>
        /// Creates a FullTextSearch option for the search query.
        /// </summary>
        public static FullTextSearchQueryOption FullTextSearch(string keyword, params string[] properties)
        {
            if (keyword.IsEmpty())
                throw new ArgumentNullException(nameof(keyword));
            if (properties == null || properties.None())
                throw new ArgumentNullException(nameof(properties));
            return new FullTextSearchQueryOption { Keyword = keyword, Properties = properties };
        }

        public static FullTextSearchQueryOption FullTextSearch<T>(string keyword, params Expression<Func<T>>[] properties)
        {
            if (properties == null || properties.None())
                throw new ArgumentNullException(nameof(properties));
            var propertyNames = new List<string>();
            foreach (var property in properties)
            {
                var propertyExpression = (property.Body as UnaryExpression)?.Operand as MemberExpression;
                if (propertyExpression == null)
                    throw new Exception($"Unsupported FullTextSearch expression. The only supported format is \"() => x.Property\". You provided: {property}");
                propertyNames.Add(propertyExpression.Member.Name);
            }

            return FullTextSearch(keyword, propertyNames.ToArray());
        }

        public static FullTextSearchQueryOption FullTextSearch<T>(string keyword, params Expression<Func<T, object>>[] properties)
        {
            if (properties == null || properties.None())
                throw new ArgumentNullException(nameof(properties));
            var propertyNames = new List<string>();
            foreach (var property in properties)
            {
                var propertyExpression = (property.Body as UnaryExpression)?.Operand as MemberExpression;
                if (propertyExpression == null || !(propertyExpression.Expression is ParameterExpression))
                    throw new Exception($"Unsupported OrderBy expression. The only supported format is \"x => x.Property\". You provided: {property}");
                propertyNames.Add(propertyExpression.Member.Name);
            }

            return FullTextSearch(keyword, propertyNames.ToArray());
        }
    }
}