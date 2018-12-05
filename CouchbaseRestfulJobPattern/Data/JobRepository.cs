using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Core;
using Couchbase.Extensions.Locks;
using Couchbase.IO;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using CouchbaseRestfulJobPattern.Data.Documents;
using CouchbaseRestfulJobPattern.Models;

namespace CouchbaseRestfulJobPattern.Data
{
    public class JobRepository
    {
        private readonly IBucket _bucket;

        public JobRepository(IDefaultBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket();
        }

        public Task<IEnumerable<Job>> GetAllJobsAsync()
        {
            var context = new BucketContext(_bucket);

            return context.Query<Job>()
                .OrderBy(p => p.Id)
                .ExecuteAsync();
        }

        public Task<IEnumerable<Job>> GetIncompleteJobsAsync()
        {
            var context = new BucketContext(_bucket);

            return context.Query<Job>()
                .Where(p => p.Status != JobStatus.Complete)
                .OrderBy(p => p.Id)
                .ExecuteAsync();
        }

        public async Task<Job> GetJobAsync(long id)
        {
            var result = await _bucket.GetDocumentAsync<Job>(Job.GetKey(id));
            if (result.Status == ResponseStatus.KeyNotFound)
            {
                return null;
            }

            // Throw an exception on a low-level error
            result.EnsureSuccess();

            return result.Content;
        }

        public async Task CreateJobAsync(Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            job.Id = await GetNextJobIdAsync();

            var document = new Document<Job>
            {
                Id = Job.GetKey(job.Id),
                Content = job
            };

            var result = await _bucket.InsertAsync(document);
            result.EnsureSuccess();
        }

        public async Task UpdateJobAsync(Job job, TimeSpan? expiration)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            var document = new Document<Job>
            {
                Id = Job.GetKey(job.Id),
                Expiry = (uint) (expiration?.TotalMilliseconds ?? 0),
                Content = job
            };

            var result = await _bucket.ReplaceAsync(document);
            result.EnsureSuccess();
        }

        public Task<ICouchbaseMutex> LockJobAsync(long id, TimeSpan expiration)
        {
            return _bucket.RequestMutexAsync(Job.GetKey(id), expiration);
        }

        private async Task<long> GetNextJobIdAsync()
        {
            var key = JobIdentity.GetKey();

            var builder = _bucket.MutateIn<JobIdentity>(key);
            builder.Counter(p => p.Id, 1, true);

            var result = await builder.ExecuteAsync();
            if (result.Status == ResponseStatus.KeyNotFound)
            {
                var content = new JobIdentity
                {
                    Id = 0
                };

                var document = new Document<JobIdentity>
                {
                    Content = content,
                    Id = key
                };

                var insertResult = await _bucket.InsertAsync(document);
                if (insertResult.Status == ResponseStatus.KeyExists || insertResult.Status == ResponseStatus.Success)
                {
                    // Document was created by us or by another service, try to increment again
                    return await GetNextJobIdAsync();
                }

                // Document was not created, throw error
                insertResult.EnsureSuccess();
            }

            result.EnsureSuccess();

            return result.Content(c => c.Id);
        }
    }
}
