using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

// https://azure.github.io/LearnAI-Cognitive-Search/06-Lab-3-Custom-Skills.html

namespace ASFunc
{
    public static class AzureSearchFce
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



        [FunctionName("AzureSearchFce")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
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
            catch(Exception e)
            {
                log.LogInformation(e.ToString());
            }

            return null;
        }

        public static string[] TopFrequentWords(string text, int topCount = 10)
        {
            var cleanText = StopwordTool.RemoveStopwords(text);
            return GetTopKWordsDic(cleanText, topCount).Keys.ToArray();
        }

        static Dictionary<string, int> GetTopKWordsDic(string input, int k)
        {
            string[] words = Regex.Split(input, @"\W");
            var occurrences = new Dictionary<string, int>();

            foreach (var word in words)
            {
                if (String.IsNullOrWhiteSpace(word))
                    continue;
                string lowerWord = word.ToLowerInvariant();
                if (!occurrences.ContainsKey(lowerWord))
                    occurrences.Add(lowerWord, 1);
                else
                    occurrences[lowerWord]++;
            }
            return (from wp in occurrences.OrderByDescending(kvp => kvp.Value) select wp).Take(k).ToDictionary(kw => kw.Key, kw => kw.Value);
        }
    }
}
