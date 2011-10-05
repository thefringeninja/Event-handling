using System.Threading;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues;
using pvc.Adapters.TransactionFile.Tests._Fixtures;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFileBlockingQueue
{
    [TestFixture]
    public class When_using_blocking_queue_from_empty_file : EmptyTransactionFileFixture<object>
    {
        private readonly TransactionFileBlockingQueue<object> _queue;
        private const string Filename = "When_using_blocking_queue_from_empty_file.dat";

        public When_using_blocking_queue_from_empty_file() : base(Filename)
        {
            _queue = new TransactionFileBlockingQueue<object>(Filename);
            _queue.Enqueue(new object());
            _queue.Enqueue(new object());
            _queue.Enqueue(new object());
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
            while (_queue.Count > 0)
            {
                var sequential = _queue.Dequeue();
                Assert.IsNotNull(sequential);
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
    }
}