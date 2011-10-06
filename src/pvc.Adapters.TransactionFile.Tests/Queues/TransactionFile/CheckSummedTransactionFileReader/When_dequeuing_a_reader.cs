using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;
using pvc.Adapters.TransactionFile.Tests._Fixtures;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFile.CheckSummedTransactionFileReader
{
    [TestFixture]
    public class When_dequeuing_a_reader : PopulatedTransactionFileFixture<object>
    {
        private const string Filename = "When_dequeuing_a_checksummed_reader";

        public When_dequeuing_a_reader() : base(Filename, 3)
        {

        }

        [Test]
        public void dequeued_item_exists()
        {
            const string filename = "dequeued_item_exists";
            CleanChecksumFile(filename);

            var reader = new CheckSummedTransactionFileReader<object>(Filename, "dequeued_item_exists", new BinaryFormatter());
            var item = reader.Dequeue();
            Assert.IsNotNull(item);
        }

        [Test]
        public void file_exhaustion_causes_blocking_until_data_is_present()
        {
            const string filename = "file_exhaustion_causes_blocking_until_data_is_present";
            CleanChecksumFile(filename);

            var background = false;
            ThreadPool.QueueUserWorkItem(
                s =>
                    {
                        Thread.Sleep(200);
                        var writer = new TransactionFileWriter<object>(Filename, new BinaryFormatter());
                        background = true;
                        writer.Enqueue(new object());
                    }
                );

            var reader = new CheckSummedTransactionFileReader<object>(Filename, filename, new BinaryFormatter());
            reader.Dequeue();
            reader.Dequeue();
            reader.Dequeue();
            reader.Dequeue();
            Assert.IsTrue(background, "Reader completed dequeuing but not from the background thread");
        }

        [Test]
        public void dequeues_can_come_from_multiple_consumers()
        {
            const string filename = "dequeues_can_come_from_multiple_consumers";
            CleanChecksumFile(filename);

            int[] count = { Count };
            var block = new ManualResetEvent(false);

            for (var i = 0; i < Count; i++)
            {
                ThreadPool.QueueUserWorkItem(
                    s =>
                    {
                        Thread.Sleep(200);
                        var reader = new CheckSummedTransactionFileReader<object>(Filename, filename, new BinaryFormatter());
                        var item = reader.Dequeue();
                        Assert.IsNotNull(item);
                        Interlocked.Decrement(ref count[0]);
                        if (count[0] == 0)
                        {
                            block.Set();
                        }
                    });
            }

            block.WaitOne();
        }

        private static void CleanChecksumFile(string filename)
        {
            if (File.Exists(filename + ".read.chk"))
            {
                File.Delete(filename + ".read.chk");
            }
            Assert.IsFalse(File.Exists(filename + ".read.chk"));
        }
    }
}