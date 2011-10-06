using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFile.TransactionFileReader
{
    [TestFixture]
    public class When_disposing_a_reader
    {
        [Test]
        public void filestream_is_disposed()
        {
            var reader = new TransactionFileReader<object>("When_disposing_a_writer", new BinaryFormatter());
            reader.Dispose();
            Assert.IsNull(reader.FileStream);
        }
    }
}