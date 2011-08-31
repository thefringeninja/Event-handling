using System.Threading;
using NUnit.Framework;
using pvc.Adapters.RabbitMQ.Throttling;

namespace pvc.Adapters.RabbitMQ.Tests.Throttling
{
    [TestFixture]
    public class When_using_queue_throttling
    {
        [Test]
        public void queue_is_enqueued()
        {
            var throttler = new QueueThrottle(10);
            throttler.RequestResource(1);
            throttler.RequestResource(2);
            
            Assert.AreEqual(2, throttler.CurrentSize);
        }

        [Test]
        public void queue_is_dequeued()
        {
            var throttler = new QueueThrottle(10);
            throttler.RequestResource(1);
            throttler.RequestResource(2);

            Assert.AreEqual(2, throttler.CurrentSize);

            throttler.ReleaseResource();
            throttler.ReleaseResource();

            Assert.AreEqual(0, throttler.CurrentSize);
        }

        [Test]
        public void releasing_resources_prevents_blocking()
        {
            var throttler = new QueueThrottle(2);
            throttler.RequestResource(1);
            throttler.RequestResource(2);

            throttler.ReleaseResource();
            throttler.RequestResource(3); // This would block indefinitely if releasing did not work
        }

        [Test]
        public void exhausting_threshold_causes_blocking()
        {
            var throttler = new QueueThrottle(2);

            ThreadPool.QueueUserWorkItem(s =>
            {
                Thread.Sleep(3000);
                throttler.ReleaseResource();
            });

            throttler.RequestResource(1);
            throttler.RequestResource(2);
            throttler.RequestResource(3); // This resource will block until a sequence is released laterally
        }
    }
}