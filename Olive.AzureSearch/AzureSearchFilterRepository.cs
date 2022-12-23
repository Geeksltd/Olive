namespace Olive.AzureSearch
{
    using Azure.Search.Documents;
    using Azure;
    using Azure.Search.Documents.Indexes;
    using Azure.Search.Documents.Indexes.Models;
    using Azure.Search.Documents.Models;
    using System.Collections.Generic;
    using System.Linq;
    using Olive;
    using System;

    public class AzureSearchFilterRepository<T>
        where T : AzureSearchFilterDTOBase
    {
        private readonly string _defaultIndex;
        private readonly SearchIndexClient _client;
        private readonly IList<SearchField> _searchFields;
        private readonly object _lock = new object();

        public AzureSearchFilterRepository(AzureSearchConfigModel configuration)
        {
            _defaultIndex = configuration.DefaultIndex.ToLower();

            var endPoint = new Uri(configuration.ServiceUrl);
            var credential = new AzureKeyCredential(configuration.AdminKey);
            _client = new SearchIndexClient(endPoint, credential);

            var fieldBuilder = new FieldBuilder();
            _searchFields = fieldBuilder.Build(typeof(T));
        }

        public void Clear()
        {
            try
            {
                if (_client.GetIndex(_defaultIndex) != null)
                {
                    _client.DeleteIndex(_defaultIndex);
                }
                Initiate();
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                Initiate();
                //if exception occurred and status is "Not Found", this is work as expect
                //Failed to find index and this is because it's not there.
            }
        }


        public void PushSearchFilter(IEnumerable<T> searchFilters)
        {
            lock (_lock)
            {
                if (!searchFilters.Any())
                    return;

                var searchClient = _client.GetSearchClient(_defaultIndex);

                var batch = IndexDocumentsBatch.Create(searchFilters.Select(x => IndexDocumentsAction.Upload(x)).ToArray());

                IndexDocumentsResult result = searchClient.IndexDocuments(batch);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="maxCountToAddSuffixResponse">If the results count is smaller than maxCountToAddSuffixResponse, it will search for suffix responses</param>
        /// <returns></returns>
        public IEnumerable<T> GetBySearchTerm(string searchTerm, SearchOptions searchOptions = null, int maxCountToAddSuffixResponse = 60)
        {
            var options = searchOptions ?? GetDefaultSearchOptions();
            var responseResults = DoGetBySearchTerm($"{searchTerm}*", options);

            if (responseResults.Count < maxCountToAddSuffixResponse)
            {
                var suffixResponseResults = DoGetBySearchTerm($"/.*{searchTerm}/", options);

                if (suffixResponseResults != null)
                {
                    responseResults.AddRange(suffixResponseResults);
                }
            }

            return responseResults.Distinct().ToList();
        }

        public bool RemoveSearchFilter(string searchTerm, params string[] searchFields)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return true;
            }

            var searchClient = _client.GetSearchClient(_defaultIndex);

            try
            {
                var count = 0;
                do
                {
                    var options = GetSearchOptionsForRemove(searchFields);
                    var keys = DoGetBySearchTerm(searchTerm, options).Select(p => p.Id);
                    count = keys.Count();
                    if (count > 0)
                    {
                        var deleteInfo = searchClient.DeleteDocuments(nameof(AzureSearchFilterDTOBase.Id), keys);
                    }
                    else
                    {
                        break;
                    }
                }
                while (count == 1000);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public SearchOptions GetDefaultSearchOptions()
        {
            var options = new SearchOptions();

            foreach (var filterableSearchField in _searchFields.Where(x => x.IsFilterable == true))
            {
                options.SearchFields.Add(filterableSearchField.Name);
            }

            foreach (var filterableSearchField in _searchFields)
            {
                options.Select.Add(filterableSearchField.Name);
            }

            options.Size = 100;
            options.SearchMode = SearchMode.All;
            options.QueryType = SearchQueryType.Full;

            return options;
        }


        private void Initiate()
        {
            var definition = new SearchIndex(_defaultIndex, _searchFields);

            _client.CreateOrUpdateIndex(definition);
        }

        private SearchOptions GetSearchOptionsForRemove(params string[] searchFields)
        {
            var options = new SearchOptions();

            var searchFieldNames = searchFields
                ?? _searchFields
                    .Where(x => x.IsFilterable == true)
                    .Select(x => x.Name)
                    .ToArray();

            foreach (var filterableSearchField in searchFields)
            {
                options.SearchFields.Add(filterableSearchField);
            }

            options.Select.Add(nameof(AzureSearchFilterDTOBase.Id));

            options.Size = 1000;
            options.SearchMode = SearchMode.All;
            options.QueryType = SearchQueryType.Full;

            return options;
        }

        private List<T> DoGetBySearchTerm(string searchTerm, SearchOptions options)
        {
            ValidateSearchTerm(ref searchTerm);

            var searchClient = _client.GetSearchClient(_defaultIndex);

            SearchResults<T> response = searchClient.Search<T>(searchTerm, options);

            List<SearchResult<T>> responseResults = response.GetResults()?.ToList() ?? new List<SearchResult<T>>();

            return responseResults.Distinct().Select(p => p.Document).ToList();
        }

        private void ValidateSearchTerm(ref string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                throw new ArgumentException($"'{nameof(searchTerm)}' cannot be null or empty.", nameof(searchTerm));
            }

            if (searchTerm.Contains("-"))
                searchTerm = searchTerm.Replace("-", @"""-""");
        }

    }
}
