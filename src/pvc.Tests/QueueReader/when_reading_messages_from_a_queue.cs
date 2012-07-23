using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using pvc.Core;
using pvc.Tests.Fixtures;
using pvc.Tests.Messages;

namespace pvc.Tests.QueueReader
{
    [TestFixture]
    public class when_reading_messages_from_a_queue : ConsumerFixture<TestMessage>
    {
        private IQueue<TestMessage> queue;
        private QueueReader<TestMessage> reader;
        private TestMessage message;
        private readonly ManualResetEvent wait = new ManualResetEvent(false);
        public override void SetUp()
        {
            queue = new InMemoryQueue<TestMessage>("TestMessage");
            reader = new QueueReader<TestMessage>(queue);
            reader.AttachConsumer(
                new ActionWrappingConsumer<TestMessage>(
                    m =>
                        {
                            message = m;
                            wait.Set();
                        }));
            reader.Start();

            base.SetUp();
        }

        protected override Consumes<TestMessage> GivenConsumer()
        {
            return new QueueWriter<TestMessage>(queue);
        }

        protected override TestMessage When()
        {
            return new TestMessage();
        }

        [Test]
        public void the_message_is_read()
        {
            wait.WaitOne(TimeSpan.FromSeconds(5));
            Assert.IsNotNull(message);
        }

        [Test]
        public void no_exception_is_thrown()
        {
            Assert.IsNull(caught);
        }

        [TearDown]
        public void TearDown()
        {
            reader.Stop();
        }
    }
}
