using System.IO;
using System.Threading;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Checksums;
using pvc.Adapters.TransactionFile.Queues;
using pvc.Adapters.TransactionFile.Tests._Fixtures;

namespace pvc.Adapters.TransactionFile.Tests.Integration
{
    [TestFixture]
    public class With_multiple_transaction_file_consumers_for_blocking_queue
    {
        private const string Filename = "With_multiple_transaction_file_consumers_for_blocking_queue.dat";

        [Test]
        public void consumers_can_read_successfully_from_fully_populated_transaction_file()
        {
            var block = new ManualResetEvent(false);
            BlockingQueueFixture.ResetTransactionFile(Filename);
            BlockingQueueFixture.ResetTransactionFile(Filename + ".write.chk");
            BlockingQueueFixture.ResetTransactionFile("checksum.read.chk");

            var queue = new TransactionFileBlockingQueue<object>(Filename);
            Assert.IsTrue(File.Exists(Filename), "File does not exist after creating a queue");
            Assert.AreEqual(0, new FileInfo(Filename).Length, "Transaction file is not empty");

            int[] produced;
            const int volume = 600;

            BlockingQueueFixture.ProduceAll(queue, out produced, volume);
            Assert.AreEqual(queue.Count, volume);

            var checksum = new FileChecksum(Filename + ".write.chk");
            var value = checksum.GetValue();
            Assert.AreEqual(24600, value, "Internal file checksum had an unexpected write value");
            checksum.Dispose();

            for (var i = 0; i < 10; i++)
            {
                ThreadPool.QueueUserWorkItem(
                    s =>
                    {
                            while(true)
                            {
                                Thread.Sleep(10);
                                queue.Dequeue();
                                if (queue.Count != 0)
                                {
                                    continue;
                                }

                                block.Set();
                                break;
                            }
                        }
                    );
            }

            block.WaitOne();
        }
    }
}