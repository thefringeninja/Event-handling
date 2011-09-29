using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;
using pvc.Adapters.TransactionFile.Tests.Fixtures;

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
                        Thread.Sleep(100);
                        var writer = new TransactionFileWriter<object>(Filename, new BinaryFormatter());
                        writer.Enqueue(new object());
                        background = true;
                    }
                );
            
            var reader = new TransactionFileReader<object>(Filename, new BinaryFormatter());
            reader.Dequeue();
            reader.Dequeue();
            reader.Dequeue();
            reader.Dequeue();
            Assert.IsTrue(background, "Reader completed dequeuing but not from the background thread");
        }
    }
}