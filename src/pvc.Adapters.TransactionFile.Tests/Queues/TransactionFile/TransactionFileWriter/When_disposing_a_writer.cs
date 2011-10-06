using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFile.TransactionFileWriter
{
    [TestFixture]
    public class When_disposing_a_writer
    {
        [Test]
        public void filestream_is_disposed()
        {
            var writer = new TransactionFileWriter<object>("When_disposing_a_writer", new BinaryFormatter());
            writer.Dispose();
            Assert.IsNull(writer.FileStream);
        }
    }
}