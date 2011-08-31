using System;
using System.Linq;
using System.Threading;

namespace pvc.Adapters.RabbitMQ.Throttling
{
    /// <summary>
    /// A <see cref="Semaphore" />-based throttler that allows a threshold limit of resource reservations against a sequence
    /// <remarks>
    ///     - Once the threshold limit is hit, subsequent requests for the resource are blocked until others are released
    ///     - An internal sequence set is maintained for tracking resources
    /// </remarks>
    /// </summary>
    public class SequenceThrottle
    {
        private readonly Semaphore _confirmPool;
        
        private readonly ThreadSafeSortedSet<ulong> _unconfirmedSet = new ThreadSafeSortedSet<ulong>();

        private Predicate<ulong> _predicate;

        public ulong CurrentSequence
        {
            get
            {
                return _unconfirmedSet.LastOrDefault();
            }
        }

        public SequenceThrottle(int threshold)
        {
            _confirmPool = new Semaphore(threshold, threshold);
        }

        /// <summary>
        /// Removes a single sequence number from the internal sequence collection
        /// </summary>
        /// <param name="sequence"></param>
        public void RemoveOne(ulong sequence)
        {
            try
            {
                _unconfirmedSet.Remove(sequence);
            }
            finally
            {
                ReleaseResources(1);
            }
        }

        /// <summary>
        /// Removes all sequence numbers up to and including the given sequence number from the internal sequence collection
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public int RemoveMany(ulong sequence)
        {
            var confirmed = 0;
            try
            {
                _predicate = _predicate ?? (s => s < sequence + 1);
                confirmed = _unconfirmedSet.Count(s => _predicate(s));
                _unconfirmedSet.RemoveWhere(_predicate);
                return confirmed;
            }
            finally
            {
                ReleaseResources(confirmed);
            }
        }

        public void RequestResource(ulong sequence)
        {
            if(_confirmPool != null)
            {
                _confirmPool.WaitOne();
            }

            _unconfirmedSet.Add(sequence);
        }

        private void ReleaseResources(int resourceCount)
        {
            if (_confirmPool != null)
            {
                _confirmPool.Release(resourceCount);
            }
        }
    }
}