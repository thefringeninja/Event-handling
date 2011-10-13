using NUnit.Framework;
using pvc.Adapters.TransactionFile.Queues;
using pvc.Core;

namespace pvc.Adapters.TransactionFile.Tests
{
    [TestFixture]
    public class When_disposing_transaction_file_consumer
    {
        [Test]
        public void disposing_is_safe()
        {
            var consumer = new TransactionFileConsumer<Message>("disposing_transaction_file_consumer_is_safe");
            consumer.Dispose();

            Assert.IsNull(consumer.Writer);
        }
    }
}
