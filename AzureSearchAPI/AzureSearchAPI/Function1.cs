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
using Azure.Search.Documents.Indexes.Models;
using System.Collections.Generic;
using Azure.Search.Documents.Indexes;

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
            string indexName = "website-documents-index";

            // Create a client
            AzureKeyCredential credential = new AzureKeyCredential(key);
            SearchClient client = new SearchClient(endpoint, indexName, credential);


            Console.WriteLine($"New York ----------------------------------------\n");

            SearchResults<SearchDocument> response = client.Search<SearchDocument>("New York");
            foreach (SearchResult<SearchDocument> result in response.GetResults())
            {
                string title = (string)result.Document["File_name"];
                Console.WriteLine($"{title}\n");
            }


            Console.WriteLine($"+London +Buckingham Palace ----------------------------------------\n");

            SearchResults<SearchDocument> response2 = client.Search<SearchDocument>("+\"London\" +\"Buckingham Palace\"", new SearchOptions { });
            foreach (SearchResult<SearchDocument> result in response2.GetResults())
            {
                string title = (string)result.Document["File_name"];
                double score = (double)result.Document["Sentiment_score"];
                Console.WriteLine($"{title} || {score}\n");
            }


            Console.WriteLine($"Positive ----------------------------------------\n");

            var options = new SearchOptions()
            {
                Filter = "Sentiment_score ge 0.5",
            };

            SearchResults<SearchDocument> response3 = client.Search<SearchDocument>("*", options);
            foreach (SearchResult<SearchDocument> result in response3.GetResults())
            {
                string title = (string)result.Document["File_name"];
                double score = (double)result.Document["Sentiment_score"];
                Console.WriteLine($"{title} || {score}\n");
            }

            Console.WriteLine($"Lower ----------------------------------------\n");

            options = new SearchOptions()
            {
                Filter = "Sentiment_score lt 0.5",
            };

            SearchResults<SearchDocument> response4 = client.Search<SearchDocument>("*", options);
            foreach (SearchResult<SearchDocument> result in response4.GetResults())
            {
                string title = (string)result.Document["File_name"];
                double score = (double)result.Document["Sentiment_score"];
                Console.WriteLine($"{title} || {score}\n");
            }

            Console.WriteLine($"Locations ----------------------------------------\n");

            options = new SearchOptions()
            {
                Filter = "Locations/any(loc: loc eq 'Broadway')",
            };
            options.OrderBy.Add("Sentiment_score desc");

            SearchResults<SearchDocument> response5 = client.Search<SearchDocument>("*", options);
            foreach (SearchResult<SearchDocument> result in response5.GetResults())
            {
                string title = (string)result.Document["File_name"];
                double score = (double)result.Document["Sentiment_score"];
                string loc = "";// (List<string>)result.Document["Locations"]; 
                Console.WriteLine($"{title} || {score}\n || {loc}\n");
            }


            //Console.WriteLine($"Images Grand Canyon ----------------------------------------\n");

            //options = new SearchOptions()
            //{
            //    Filter = "Locations/any(loc: loc eq 'Broadway')",
            //};

            //SearchResults<SearchDocument> response6 = client.Search<SearchDocument>("*", options);
            //foreach (SearchResult<SearchDocument> result in response6.GetResults())
            //{
            //    //string title = (string)result.Document["File_name"];
            //    //double score = (double)result.Document["Sentiment_score"];
            //    //string loc = "";// (List<string>)result.Document["Locations"]; 
            //    //string merged = (string)result.Document["Merged_text"];
            //    string extracted = (string)result.Document["Extracted_text"];
            //    string description = (string)result.Document["Description"];
            //    Console.WriteLine($"{extracted}\n || {description}\n");
            //}

            Console.WriteLine($"Locations ----------------------------------------\n");

            options = new SearchOptions()
            {                
            };
            options.OrderBy.Add("Size desc");

            SearchResults<SearchDocument> response7 = client.Search<SearchDocument>("*", options);
            foreach (SearchResult<SearchDocument> result in response7.GetResults())
            {
                string title = (string)result.Document["File_name"];
                int size = (int)result.Document["Size"];
                Console.WriteLine($"{title} || {size}\n");
            }

            //Console.WriteLine($"Las Vegas +reviews ----------------------------------------\n");


            //TODO - not finished

            //var options = new SearchOptions() { };

            //// Enter Hotel property names into this list so only these values will be returned.
            //// If Select is empty, all values will be returned, which can be inefficient.
            //options.Select.Add("reviews");

            ////SearchResults<SearchDocument> response3 = client.Search<SearchDocument>("Las Vegas", options);

            //SearchResults<SearchDocument> response3 = client.Search<SearchDocument>("+\"Las Vegas\" +\"revies\"", new SearchOptions { });
            //foreach (SearchResult<SearchDocument> result in response3.GetResults())
            //{
            //    // Print out the title and job description (we'll see below how to
            //    // use C# objects to make accessing these fields much easier)
            //    string title = (string)result.Document["metadata_storage_name"];
            //    //                string description = (string)result.Document["content"];
            //    //                Console.WriteLine($"{title}\n{description}\n");
            //    Console.WriteLine($"{title}\n");
            //}



            //            // Create the skills
            //            Console.WriteLine("Creating the skills...");
            //            //OcrSkill ocrSkill = CreateOcrSkill();
            //            //MergeSkill mergeSkill = CreateMergeSkill();
            //            EntityRecognitionSkill entityRecognitionSkill = CreateEntityRecognitionSkill();
            //            //LanguageDetectionSkill languageDetectionSkill = CreateLanguageDetectionSkill();
            //            //SplitSkill splitSkill = CreateSplitSkill();
            ////            KeyPhraseExtractionSkill keyPhraseExtractionSkill = CreateKeyPhraseExtractionSkill();

            //            // Create the skillset
            //            Console.WriteLine("Creating or updating the skillset...");
            //            List<SearchIndexerSkill> skills = new List<SearchIndexerSkill>();
            //            //skills.Add(ocrSkill);
            //            //skills.Add(mergeSkill);
            //            //skills.Add(languageDetectionSkill);
            //            //skills.Add(splitSkill);
            //            skills.Add(entityRecognitionSkill);
            //   //         skills.Add(keyPhraseExtractionSkill);

            //   //         SearchIndexerSkillset skillset = CreateOrUpdateDemoSkillSet(indexerClient, skills, cognitiveServicesKey);


            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }


        private static EntityRecognitionSkill CreateEntityRecognitionSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("text")
            {
                Source = "/document/pages/*"
            });

            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("organizations")
            {
                TargetName = "organizations"
            });

            EntityRecognitionSkill entityRecognitionSkill = new EntityRecognitionSkill(inputMappings, outputMappings)
            {
                Description = "Recognize organizations",
                Context = "/document/pages/*",
                DefaultLanguageCode = EntityRecognitionSkillLanguage.En
            };
            entityRecognitionSkill.Categories.Add(EntityCategory.Organization);

            return entityRecognitionSkill;
        }

        //private static SearchIndexerSkillset CreateOrUpdateDemoSkillSet(SearchIndexerClient indexerClient, IList<SearchIndexerSkill> skills, string cognitiveServicesKey)
        //{
        //    SearchIndexerSkillset skillset = new SearchIndexerSkillset("demoskillset", skills)
        //    {
        //        Description = "Demo skillset",
        //        CognitiveServicesAccount = new CognitiveServicesAccountKey(cognitiveServicesKey)
        //    };

        //    // Create the skillset in your search service.
        //    // The skillset does not need to be deleted if it was already created
        //    // since we are using the CreateOrUpdate method
        //    try
        //    {
        //        indexerClient.CreateOrUpdateSkillset(skillset);
        //    }
        //    catch (RequestFailedException ex)
        //    {
        //        Console.WriteLine("Failed to create the skillset\n Exception message: {0}\n", ex.Message);
        //        ExitProgram("Cannot continue without a skillset");
        //    }

        //    return skillset;
        //}
    }
}
