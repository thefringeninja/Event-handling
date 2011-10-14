using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;
using ZMQ;
using pvc.Adapters.ZeroMQ.Tests.Fixtures;
using pvc.Core;

namespace pvc.Adapters.ZeroMQ.Tests
{
    [TestFixture]
    public class When_using_producer
    {
        [Test]
        public void message_is_produced()
        {
            var block = new ManualResetEvent(false);

            var received = false;

            var context = new Context(1);
            var consumer = new TestConsumer();
            
            var producer = new ZeroProducer<Message>("tcp://localhost:5562");
            producer.AttachConsumer(consumer);
            
            ThreadPool.QueueUserWorkItem(
                s =>
                    {
                        using (var socket = context.Socket(SocketType.REQ))
                        {
                            socket.Bind("tcp://*:5562");
                            using (var ms = new MemoryStream())
                            {
                                new BinaryFormatter().Serialize(ms, new TestMessage());
                                socket.Send(ms.ToArray());
                            }
                        }
                        received = true;
                        block.Set();
                    }
                );

            
            block.WaitOne();
            Assert.IsTrue(received);

            producer.Dispose();
        }
    }
}