using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CouchbaseRestfulJobPattern.Models
{
    public enum JobStatus
    {
        Queued = 0,
        Running = 1,
        Complete = 2
    }
}
