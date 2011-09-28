using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues;

namespace pvc.Adapters.TransactionFile.Tests
{
    [TestFixture]
    public class TransactionFileBlockingQueueTests
    {
        [Test]
        public void Can_block_with_multiple_queue_consumers()
        {
            var queue = new TransactionFileBlockingQueue<object>("Can_block_with_multiple_queue_consumers.dat");
            queue.Enqueue(new object());
            queue.Enqueue(new object());
            queue.Enqueue(new object());


        }
    }
}
