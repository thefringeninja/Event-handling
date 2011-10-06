using System;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFile.TransactionFileWriter
{
    [TestFixture]
    public class When_instantiating_a_writer
    {
        [Test]
        public void filename_is_required()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new TransactionFileWriter<object>(null, new BinaryFormatter());
            });
        }

        [Test]
        public void formatter_is_required()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new TransactionFileWriter<object>("When_instantiating_a_writer.dat", null);
            });
        }

        [Test]
        public void name_is_set()
        {
            var writer = new TransactionFileWriter<object>("When_instantiating_a_writer", new BinaryFormatter());
            Assert.IsNotNullOrEmpty(writer.ChecksumName);
        }
    }
}