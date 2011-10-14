using NUnit.Framework;
using pvc.Tests.Messages;

namespace pvc.Tests.TestConsumer
{
    [TestFixture]
	public class when_a_test_consumer_is_constructed {
		
		TestConsumer<TestMessage> sut;
		
		[SetUp]
		public void SetUp() {
			sut = new TestConsumer<TestMessage>();
		}
		
		[Test, ExpectedException(typeof(NoMessagesReceivedException))]
		public void only_message_received_throws_no_messages_received_exception() {
			TestMessage x = sut.OnlyMessageReceived; 
			//TODO switch to assert.throws
		}
		
		[Test]
		public void was_called_is_set_to_false() {
			Assert.IsFalse(sut.WasCalled);
		}
		
		[Test]
		public void times_called_is_zero() {
			Assert.AreEqual(0, sut.TimesCalled);
		}
	}
}

