using NUnit.Framework;
using pvc.Core;
using pvc.Tests.Fixtures;
using pvc.Tests.Messages;
using pvc.Tests.TestConsumer;

namespace pvc.Tests.Multiplexor
{
    [TestFixture]
    public class when_multiplexing_to_multiple_constructed_consumers : ConsumerFixture<TestMessage> {
		
        TestConsumer<TestMessage> firstConsumer;
        TestConsumer<TestMessage> secondConsumer;
		
        protected override Consumes<TestMessage> GivenConsumer ()
        {
            firstConsumer = new TestConsumer<TestMessage>();
            secondConsumer = new TestConsumer<TestMessage>();
            var m = new Multiplexor<TestMessage>(new [] {firstConsumer, secondConsumer});
            return m;
        }
		
        protected override TestMessage When()
        {
            return new TestMessage(); 
        }
		
        [Test]
        public void first_consumer_received_only_correct_message() {
            Assert.AreEqual(published, firstConsumer.OnlyMessageReceived);
        }
		
        [Test]
        public void second_consumer_received_only_correct_message() {
            Assert.AreEqual(published, secondConsumer.OnlyMessageReceived);
        }
		
    }
}