using NUnit.Framework;
using pvc.Core;
using pvc.Tests.Fixtures;
using pvc.Tests.Messages;
using pvc.Tests.TestConsumer;

namespace pvc.Tests.Multiplexor
{
    [TestFixture]
    public class when_multiplexing_to_removed_consumer : ConsumerFixture<TestMessage> {
        TestConsumer<TestMessage> attachedConsumer;
        TestConsumer<TestMessage> removedConsumer;
		
        protected override Consumes<TestMessage> GivenConsumer ()
        {
            Multiplexor<TestMessage> m = new Multiplexor<TestMessage>();
            attachedConsumer = new TestConsumer<TestMessage>();
            removedConsumer = new TestConsumer<TestMessage>();
            m.AttachConsumer(removedConsumer);
            m.AttachConsumer(attachedConsumer);
            m.RemoveConsumer(removedConsumer);
            return m;
        }
		
        protected override TestMessage When ()
        {
            return new TestMessage(); 
        }
		
        [Test]
        public void attached_consumer_received_correct_message() {
            Assert.AreEqual(published, attachedConsumer.OnlyMessageReceived);
        }
		
        [Test]
        public void removed_consumer_does_not_receive_message() {
            Assert.IsFalse(removedConsumer.WasCalled);
        }
    }
}