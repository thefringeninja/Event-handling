using System;
using System.Threading;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues;
using pvc.Adapters.TransactionFile.Tests._Fixtures;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFileBlockingQueue
{
    [TestFixture]
    public class When_using_blocking_queue_from_existing_file : PopulatedTransactionFileFixture<object>
    {
        private readonly TransactionFileBlockingQueue<object> _queue;
        private const string Filename = "When_using_blocking_queue_from_existing_file.dat";

        public When_using_blocking_queue_from_existing_file() : base(Filename, 3)
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
        public void queue_blocks_when_exhausted_until_enqueued()
        {
            var count = 3;
            while (count > 0)
            {
                var sequential = _queue.Dequeue();
                Assert.IsNotNull(sequential);
                count--;
            }

            var background = false;
            ThreadPool.QueueUserWorkItem(s =>
            {
                Thread.Sleep(500);
                background = true;
                _queue.Enqueue(new object());
            });

            var final = _queue.Dequeue();
            Assert.IsNotNull(final);
            Assert.IsTrue(background);
        }

        [Test]
        public void record_is_available()
        {
            var queue = new TransactionFileBlockingQueue<object>(Filename);
            Assert.IsTrue(queue.RecordAvailable);
        }
    }
}
