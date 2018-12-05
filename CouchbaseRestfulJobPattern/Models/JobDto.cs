using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CouchbaseRestfulJobPattern.Models
{
    public class JobDto
    {
        public long Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public JobStatus Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public StarDto CreateStar { get; set; }
    }
}
