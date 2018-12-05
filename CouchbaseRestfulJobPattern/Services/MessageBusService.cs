using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CouchbaseRestfulJobPattern.Services
{
    /// <summary>
    /// Emulates a message bus, such as Kafka or RabbitMQ, for the sake of this example.
    /// </summary>
    public class MessageBusService
    {
        private readonly ConcurrentQueue<Message> _queue = new ConcurrentQueue<Message>();

        public void SendMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _queue.Enqueue(message);
        }

        public async Task<Message> ReceiveMessage(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_queue.TryDequeue(out var message))
                    {
                        return message;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Return null on cancellation
            }

            return null;
        }
    }
}
