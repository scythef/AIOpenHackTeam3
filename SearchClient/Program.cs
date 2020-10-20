using System;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using SearchClient;


namespace SearchClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            string searchServiceName = configuration["SearchServiceName"];
            string adminApiKey = configuration["SearchServiceAdminApiKey"];

            Console.WriteLine("searchServiceName: {0}", searchServiceName);
            Console.WriteLine("searchServiceApiKey: {0}", adminApiKey);

            var searchClient = new WebsiteDocumentsSearchClient(searchServiceName, adminApiKey);

            Console.WriteLine("{0}", "Deleting index...");
            searchClient.DeleteIndexIfExists();

            Console.WriteLine("{0}", "Creating index...");
            searchClient.CreateIndex();

            Console.WriteLine("{0}", "Creating datasource...");
            searchClient.CreateDataSource(configuration);

            Console.WriteLine("{0}", "Creating indexer...");
            searchClient.CreateIndexer();

            Console.WriteLine("{0}", "Running indexer...");
            searchClient.RunIndexer();
        }
    }
}
