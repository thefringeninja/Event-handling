using System;
using System.Threading;
using NUnit.Framework;
using pvc.Adapters.ZeroMQ.Tests.Fixtures;
using pvc.Core;
using pvc.Core.Bus;

namespace pvc.Adapters.ZeroMQ.Tests.Bus
{
    [TestFixture]
    public class When_using_bus
    {
        [Serializable]
        public class FakeEvent : Event
        {
            public string Id { get; set; }

            public FakeEvent()
            {
                Id = Guid.NewGuid().ToString();
            }
        }

        [Test]
        public void a_published_message_is_received_by_a_subscriber()
        {
            // When the bus publishes a message, it is sent through the socket
            var consumer = new ZeroConsumer<Message>("tcp://localhost:5562");
            var bus = new BusAdapter();
            bus.AttachConsumer(consumer);

            // Meanwhile, the aggregator waits for messages from the same socket
            var producer = new ZeroProducer<Message>("tcp://*:5562");
            var aggregator = new EventAggregator<Message>();
            aggregator.AttachTo(producer);
            
            // While a test consumer is subscribed to the aggregator
            // (the syntax looks like it's the other way around)
            var test = new TestConsumer();
            aggregator.SubscribeTo(test);

            // When we drop a message on the bus, the test consumer should get it
            var @event = new FakeEvent();
            bus.Publish(@event);

            // Pause the thread so the producer (via aggregator) can send to the test consumer
            var timeout = TimeSpan.FromSeconds(1).TotalMilliseconds;
            Thread.Sleep((int)timeout);

            Assert.IsTrue(test.Received);
            producer.Dispose();
        }
    }
}
