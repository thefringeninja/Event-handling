using NUnit.Framework;
using pvc.Tests.Messages;

namespace pvc.Tests.TestConsumer
{
    [TestFixture]
    public class when_test_consumer_receives_two_messages {
        TestConsumer<TestMessage> sut;
		
        [SetUp]
        public void SetUp() {
            sut = new TestConsumer<TestMessage>();
            sut.Handle(new TestMessage());
            sut.Handle(new TestMessage());
        }
		
        [Test, ExpectedException(typeof(TooManyMessagesReceivedException))]
        public void only_message_received_throws_too_many_messages_received_exception() {
            TestMessage x = sut.OnlyMessageReceived; 
            //TODO switch to assert.throws
        }
		
        [Test]
        public void was_called_is_set_to_true() {
            Assert.IsTrue(sut.WasCalled);
        }
		
        [Test]
        public void times_called_is_two() {
            Assert.AreEqual(2, sut.TimesCalled);
        }
    }
}