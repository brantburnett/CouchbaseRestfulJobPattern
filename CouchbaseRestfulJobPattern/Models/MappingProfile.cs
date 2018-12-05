using System;
using AutoMapper;
using CouchbaseRestfulJobPattern.Data.Documents;

namespace CouchbaseRestfulJobPattern.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Star, StarDto>();
            CreateMap<StarDto, Star>();

            CreateMap<Job, JobDto>();
            CreateMap<JobDto, Job>();
        }
    }
}
