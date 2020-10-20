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

        // Create an index whose fields correspond to the properties of the ContentItemsSearchItem class.
        // The Address property of ContentItemsSearchItem will be modeled as a complex field.
        // The properties of the Address class in turn correspond to sub-fields of the Address complex field.
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
            var maps = new List<FieldMapping>();
        
            maps.Add(new FieldMapping( sourceFieldName: "metadata_storage_name", targetFieldName: "file_name" ));
            maps.Add(new FieldMapping( sourceFieldName: "metadata_storage_path", targetFieldName: "url" ));
            maps.Add(new FieldMapping( sourceFieldName: "metadata_storage_size", targetFieldName: "size" ));
            maps.Add(new FieldMapping( sourceFieldName: "metadata_storage_last_modified", targetFieldName: "last_modified" ));

            Indexer blobIndexer = new Indexer(
                name: "website-indexer",
                dataSourceName: "websitedocs",
                targetIndexName: "website-documents-index",
                fieldMappings: maps,
                schedule: new IndexingSchedule(TimeSpan.FromDays(1)));

            if (_serviceClient.Indexers.Exists("website-indexer"))
            {
                _serviceClient.Indexers.Reset("website-indexer");
            }

            _serviceClient.Indexers.CreateOrUpdate(blobIndexer);
        }

        public void RunIndexer()
        {
            _serviceClient.Indexers.Run("website-indexer");

            Console.WriteLine("Indexing documents...\n");

            Thread.Sleep(10000);
        }
    }
}
