using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SearchClient
{
    public class WebsiteDocument
    {
        [System.ComponentModel.DataAnnotations.Key]
        [IsFilterable]
        public string Id { get; set; }

        [IsFilterable, IsSortable, IsFacetable, IsSearchable]
        public string Url { get; set; }

        [IsFilterable, IsSortable, IsFacetable]
        public string File_name { get; set; }

        [IsSearchable]
        public string Content { get; set; }

        [IsFilterable, IsSortable]
        public int Size { get; set; }

        [IsFilterable, IsSortable, IsFacetable]
        public DateTime Last_modified { get; set; }

        [IsSortable, IsFacetable, IsFilterable]
        public Double Sentiment_score { get; set; }

        [IsSearchable]
        public string[] Key_phrases { get; set; }

        [IsSearchable, IsFilterable]
        public string[] Persons { get; set; }
        
        [IsSearchable, IsFilterable]
        public string[] Locations { get; set; }
        
        [IsSearchable, IsFilterable]
        public string[] Urls { get; set; }

        [IsSearchable]
        public string Merged_text { get; set; }
        
        [IsSearchable]
        public string[] Extracted_text { get; set; }

        [IsSearchable]
        public string Categories { get; set; }

        [IsSearchable]
        public string[] Tags { get; set; }

        [IsSearchable]
        public string[] Description { get; set; }

        [IsSearchable]
        public string Faces { get; set; }

        [IsSearchable]
        public string Brands { get; set; }

        [IsSearchable]
        public string Objects { get; set; }
        
        [IsSearchable, IsFilterable]
        public string[] Celebrities { get; set; }
        
        [IsSearchable, IsFilterable]
        public string[] Landmarks { get; set; }

        [IsSearchable,IsFilterable]
        public string[] top_words {get; set;} 
    }

    public partial class Tag
    {
        [IsSearchable]
        public string Name { get; set; }

        public double Confidence { get; set; }
        public string Hint { get; set; }
    }

    public class Description
    {
        [IsSearchable]
        public string[] Tags { get; set; }

        [IsSearchable] 
        public Tag[] Captions { get; set; }
    }

}