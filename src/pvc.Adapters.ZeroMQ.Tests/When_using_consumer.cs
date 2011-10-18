using System.Threading;
using NUnit.Framework;
using ZMQ;
using pvc.Adapters.ZeroMQ.Tests.Fixtures;
using pvc.Core;

namespace pvc.Adapters.ZeroMQ.Tests
{
    [TestFixture]
    public class When_using_consumer
    {
        [Test]
        public void message_is_consumed()
        {
            var block = new ManualResetEvent(false);

            var consumed = false;

            var context = new Context(1);

            ThreadPool.QueueUserWorkItem(
                s =>
                    {
                        using (var socket = context.Socket(SocketType.REP))
                        {
                            socket.Bind("tcp://*:5562");
                            socket.Recv(); // blocking
                        }
                        consumed = true;
                        block.Set();
                    }
                );

            using (var consumer = new ZeroConsumer<Message>("tcp://localhost:5562"))
            {
                consumer.Handle(new TestMessage());
            }
            block.WaitOne();
            Assert.IsTrue(consumed);
        }
    }
}
