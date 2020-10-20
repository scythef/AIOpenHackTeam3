using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}