using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Checksums;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;
using pvc.Adapters.TransactionFile.Tests._Fixtures;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFile.TransactionFileWriter
{
    [TestFixture]
    public class When_enqueuing_a_writer : EmptyTransactionFileFixture
    {
        private const string Filename = "When_enqueuing_a_writer";

        public When_enqueuing_a_writer() : base(Filename)
        {

        }

        [Test]
        public void enqueued_item_exists_when_dequeued()
        {
            var writer = new TransactionFileWriter<object>(Filename, new BinaryFormatter());
            writer.Enqueue(new object());

            var reader = new TransactionFileReader<object>(Filename, new BinaryFormatter());
            var item = reader.Dequeue();
            Assert.IsNotNull(item);
        }

        [Test]
        public void enqueues_can_come_from_multiple_consumers()
        {
            const int trials = 100;
            int[] count = { 0 };
            var block = new ManualResetEvent(false);
            
            for (var i = 0; i < trials; i++)
            {
                ThreadPool.QueueUserWorkItem(
                    s =>
                    {
                        var writer = new TransactionFileWriter<object>(Filename, new BinaryFormatter());
                        writer.Enqueue(new object());
                        Interlocked.Increment(ref count[0]);
                        if (count[0] == trials)
                        {
                            block.Set();
                        }
                    });
            }
            block.WaitOne();

            var checksum = new FileChecksum(Filename + ".write.chk");
            Assert.GreaterOrEqual(checksum.GetValue(), 4100, "Final writer checksum for the message content did not match expectations");
        }
    }
}