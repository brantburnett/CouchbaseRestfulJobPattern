using System;
using System.ComponentModel.DataAnnotations;

namespace CouchbaseRestfulJobPattern.Models
{
    public class StarDto
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
