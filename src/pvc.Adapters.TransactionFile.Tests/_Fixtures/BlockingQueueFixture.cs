using System;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace pvc.Adapters.TransactionFile.Tests._Fixtures
{
    public class BlockingQueueFixture
    {
        public static void ConsumeAll(int produced, IBlockingQueue<object> queue, int volume)
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

        public static int ProduceAll(IBlockingQueue<object> queue, out int[] produced, int volume)
        {
            produced = new[] { 0 };
            PublishToQueue(queue, volume, produced);
            while (produced[0] < volume)
            {
                Assert.LessOrEqual(produced[0], volume, String.Format("More messages reported produced than requested: produced {0}, volume {1}", produced[0], volume));
            }
            Assert.AreEqual(produced[0], volume, String.Format("More messages produced than requested: produced {0}", produced[0]));
            return volume;
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

        public static void ResetTransactionFile(string filename)
        {
            if (!File.Exists(filename))
            {
                return;
            }

            File.Delete(filename);
            File.Delete(String.Concat(filename, ".chk"));
            Assert.IsFalse(File.Exists(filename), "Transaction file should not exist");
        }
    }
}