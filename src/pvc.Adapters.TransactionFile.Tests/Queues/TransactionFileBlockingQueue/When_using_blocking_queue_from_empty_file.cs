using System.IO;
using System.Threading;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFileBlockingQueue
{
    [TestFixture]
    public class When_using_blocking_queue_from_empty_file
    {
        [Test]
        public void can_force_empty_a_queue_using_blocking_flag()
        {
            ResetTransactionFile("can_force_empty_a_queue_using_blocking_flag");

            var queue = new TransactionFileBlockingQueue<object>("can_force_empty_a_queue_using_blocking_flag");
            queue.Enqueue(new object());
            queue.Enqueue(new object());
            queue.Enqueue(new object());

            Assert.AreEqual(3, queue.Count);

            ForceEmptyQueue(queue);

            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void record_is_not_available_until_enqueued()
        {
            ResetTransactionFile("record_is_not_available_until_enqueued");
            var queue = new TransactionFileBlockingQueue<object>("record_is_not_available_until_enqueued");
            
            Assert.IsFalse(queue.RecordAvailable);

            queue.Enqueue(new object());

            Assert.IsTrue(queue.RecordAvailable);
        }

        [Test]
        public void count_increments_and_decrements_when_queue_is_enqueued_and_dequeued()
        {
            ResetTransactionFile("count_increments_and_decrements_when_queue_is_enqueued_and_dequeued");
            var queue = new TransactionFileBlockingQueue<object>("count_increments_and_decrements_when_queue_is_enqueued_and_dequeued");
            
            queue.Enqueue(new object());
            queue.Enqueue(new object());
            queue.Enqueue(new object());

            Assert.AreEqual(3, queue.Count);

            while (queue.Count > 0)
            {
                queue.Dequeue();
            }

            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void queue_blocks_when_exhausted_until_enqueued()
        {
            ResetTransactionFile("queue_blocks_when_exhausted_until_enqueued");
            var queue = new TransactionFileBlockingQueue<object>("queue_blocks_when_exhausted_until_enqueued");
            
            queue.Enqueue(new object());
            queue.Enqueue(new object());
            queue.Enqueue(new object());

            Assert.AreEqual(3, queue.Count);

            while (queue.Count > 0)
            {
                var sequential = queue.Dequeue();
                Assert.IsNotNull(sequential);
            }

            var background = false;
            ThreadPool.QueueUserWorkItem(s =>
            {
                Thread.Sleep(500);
                background = true;
                queue.Enqueue(new object());
            });

            var final = queue.Dequeue();
            Assert.IsNotNull(final);
            Assert.IsTrue(background);

            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void requeuing_is_equivalent_to_enqueuing()
        {
            ResetTransactionFile("requeuing_is_equivalent_to_enqueuing");
            var queue = new TransactionFileBlockingQueue<object>("requeuing_is_equivalent_to_enqueuing");

            queue.Enqueue(new object());
            queue.Enqueue(new object());
            queue.Enqueue(new object());

            var first = queue.Dequeue();
            queue.Dequeue();
            queue.Dequeue();

            Assert.AreEqual(0, queue.Count);

            queue.Requeue(first);
            queue.Dequeue();

            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void marking_complete_does_nothing()
        {
            ResetTransactionFile("marking_complete_does_nothing");
            var queue = new TransactionFileBlockingQueue<object>("marking_complete_does_nothing");
            queue.Enqueue(new object());

            var item = queue.Dequeue();
            queue.MarkComplete(item);

            Assert.AreEqual(0, queue.Count);
        }

        private static void ForceEmptyQueue(TransactionFileBlockingQueue<object> queue)
        {
            var cancel = false;
            ThreadPool.QueueUserWorkItem(
                s =>
                {
                    while (true)
                    {
                        if (!queue.IsBlocking)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        cancel = true;
                        queue.Enqueue(new object());
                        break;
                    }
                });

            while (!cancel)
            {
                queue.Dequeue();
            }
        }

        private static void ResetTransactionFile(string filename)
        {
            var checksum = string.Concat(filename, ".write.chk");
            if (!File.Exists(filename) && !File.Exists(checksum))
            {
                return;
            }
            File.Delete(filename);
            File.Delete(checksum);
        }
    }
}