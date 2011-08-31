using System.Threading;
using NUnit.Framework;
using pvc.Adapters.RabbitMQ.Throttling;

namespace pvc.Adapters.RabbitMQ.Tests.Throttling
{
    [TestFixture]
    public class When_using_sequence_throttling
    {
        [Test]
        public void sequence_is_incremented()
        {
            var throttler = new SequenceThrottle(10);
            throttler.RequestResource(1);
            throttler.RequestResource(2);
            
            Assert.AreEqual(2, (int)throttler.CurrentSequence);
        }

        [Test]
        public void sequence_is_decremented()
        {
            var throttler = new SequenceThrottle(10);
            throttler.RequestResource(1);
            throttler.RequestResource(2);

            Assert.AreEqual(2, (int)throttler.CurrentSequence);
            
            throttler.RemoveMany(2);
            
            Assert.AreEqual(0, (int)throttler.CurrentSequence);
        }

        [Test]
        public void releasing_resources_prevents_blocking()
        {
            var throttler = new SequenceThrottle(2);
            throttler.RequestResource(1);
            throttler.RequestResource(2);
            
            throttler.RemoveOne(2); 
            throttler.RequestResource(3); // This would block indefinitely if releasing did not work
        }

        [Test]
        public void exhausting_threshold_causes_blocking()
        {
            var throttler = new SequenceThrottle(2);

            ThreadPool.QueueUserWorkItem(s => { 
                Thread.Sleep(3000);
                throttler.RemoveMany(2);
            });
           
            throttler.RequestResource(1);
            throttler.RequestResource(2);
            throttler.RequestResource(3); // This resource will block until a sequence is released laterally
        }
    }
}
