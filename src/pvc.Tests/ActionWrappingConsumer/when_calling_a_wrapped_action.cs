using NUnit.Framework;
using pvc.Core;
using pvc.Tests.Fixtures;
using pvc.Tests.Messages;

namespace pvc.Tests.ActionWrappingConsumer
{
    [TestFixture]
	class when_calling_a_wrapped_action : ConsumerFixture<TestMessage>
	{
		int _called;
		private TestMessage _message;
		protected override Consumes<TestMessage> GivenConsumer()
		{
			return new ActionWrappingConsumer<TestMessage>(x =>
			                                               	{
			                                               		_called++;
			                                               		_message = x;
			                                               	});
		}

		protected override TestMessage When()
		{
			return new TestMessage();
		}

		public override void SetUp()
		{
			_message = null;
			_called = 0;
			base.SetUp();
		}

		[Test]
		public void the_action_is_called()
		{
		     Assert.AreEqual(published, _message);
		}

		[Test]
		public void the_action_is_called_once()
		{
			Assert.AreEqual(1, _called);
		}

	}
}
