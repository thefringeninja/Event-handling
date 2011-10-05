using System.IO;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues;
using pvc.Adapters.TransactionFile.Tests._Fixtures;

namespace pvc.Adapters.TransactionFile.Tests.Integration
{
    [TestFixture]
    public class With_single_transaction_file_consumer_for_blocking_queue
    {
        private const string Filename = "With_single_transaction_file_consumer.dat";

        [Test]
        public void consumer_can_read_successfully_from_fully_populated_transaction_file()
        {
            BlockingQueueFixture.ResetTransactionFile(Filename);
            BlockingQueueFixture.ResetTransactionFile(Filename + ".write.chk");
            
            var queue = new TransactionFileBlockingQueue<object>(Filename);
            Assert.IsTrue(File.Exists(Filename), "File does not exist after creating a queue");
            Assert.AreEqual(0, new FileInfo(Filename).Length, "Transaction file is not empty");

            int[] produced;
            var volume = BlockingQueueFixture.ProduceAll(queue, out produced, 100);

            BlockingQueueFixture.ConsumeAll(produced[0], queue, volume);
        }
    }
}