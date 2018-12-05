using System;
using Couchbase.Linq.Filters;
using CouchbaseRestfulJobPattern.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CouchbaseRestfulJobPattern.Data.Documents
{
    [DocumentTypeFilter(TypeString)]
    public class Job
    {
        private const string TypeString = "job";

        public long Id { get; set; }
        public string Type => TypeString;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Star CreateStar { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public JobStatus Status { get; set; }

        public static string GetKey(long id) => $"{TypeString}-{id}";
    }
}
