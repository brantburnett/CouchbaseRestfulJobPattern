using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CouchbaseRestfulJobPattern.Services
{
    /// <summary>
    /// Processes events from the message bus to start jobs, ensuring that concurrency
    /// is limited so the application instance isn't overloaded.
    /// </summary>
    public class JobProcessor : IDisposable
    {
        private readonly JobService _jobService;
        private readonly ILogger<JobProcessor> _logger;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        // Limit the number of simultaneous jobs processed by a single instance
        private readonly SemaphoreSlim _concurrencyLimiter = new SemaphoreSlim(2);

        private bool _started;
        private bool _disposed;

        public JobProcessor(JobService jobService, ILogger<JobProcessor> logger)
        {
            _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        private async Task Poll()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await _concurrencyLimiter.WaitAsync(_cts.Token);

#pragma warning disable 4014
                    _jobService.ProcessNextJobAsync(_cts.Token)
                        .ContinueWith(t =>
                        {
                            _concurrencyLimiter.Release();

                            if (t.IsFaulted)
                            {
                                _logger.LogError(t.Exception, "Unhandled exception in JobPoller");
                            }
                        }, _cts.Token);
#pragma warning restore 4014
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in JobPoller");
                }
            }
        }
    }
}
