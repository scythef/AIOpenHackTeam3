﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net;
using System.Text;

namespace Challenge5
{
    public static class Function1
    {
        #region classes used to serialize the response
        private class WebApiResponseError
        {
            public string message { get; set; }
        }

        private class WebApiResponseWarning
        {
            public string message { get; set; }
        }

        private class WebApiResponseRecord
        {
            public string recordId { get; set; }
            public Dictionary<string, object> data { get; set; }
            public List<WebApiResponseError> errors { get; set; }
            public List<WebApiResponseWarning> warnings { get; set; }
        }

        private class WebApiEnricherResponse
        {
            public List<WebApiResponseRecord> values { get; set; }
        }
        #endregion

        [FunctionName("FunctionChallenge5")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                string recordId = null;
                string originalText = null;

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation(requestBody);
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                if (data?.values == null)
                    return new BadRequestObjectResult(" Could not find values array");

                if (data?.values.HasValues == false || data?.values.First.HasValues == false)
                    return new BadRequestObjectResult(" Could not find valid records in values array");

                recordId = data?.values?.First?.recordId?.Value as string;
                originalText = data?.values?.First?.data?.text?.Value as string;

                if (recordId == null)
                    return new BadRequestObjectResult("recordId cannot be null");

                WebApiResponseRecord responseRecord = new WebApiResponseRecord();
                responseRecord.data = new Dictionary<string, object>();
                responseRecord.recordId = recordId;

                if (!String.IsNullOrWhiteSpace(originalText))
                    responseRecord.data.Add("top_words", TopFrequentWords(originalText));

                WebApiEnricherResponse response = new WebApiEnricherResponse();
                response.values = new List<WebApiResponseRecord>();
                response.values.Add(responseRecord);

                var result = JsonConvert.SerializeObject(response);
                log.LogInformation(result);
                return (ActionResult)new OkObjectResult(response);
            }
            catch (Exception e)
            {
                log.LogInformation(e.ToString());
            }

            return null;







            //log.LogInformation("C# HTTP trigger function processed a request.");

            //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;



            //// Test the function
            //string test_text = @"Four score and seven years ago our fathers brought forth on this continent, a new nation, conceived in Liberty, and dedicated to the proposition that all men are created equal.
            //    Now we are engaged in a great civil war, testing whether that nation, or any nation so conceived and so dedicated, can long endure. We are met on a great battlefield of that war.
            //    We have come to dedicate a portion of that field, as a final resting place for those who here gave their lives that that nation might live. It is altogether fitting and proper that we should do this.\
            //    But, in a larger sense, we can not dedicate, we can not consecrate, we can not hallow this ground. The brave men, living and dead, who struggled here, have consecrated it, far above our poor power to add or detract.\
            //    The world will little note, nor long remember what we say here, but it can never forget what they did here. It is for us the living, rather, to be dedicated here to the unfinished work which they who fought here have thus far so nobly advanced.\
            //    It is rather for us to be here dedicated to the great task remaining before us that from these honored dead we take increased devotion to that cause for which they gave the last full measure of devotion, that we here highly resolve that these dead shall not have died in vain; that this nation, under God, shall have a new birth of freedom and that government of the people, by the people, for the people, shall not perish from the earth.";

            //word_list test_result = get_top_ten_words(test_text);

            ////// print the object as JSON
            ////Console.WriteLine(JsonConvert.SerializeObject(test_result));
        

            ////string[] result = { "first", "second", "third" };
            //string[] result = get_top_ten_words(test_text).words.ToArray();
            //string responseMessage = JsonConvert.SerializeObject(result);


            ////string responseMessage = string.IsNullOrEmpty(name)
            ////    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            ////    : $"Hello, {name}. This HTTP triggered function executed successfully.";


            //var OKResult =  new OkObjectResult(responseMessage);
            //OKResult.ContentTypes.Add("application/json");

            ////return OKResult;

            //var response = new WebApiResponse
            //{
            //    Values = new List<OutputRecord>()
            //};

            //return (ActionResult)new OkObjectResult(response);


            ////HttpResponseMessage httpresponse = new HttpResponseMessage(HttpStatusCode.OK)
            ////{​
            ////    Content = new StringContent(responseMessage, Encoding.UTF8, "application/json")
            ////};​​
            ////return httpresponse;
        }



