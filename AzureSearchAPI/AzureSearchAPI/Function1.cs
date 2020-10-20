using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace AzureSearchAPI
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            // Get the service endpoint and API key from the environment
            Uri endpoint = new Uri(Environment.GetEnvironmentVariable("SEARCH_ENDPOINT"));
            string key = Environment.GetEnvironmentVariable("SEARCH_API_KEY");
            string indexName = "azureblob-index2";

            // Create a client
            AzureKeyCredential credential = new AzureKeyCredential(key);
            SearchClient client = new SearchClient(endpoint, indexName, credential);




            Console.WriteLine($"New York ----------------------------------------\n");

            SearchResults<SearchDocument> response = client.Search<SearchDocument>("New York");
            foreach (SearchResult<SearchDocument> result in response.GetResults())
            {
                string title = (string)result.Document["metadata_storage_name"];
                Console.WriteLine($"{title}\n");
            }




            Console.WriteLine($"+London +Buckingham Palace ----------------------------------------\n");

            SearchResults<SearchDocument> response2 = client.Search<SearchDocument>("+\"London\" +\"Buckingham Palace\"", new SearchOptions { });
            foreach (SearchResult<SearchDocument> result in response2.GetResults())
            {
                string title = (string)result.Document["metadata_storage_name"];
                Console.WriteLine($"{title}\n");
            }













            //Console.WriteLine($"Las Vegas +reviews ----------------------------------------\n");


            ////TODO - not finished

            //var options = new SearchOptions() { };

            //// Enter Hotel property names into this list so only these values will be returned.
            //// If Select is empty, all values will be returned, which can be inefficient.
            //options.Select.Add("reviews");

            //SearchResults<SearchDocument> response3 = client.Search<SearchDocument>("Las Vegas", options);
            //foreach (SearchResult<SearchDocument> result in response3.GetResults())
            //{
            //    // Print out the title and job description (we'll see below how to
            //    // use C# objects to make accessing these fields much easier)
            //    string title = (string)result.Document["metadata_storage_name"];
            //    //                string description = (string)result.Document["content"];
            //    //                Console.WriteLine($"{title}\n{description}\n");
            //    Console.WriteLine($"{title}\n");
            //}


            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
