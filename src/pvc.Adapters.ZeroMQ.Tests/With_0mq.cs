using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ZMQ;
using pvc.Adapters.ZeroMQ.Tests.Fixtures;

namespace pvc.Adapters.ZeroMQ.Tests
{
    [TestFixture]
    public class With_0mq
    {
        [Test]
        public void point_to_point_pattern_is_functional()
        {
            var block = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem(
                s =>
                {
                    using (var context = new Context())
                    {
                        using (var socket = context.Socket(SocketType.REP))
                        {
                            socket.Bind("tcp://*:5562");
                            var buffer = socket.Recv();
                            socket.Send("", Encoding.Unicode); // ACK


                            using (var ms = new MemoryStream(buffer))
                            {
                                var message = new BinaryFormatter().Deserialize(ms) as TestMessage;
                                if(message != null)
                                {
                                    block.Set();
                                }
                            }
                        }
                    }
                });
            
            ThreadPool.QueueUserWorkItem(
                s =>
                    {
                        using (var context = new Context())
                        {
                            using (var socket = context.Socket(SocketType.REQ))
                            {
                                socket.Connect("tcp://localhost:5562");
                                using (var ms = new MemoryStream())
                                {
                                    new BinaryFormatter().Serialize(ms, new TestMessage());
                                    socket.Send(ms.ToArray());
                                    socket.Recv(); // ACK
                                }
                            }
                        }
                    }
                );

            block.WaitOne();
        }

        [Test]
        public void timeouts_are_handled_gracefully()
        {
            using (var context = new Context())
            {
                using (var socket = context.Socket(SocketType.REP))
                {
                    socket.Bind("tcp://*:5562");
                    var received = socket.Recv(100);
                    Assert.IsNull(received);
                }
            }
        }
    }
}
