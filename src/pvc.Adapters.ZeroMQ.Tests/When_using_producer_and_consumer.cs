using NUnit.Framework;
using pvc.Adapters.ZeroMQ.Tests.Fixtures;
using pvc.Core;

namespace pvc.Adapters.ZeroMQ.Tests
{
    [TestFixture]
    public class When_using_producer_and_consumer
    {
        [Test]
        public void message_is_produced_and_consumed()
        {
            var test = new TestConsumer();

            using (var consumer = new ZeroConsumer<Message>("tcp://*:5562"))
            {
                using (var producer = new ZeroProducer<Message>("tcp://localhost:5562"))
                {
                    producer.AttachConsumer(test);

                    consumer.Handle(new TestMessage());

                    producer.Stop();
                }
            }

            Assert.IsTrue(test.Received);
        }
    }
}