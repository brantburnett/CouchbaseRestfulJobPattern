using System;

namespace CouchbaseRestfulJobPattern.Data.Documents
{
    public class JobIdentity
    {
        private const string TypeString = "jobIdentity";

        public long Id { get; set; }
        public string Type => TypeString;

        public static string GetKey() => TypeString;
    }
}
