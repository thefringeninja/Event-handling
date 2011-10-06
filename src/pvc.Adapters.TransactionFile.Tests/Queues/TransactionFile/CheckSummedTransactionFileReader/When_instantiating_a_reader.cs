using System;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFile.CheckSummedTransactionFileReader
{
    [TestFixture]
    public class When_instantiating_a_reader
    {
        [Test]
        public void filename_is_required()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new CheckSummedTransactionFileReader<object>(null, "filename_is_required", new BinaryFormatter());
            });
        }

        [Test]
        public void formatter_is_required()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new CheckSummedTransactionFileReader<object>("filename.dat", "formatter_is_required", null);
            });
        }
    }
}