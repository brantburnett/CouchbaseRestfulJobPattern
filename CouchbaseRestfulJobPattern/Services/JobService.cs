using System;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Extensions.Locks;
using CouchbaseRestfulJobPattern.Data;
using CouchbaseRestfulJobPattern.Data.Documents;
using CouchbaseRestfulJobPattern.Models;
using Microsoft.Extensions.Logging;

namespace CouchbaseRestfulJobPattern.Services
{
    /// <summary>
    /// Service for creating and executing jobs.
    /// </summary>
    public class JobService
    {
        private readonly JobRepository _jobRepository;
        private readonly StarRepository _starRepository;
        private readonly MessageBusService _messageBus;
        private readonly ILogger<JobService> _logger;


        public JobService(JobRepository jobRepository, StarRepository starRepository, MessageBusService messageBus,
            ILogger<JobService> logger)
        {
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
            _starRepository = starRepository ?? throw new ArgumentNullException(nameof(starRepository));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Job> CreateStarJobAsync(Star star)
        {
            var job = new Job
            {
                CreateStar = star,
                Status = JobStatus.Queued
            };

            await _jobRepository.CreateJobAsync(job);

            QueueJob(job.Id);

            return job;
        }

        public void QueueJob(long id)
        {
            _messageBus.SendMessage(new Message {JobId = id});
        }

        public async Task ProcessNextJobAsync(CancellationToken cancellationToken)
        {
            var message = await _messageBus.ReceiveMessage(cancellationToken);
            if (message != null)
            {
                await ExecuteJobAsync(message.JobId, cancellationToken);
            }
        }

        /// <returns>True if the job was processed, or false if it was locked or already complete.</returns>
        public async Task<bool> ExecuteJobAsync(long id, CancellationToken token)
        {

            try
            {
                using (var mutex = await _jobRepository.LockJobAsync(id, TimeSpan.FromMinutes(1)))
                {
                    mutex.AutoRenew(TimeSpan.FromSeconds(15), TimeSpan.FromHours(1));

                    // Once we have the lock, reload the document to make sure it's still pending
                    var job = await _jobRepository.GetJobAsync(id);
                    if (job.Status == JobStatus.Complete)
                    {
                        return false;
                    }

                    // Update the status to Running
                    job.Status = JobStatus.Running;
                    await _jobRepository.UpdateJobAsync(job, null);

                    // We're just emulating a long running job here, so just delay
                    await Task.Delay(TimeSpan.FromSeconds(45), token);

                    // To emulate a failed job, either throw an exception here
                    // Or stop the app before the delay above is reached

                    // Finish creating the star
                    await _starRepository.CreateStarAsync(job.CreateStar);

                    // Update the job status document
                    job.Status = JobStatus.Complete;
                    await _jobRepository.UpdateJobAsync(job, TimeSpan.FromDays(1));
                }

                return true;
            }
            catch (CouchbaseLockUnavailableException)
            {
                return false;
            }
        }
    }
}
