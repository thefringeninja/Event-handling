using NUnit.Framework;
using pvc.Core;
using pvc.Tests.Fixtures;
using pvc.Tests.Messages;

namespace pvc.Tests.TestConsumer
{
    [TestFixture]
    public class when_test_consumer_receives_a_single_message : ConsumerFixture<TestMessage> {
		
        TestConsumer<TestMessage> sut;
		
        protected override Consumes<TestMessage> GivenConsumer ()
        {
            sut = new TestConsumer<TestMessage>();
            return sut;
        }
		
        protected override TestMessage When ()
        {
            return new TestMessage();
        }
		
        [Test]
        public void only_message_received_returns_proper_message() {
            Assert.AreEqual(published, sut.OnlyMessageReceived);
        }
		
        [Test]
        public void was_called_is_set_to_true() {
            Assert.IsTrue(sut.WasCalled);
        }
		
        [Test]
        public void times_called_is_one() {
            Assert.AreEqual(1, sut.TimesCalled);
        }
    }
}