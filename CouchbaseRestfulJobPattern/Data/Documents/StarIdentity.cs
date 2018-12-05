using System;

namespace CouchbaseRestfulJobPattern.Data.Documents
{
    public class StarIdentity
    {
        private const string TypeString = "starIdentity";

        public long Id { get; set; }
        public string Type => TypeString;

        public static string GetKey() => TypeString;
    }
}
