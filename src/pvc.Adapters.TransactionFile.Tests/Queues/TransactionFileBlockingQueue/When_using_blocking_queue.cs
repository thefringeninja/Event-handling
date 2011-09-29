using System;
using System.Threading;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues;
using pvc.Adapters.TransactionFile.Tests.Fixtures;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFileBlockingQueue
{
    [TestFixture]
    public class When_using_blocking_queue : PopulatedTransactionFileFixture<object>
    {
        private readonly TransactionFileBlockingQueue<object> _queue;
        private const string Filename = "When_using_blocking_queue.dat";

        public When_using_blocking_queue() : base(Filename, 3)
        {
            _queue = new TransactionFileBlockingQueue<object>(Filename);
        }

        [Test]
        public void filename_is_required()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new TransactionFileBlockingQueue<object>(null);
            });
        }

        [Test]
        public void count_increments_and_decrements_when_queue_is_enqueued_and_dequeued()
        {
            Assert.AreEqual(3, _queue.Count);

            while (_queue.Count > 0)
            {
                _queue.Dequeue();
            }

            Assert.AreEqual(0, _queue.Count);
        }

        [Test]
        public void queue_blocks_when_exhausted_until_enqueued()
        {
            // TODO: Replace with _queue.Count when that test passes
            var count = 3;
            while (count  /* _queue.Count */> 0)
            {
                var sequential = _queue.Dequeue();
                Assert.IsNotNull(sequential);
                count--;
            }
            
            var background = false;
            ThreadPool.QueueUserWorkItem(s =>
                                             {
                                                 Thread.Sleep(2000);
                                                 background = true;
                                                 _queue.Enqueue(new object());
                                             });

            var final = _queue.Dequeue();
            Assert.IsNotNull(final);
            Assert.IsTrue(background);
        }
    }
}
