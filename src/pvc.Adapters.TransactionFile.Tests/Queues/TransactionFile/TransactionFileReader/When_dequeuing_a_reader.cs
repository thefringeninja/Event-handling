using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;
using pvc.Adapters.TransactionFile.Tests._Fixtures;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFile.TransactionFileReader
{
    [TestFixture]
    public class When_dequeuing_a_reader : PopulatedTransactionFileFixture<object>
    {
        private const string Filename = "When_dequeuing_a_reader.dat";

        public When_dequeuing_a_reader() : base(Filename, 3)
        {

        }

        [Test]
        public void dequeued_item_exists()
        {
            var reader = new TransactionFileReader<object>(Filename, new BinaryFormatter());
            var item = reader.Dequeue();
            Assert.IsNotNull(item);
        }

        [Test]
        public void file_exhaustion_causes_blocking_until_data_is_present()
        {
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
            
            var reader = new TransactionFileReader<object>(Filename, new BinaryFormatter());
            reader.Dequeue();
            reader.Dequeue();
            reader.Dequeue();
            reader.Dequeue();
            Assert.IsTrue(background, "Reader completed dequeuing but not from the background thread");
        }

        [Test]
        public void dequeues_can_come_from_multiple_consumers()
        {
            int[] count = { Count };
            var block = new ManualResetEvent(false);

            for (var i = 0; i < Count; i++)
            {
                ThreadPool.QueueUserWorkItem(
                    s =>
                    {
                        Thread.Sleep(200);
                        var reader = new TransactionFileReader<object>(Filename, new BinaryFormatter());
                        var item = reader.Dequeue();
                        Assert.IsNotNull(item);
                        Interlocked.Decrement(ref count[0]);
                        if(count[0] == 0)
                        {
                            block.Set();
                        }
                    });
            }

            block.WaitOne();
        }
    }
}