using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;

namespace SearchClient
{
    public class WebsiteDocumentsSearchClient
    {
        private ISearchIndexClient _indexClient;
        private SearchServiceClient _serviceClient;
        private static readonly string _indexName = "website-documents-index";

        public WebsiteDocumentsSearchClient(string searchServiceName, string searchServiceKey)
        {
            _serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(searchServiceKey));
            _indexClient = GetWebsiteDocumentsIndex();
        }

        // return the content items search index
        // it creates the index, if it doesn't exist
        public ISearchIndexClient GetWebsiteDocumentsIndex()
        {
            if (!_serviceClient.Indexes.Exists(_indexName))
            {
                CreateIndex(_indexName, _serviceClient);
            }

            return _serviceClient.Indexes.GetClient(_indexName);
        }

        // Create an index whose fields correspond to the properties of the WebsiteDocument class.
        // The fields of the index are defined by calling the FieldBuilder.BuildForType() method.
        private static void CreateIndex(string indexName, SearchServiceClient serviceClient)
        {
            var definition = new Microsoft.Azure.Search.Models.Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<WebsiteDocument>()
            };

            serviceClient.Indexes.Create(definition);
        }

        public void CreateIndex()
        {
            CreateIndex(_indexName, _serviceClient);
        }
        
        public void DeleteIndexIfExists()
        {
            if (_serviceClient.Indexes.Exists(_indexName))
            {
                _serviceClient.Indexes.Delete(_indexName);
            }
        }

        public void CreateDataSource(IConfigurationRoot configuration)
        {
            string conn = configuration["StorageConnectionString"];
            DataSource blobdatasource = DataSource.AzureBlobStorage(
                name: "websitedocs",
                storageConnectionString: conn, 
                containerName: "websitedocs"
            );

            _serviceClient.DataSources.CreateOrUpdate(blobdatasource);           
        }

        public void CreateIndexer()
        {
            var mapping = new List<FieldMapping>();
        
            mapping.Add(new FieldMapping( sourceFieldName: "metadata_storage_name", targetFieldName: "file_name" ));
            mapping.Add(new FieldMapping( sourceFieldName: "metadata_storage_path", targetFieldName: "url" ));
            mapping.Add(new FieldMapping( sourceFieldName: "metadata_storage_size", targetFieldName: "size" ));
            mapping.Add(new FieldMapping( sourceFieldName: "metadata_storage_last_modified", targetFieldName: "last_modified" ));

            var outputMapping = new List<FieldMapping>();
        
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/sentiment_score", targetFieldName: "Sentiment_score" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/persons/*", targetFieldName: "Persons" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/locations/*", targetFieldName: "Locations" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/key_phrases/*", targetFieldName: "Key_phrases" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/urls/*", targetFieldName: "Urls" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/merged_text", targetFieldName: "Merged_text" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/normalized_images/*/extracted_text", targetFieldName: "Extracted_text" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/normalized_images/*/categories", targetFieldName: "Categories" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/normalized_images/*/tags", targetFieldName: "Tags" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/normalized_images/*/description", targetFieldName: "Description" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/normalized_images/*/faces", targetFieldName: "Faces" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/normalized_images/*/brands", targetFieldName: "Brands" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/normalized_images/*/objects", targetFieldName: "Objects" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/normalized_images/*/categories/detail/celebrities/*", targetFieldName: "Celebrities" ));
            outputMapping.Add(new FieldMapping( sourceFieldName: "/document/normalized_images/*/categories/detail/landmarks/*", targetFieldName: "Landmarks" ));

            // required in challenge 4, tell the indexer to create content, metadata and images
            IDictionary<string, object> idxrConfig = new Dictionary<string, object>();
            idxrConfig.Add(
                key: "dataToExtract",
                value: "contentAndMetadata");
            idxrConfig.Add(
                key: "imageAction",
                value: "generateNormalizedImages"
            );

            Indexer blobIndexer = new Indexer(
                name: "website-indexer",
                dataSourceName: "websitedocs",
                targetIndexName: "website-documents-index",
                fieldMappings: mapping,
                skillsetName: "test003",
                schedule: new IndexingSchedule(TimeSpan.FromDays(1)),
                parameters: new IndexingParameters(
                    maxFailedItems: -1,
                    maxFailedItemsPerBatch: -1,
                    configuration: idxrConfig),
                outputFieldMappings: outputMapping);

            if (_serviceClient.Indexers.Exists("website-indexer"))
            {
                _serviceClient.Indexers.Reset("website-indexer");
            }

            _serviceClient.Indexers.CreateOrUpdate(blobIndexer);
        }

        public void RunIndexer()
        {
            _serviceClient.Indexers.Run("website-indexer");

            Console.WriteLine("Indexing documents...");

            Thread.Sleep(10000);
        }

        public void RunQueries()
        {
            string searchDescription;
            string searchTerm;
            SearchParameters searchParameters;

            searchDescription = "Generic New York Search";
            searchTerm = "New York";
            searchParameters = new SearchParameters()
                {
                    Select = new[] { "File_name", "Url", "Size", "Last_modified","Content","Sentiment_score","Key_phrases","Locations" },
                    IncludeTotalResultCount=true,
                    SearchFields = new[] {"Content"},
                    OrderBy = new[] { "Sentiment_score desc" },
                    QueryType = QueryType.Full
                };
            RunQuery(searchDescription, searchTerm, searchParameters);

            searchDescription = "Filter search results to include only collateral documents that contain URLs.";
            searchTerm = "*";
            searchParameters = new SearchParameters()
                {
                    Select = new[] { "File_name", "Url", "Urls" },
                    Filter = "search.ismatch('collateral', 'Url') and Urls/any()",
                    SearchMode = SearchMode.All,
                    QueryType = QueryType.Full
                };
            RunQuery(searchDescription, searchTerm, searchParameters);
        }

        private void RunQuery(string searchDescription, string searchTerm, SearchParameters searchParameters)
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine(searchDescription);
            Console.WriteLine("Searching '{0}'...", searchTerm);

            DocumentSearchResult<WebsiteDocument> result = _indexClient.Documents.Search<WebsiteDocument>("\"" + searchTerm + "\"", searchParameters);

            PrintResults(result);
        }

        private static void PrintResults(DocumentSearchResult<WebsiteDocument> result)
        {
            long? count = result.Count;

            Console.WriteLine("Results found: {0}",count);

            foreach(var resultItem in result.Results)
            {
                WebsiteDocument doc = resultItem.Document;

                Console.WriteLine("--------------------------------------------------------------------------------");
                Console.WriteLine("Url         : {0}", doc.Url);
                Console.WriteLine("File Name   : {0}", doc.File_name);
                Console.WriteLine("Key Phrases : {0}", string.Join(", ",doc.Key_phrases));
                Console.WriteLine("Locations   : {0}", string.Join(", ", doc.Locations));
                Console.WriteLine("Sentiment   : {0}", doc.Sentiment_score);
            }
        }
    }
}
