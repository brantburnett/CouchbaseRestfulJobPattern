using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CouchbaseRestfulJobPattern.Data;
using CouchbaseRestfulJobPattern.Models;
using Microsoft.AspNetCore.Mvc;

namespace CouchbaseRestfulJobPattern.Controllers
{
    [Route("job")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly JobRepository _jobRepository;
        private readonly IMapper _mapper;

        public JobsController(JobRepository jobRepository, IMapper mapper)
        {
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // GET job
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobDto>>> Get()
        {
            var jobs = await _jobRepository.GetAllJobsAsync();

            return jobs.Select(p => _mapper.Map<JobDto>(p)).ToList();
        }

        // GET job/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JobDto>> Get(long id)
        {
            var result = await _jobRepository.GetJobAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            return _mapper.Map<JobDto>(result);
        }
    }
}