        //private class WebApiResponse
        //{
        //    public List<OutputRecord> Values { get; set; }
        //}

        //private class OutputRecord
        //{
        //    public class OutputRecordData
        //    {
        //        public string Name { get; set; } = "";
        //        public string Description { get; set; } = "";
        //        public string Source { get; set; } = "";
        //        public string SourceUrl { get; set; } = "";
        //        public string LicenseAttribution { get; set; } = "";
        //        public string LicenseUrl { get; set; } = "";
        //    }

        //    public class OutputRecordMessage
        //    {
        //        public string Message { get; set; }
        //    }

        //    public string RecordId { get; set; }
        //    public OutputRecordData Data { get; set; }
        //    public List<OutputRecordMessage> Errors { get; set; }
        //    public List<OutputRecordMessage> Warnings { get; set; }
        //}

        // class for results
        public class word_list
        {
            public List<string> words { get; set; }
        }

        public static string[] TopFrequentWords(string text, int topCount = 10)
        {
            //var cleanText = StopwordTool.RemoveStopwords(text);
            //return GetTopKWordsDic(cleanText, topCount).Keys.ToArray();
            return get_top_ten_words(text).words.ToArray();
        }

        public static word_list get_top_ten_words(string text)
        {

            // convert to lowercase
            text = text.ToLowerInvariant();
            List<string> words = text.Split(' ').ToList();

            //remove any non alphabet characters
            var onlyAlphabetRegEx = new Regex(@"^[A-z]+$");
            words = words.Where(f => onlyAlphabetRegEx.IsMatch(f)).ToList();

            // Array of stop words to be ignored
            string[] stopwords = { "", "i", "me", "my", "myself", "we", "our", "ours", "ourselves", "you",
                    "youre", "youve", "youll", "youd", "your", "yours", "yourself",
                    "yourselves", "he", "him", "his", "himself", "she", "shes", "her",
                    "hers", "herself", "it", "its", "itself", "they", "them", "thats",
                    "their", "theirs", "themselves", "what", "which", "who", "whom",
                    "this", "that", "thatll", "these", "those", "am", "is", "are", "was",
                    "were", "be", "been", "being", "have", "has", "had", "having", "do",
                    "does", "did", "doing", "a", "an", "the", "and", "but", "if", "or",
                    "because", "as", "until", "while", "of", "at", "by", "for", "with",
                    "about", "against", "between", "into", "through", "during", "before",
                    "after", "above", "below", "to", "from", "up", "down", "in", "out",
                    "on", "off", "over", "under", "again", "further", "then", "once", "here",
                    "there", "when", "where", "why", "how", "all", "any", "both", "each",
                    "few", "more", "most", "other", "some", "such", "no", "nor", "not",
                    "only", "own", "same", "so", "than", "too", "very", "s", "t", "can",
                    "will", "just", "don", "dont", "should", "shouldve", "now", "d", "ll",
                    "m", "o", "re", "ve", "y", "ain", "aren", "arent", "couldn", "couldnt",
                    "didn", "didnt", "doesn", "doesnt", "hadn", "hadnt", "hasn", "hasnt",
                    "havent", "isn", "isnt", "ma", "mightn", "mightnt", "mustn",
                    "mustnt", "needn", "neednt", "shan", "shant", "shall", "shouldn", "shouldnt", "wasn",
                    "wasnt", "weren", "werent", "won", "wont", "wouldn", "wouldnt"};
            words = words.Where(x => !stopwords.Contains(x)).ToList();

            // Find distict keywords by key and count, and then order by count.
            var keywords = words.GroupBy(x => x).OrderByDescending(x => x.Count());
            var klist = keywords.ToList();
            var numofWords = 10;
            if (klist.Count < 10)
                numofWords = klist.Count;

            // Return the first 10 words
            List<string> resList = new List<string>();
            for (int i = 0; i < numofWords; i++)
            {
                resList.Add(klist[i].Key);
            }

            // Return the first 10 words
            //string[] resList = { };
            //for (int i = 0; i < numofWords; i++)
            //{
            //    resList.Add(klist[i].Key);
            //}


            // Construct object for results
            word_list json_result = new word_list();
            json_result.words = resList;

            // return the results object
            return json_result;
        }
    }
}
