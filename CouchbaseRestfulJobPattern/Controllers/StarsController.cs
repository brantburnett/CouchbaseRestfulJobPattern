using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CouchbaseRestfulJobPattern.Data;
using CouchbaseRestfulJobPattern.Data.Documents;
using CouchbaseRestfulJobPattern.Models;
using CouchbaseRestfulJobPattern.Services;
using Microsoft.AspNetCore.Mvc;

namespace CouchbaseRestfulJobPattern.Controllers
{
    [Route("star")]
    [ApiController]
    public class StarsController : ControllerBase
    {
        private readonly StarRepository _starRepository;
        private readonly IMapper _mapper;
        private readonly JobService _jobService;

        public StarsController(StarRepository starRepository, IMapper mapper, JobService jobService)
        {
            _starRepository = starRepository ?? throw new ArgumentNullException(nameof(starRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        }

        // GET star
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StarDto>>> Get()
        {
            var stars = await _starRepository.GetAllStarsAsync();

            return stars.Select(p => _mapper.Map<StarDto>(p)).ToList();
        }

        // GET star/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StarDto>> Get(long id)
        {
            var result = await _starRepository.GetStarAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            return _mapper.Map<StarDto>(result);
        }

        // POST star
        [HttpPost]
        public async Task<ActionResult<StarDto>> Post([FromBody] StarDto star)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            star.Id = 0;
            var job = await _jobService.CreateStarJobAsync(_mapper.Map<Star>(star));

            return AcceptedAtAction("Get", "Jobs", new {id = job.Id});
        }

        // DELETE star/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _starRepository.DeleteStarAsync(id);

            return NoContent();
        }
    }
}
