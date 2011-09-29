using System;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFile.TransactionFileReader
{
    [TestFixture]
    public class When_instantiating_a_reader
    {
        [Test]
        public void filename_is_required()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new TransactionFileReader<object>(null, new BinaryFormatter());
            });
        }

        [Test]
        public void formatter_is_required()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new TransactionFileReader<object>("filename.dat", null);
            });
        }
    }
}