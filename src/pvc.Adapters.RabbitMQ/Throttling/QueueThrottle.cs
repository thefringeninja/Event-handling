using System.Collections.Concurrent;
using System.Threading;

namespace pvc.Adapters.RabbitMQ.Throttling
{
    /// <summary>
    /// A simple throttle that permits enqueuing of an internal queue to a threshold count value, after which point subsequent
    /// requestors are blocked until under threshold 
    /// <remarks>
    ///     - There is no meter on the dequeuing process; it will exhaust as fast as possible
    /// </remarks>
    /// </summary>
    public class QueueThrottle
    {
        private readonly ConcurrentQueue<ulong> _queue;
        private readonly Semaphore _confirmPool;

        public int CurrentSize
        {
            get { return _queue.Count; }
        }

        public QueueThrottle(int threshold)
        {
            _queue = new ConcurrentQueue<ulong>();

            _confirmPool = new Semaphore(threshold, threshold);
        }
    
        public void RequestResource(ulong sequence)
        {
            _confirmPool.WaitOne();

            _queue.Enqueue(sequence);
        }

        public ulong ReleaseResource()
        {
            try
            {
                ulong result;
                _queue.TryDequeue(out result);
                return result;
            }
            finally
            {
                _confirmPool.Release();
            }
        }
    }
}