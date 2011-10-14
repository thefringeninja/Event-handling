using System;
using NUnit.Framework;
using pvc.Core;
using pvc.Tests.Messages;

namespace pvc.Tests.ActionWrappingConsumer
{
    [TestFixture]
    class when_build_a_wrapped_action
    {
        [Test]
        public void a_null_action_throws_argumentnullexception() {
            Assert.Throws<ArgumentNullException>(() => new ActionWrappingConsumer<TestMessage>(null)); 
        }
    }
}