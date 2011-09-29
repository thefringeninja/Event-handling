using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues;

namespace pvc.Adapters.TransactionFile.Tests.Integration
{
    [TestFixture]
    public class With_single_transaction_file_consumer
    {
        private const string Filename = "With_single_transaction_file_consumer.dat";

        [Test]
        public void consumer_can_read_successfully_from_fully_populated_transaction_file()
        {
            ResetTransactionFile(Filename);
            Assert.IsFalse(File.Exists(Filename), "Transaction should not exist");

            var queue = new TransactionFileBlockingQueue<object>(Filename);
            Assert.IsTrue(File.Exists(Filename), "File does not exist after creating a queue");
            Assert.AreEqual(0, new FileInfo(Filename).Length, "Transaction file is not empty");

            int[] produced;
            var volume = ProduceAll(queue, out produced, 600);

            ConsumeAll(produced[0], queue, volume);
        }

        public static void ResetTransactionFile(string filename)
        {
            if (!File.Exists(filename))
            {
                return;
            }
            File.Delete(filename);
            File.Delete(String.Concat(filename, ".chk"));
        }

        public static void PublishToQueue(IBlockingQueue<object> queue, int volume, int[] produced)
        {
            while (produced[0] < volume)
            {
                try
                {
                    queue.Enqueue(new object());
                    Interlocked.Increment(ref produced[0]);
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.Message);
                }
            }
        }

        private static void ConsumeAll(int produced, IBlockingQueue<object> queue, int volume)
        {
            int[] consumed = { 0 };
            while (consumed[0] < volume)
            {
                try
                {
                    Thread.Sleep(10);
                    var item = queue.Dequeue();
                    Assert.IsNotNull(item);
                    Interlocked.Increment(ref consumed[0]);
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.Message);
                }
            }

            while (consumed[0] < produced)
            {
                Assert.GreaterOrEqual(produced, consumed[0], "More messages reported consumed than produced");
            }

            Assert.AreEqual(produced, consumed[0], "Consumers reported complete before a count mismatch");
        }

        private static int ProduceAll(IBlockingQueue<object> queue, out int[] produced, int volume)
        {
            produced = new[] { 0 };
            PublishToQueue(queue, volume, produced);
            while (produced[0] < volume)
            {
                Assert.LessOrEqual(produced[0], volume, string.Format("More messages reported produced than requested: produced {0}, volume {1}", produced[0], volume));
            }
            Assert.AreEqual(produced[0], volume, string.Format("More messages produced than requested: produced {0}", produced[0]));
            return volume;
        }
    }
}