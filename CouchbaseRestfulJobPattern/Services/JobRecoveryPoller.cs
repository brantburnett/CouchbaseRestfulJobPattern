using System;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Extensions.Locks;
using CouchbaseRestfulJobPattern.Data;
using Microsoft.Extensions.Logging;

namespace CouchbaseRestfulJobPattern.Services
{
    /// <summary>
    /// Polls at a regular interval to find incomplete jobs and attempt to requeue them
    /// via the message bus. This will restart any jobs that were dropped.
    /// </summary>
    public class JobRecoveryPoller : IDisposable
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

        private readonly JobService _jobService;
        private readonly JobRepository _jobRepository;
        private readonly ILogger<JobRecoveryPoller> _logger;
        private readonly IBucket _bucket;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private bool _started;
        private bool _disposed;


        public JobRecoveryPoller(JobService jobService, JobRepository jobRepository, IDefaultBucketProvider bucketProvider,
            ILogger<JobRecoveryPoller> logger)
        {
            _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _bucket = bucketProvider.GetBucket();
        }

        public void Start()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(JobProcessor));
            }

            if (!_started)
            {
                Task.Run(Poll);

                _started = true;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cts.Cancel();

                _disposed = true;
            }
        }

        public async Task Poll()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    // Wait for the poll interval
                    await Task.Delay(PollInterval, _cts.Token);

                    // Take out a lock for job recovery polling
                    // Don't dispose so that it holds until it expires after PollInterval
                    // This will ensure only one instance of the app polls every poll interval
                    await _bucket.RequestMutexAsync("jobRecoveryPoller", PollInterval);

                    var jobs = await _jobRepository.GetIncompleteJobsAsync();
                    foreach (var job in jobs)
                    {
                        try
                        {
                            // Try to lock the job to see if it's being processed currently
                            using (_jobRepository.LockJobAsync(job.Id, TimeSpan.FromSeconds(1)))
                            {
                            }

                            // Make sure we've unlocked the job before we get here
                            // And fire events into the message bus for the unhandled job
                            // This allows any instance with capacity to pick up the job
                            _jobService.QueueJob(job.Id);
                        }
                        catch (CouchbaseLockUnavailableException)
                        {
                            // Job is already being processed, ignore
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (CouchbaseLockUnavailableException)
                {
                    // Unable to lock for singleton job recovery poller process, ignore
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in JobRecoveryPoller");
                }
            }
        }
    }
}
