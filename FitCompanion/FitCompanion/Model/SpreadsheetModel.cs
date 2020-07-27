using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FitCompanion.Model
{
    class SpreadsheetModel
    {

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class Id
        {

            [JsonProperty("$t")]
            public string T { get; set; }

        }

        public class Updated
        {

            [JsonProperty("$t")]
            public DateTime T { get; set; }

        }

        public class Category
        {

            [JsonProperty("scheme")]
            public string Scheme { get; set; }

            [JsonProperty("term")]
            public string Term { get; set; }

        }

        public class Title
        {

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("$t")]
            public string T { get; set; }

        }

        public class Link
        {

            [JsonProperty("rel")]
            public string Rel { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("href")]
            public string Href { get; set; }

        }

        public class Name
        {

            [JsonProperty("$t")]
            public string T { get; set; }

        }

        public class Email
        {

            [JsonProperty("$t")]
            public string T { get; set; }

        }

        public class Author
        {

            [JsonProperty("name")]
            public Name Name { get; set; }

            [JsonProperty("email")]
            public Email Email { get; set; }

        }

        public class OpenSearchTotalResults
        {

            [JsonProperty("$t")]
            public string T { get; set; }

        }

        public class OpenSearchStartIndex
        {

            [JsonProperty("$t")]
            public string T { get; set; }

        }

        public class GsRowCount
        {

            [JsonProperty("$t")]
            public string T { get; set; }

        }

        public class GsColCount
        {

            [JsonProperty("$t")]
            public string T { get; set; }

        }

        public class Id2
        {

            [JsonProperty("$t")]
            public string T { get; set; }

        }

        public class Updated2
        {

            [JsonProperty("$t")]
            public DateTime T { get; set; }

        }

        public class Category2
        {

            [JsonProperty("scheme")]
            public string Scheme { get; set; }

            [JsonProperty("term")]
            public string Term { get; set; }

        }

        public class Title2
        {

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("$t")]
            public string T { get; set; }

        }

        public class Content
        {

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("$t")]
            public string T { get; set; }

        }

        public class Link2
        {

            [JsonProperty("rel")]
            public string Rel { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("href")]
            public string Href { get; set; }

        }

        public class GsCell
        {

            [JsonProperty("row")]
            public string Row { get; set; }

            [JsonProperty("col")]
            public string Col { get; set; }

            [JsonProperty("inputValue")]
            public string InputValue { get; set; }

            [JsonProperty("$t")]
            public string T { get; set; }

            [JsonProperty("numericValue")]
            public string NumericValue { get; set; }

        }

        public class Entry
        {

            [JsonProperty("id")]
            public Id2 Id { get; set; }

            [JsonProperty("updated")]
            public Updated2 Updated { get; set; }

            [JsonProperty("category")]
            public List<Category2> Category { get; set; }

            [JsonProperty("title")]
            public Title2 Title { get; set; }

            [JsonProperty("content")]
            public Content Content { get; set; }

            [JsonProperty("link")]
            public List<Link2> Link { get; set; }

            [JsonProperty("gs$cell")]
            public GsCell GsCell { get; set; }

        }

        public class Feed
        {

            [JsonProperty("xmlns")]
            public string Xmlns { get; set; }

            [JsonProperty("xmlns$openSearch")]
            public string XmlnsOpenSearch { get; set; }

            [JsonProperty("xmlns$batch")]
            public string XmlnsBatch { get; set; }

            [JsonProperty("xmlns$gs")]
            public string XmlnsGs { get; set; }

            [JsonProperty("id")]
            public Id Id { get; set; }

            [JsonProperty("updated")]
            public Updated Updated { get; set; }

            [JsonProperty("category")]
            public List<Category> Category { get; set; }

            [JsonProperty("title")]
            public Title Title { get; set; }

            [JsonProperty("link")]
            public List<Link> Link { get; set; }

            [JsonProperty("author")]
            public List<Author> Author { get; set; }

            [JsonProperty("openSearch$totalResults")]
            public OpenSearchTotalResults OpenSearchTotalResults { get; set; }

            [JsonProperty("openSearch$startIndex")]
            public OpenSearchStartIndex OpenSearchStartIndex { get; set; }

            [JsonProperty("gs$rowCount")]
            public GsRowCount GsRowCount { get; set; }

            [JsonProperty("gs$colCount")]
            public GsColCount GsColCount { get; set; }

            [JsonProperty("entry")]
            public List<Entry> Entry { get; set; }

        }

        public class Root
        {

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("encoding")]
            public string Encoding { get; set; }

            [JsonProperty("feed")]
            public Feed Feed { get; set; }

        }

    }
}

