using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using ZMQ;
using pvc.Core;

namespace pvc.Adapters.ZeroMQ
{
    /// <summary>
    /// A point-to-point producer that publishes messages received from a 0MQ REP socket
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ZeroProducer<T> : Produces<T>, IDisposable where T : Message
    {
        private Consumes<T> _consumer;
        private Thread _thread;
        private bool _stop;
        private Context _context;
        private readonly string _endpoint;
        private readonly IFormatter _formatter;

        public ZeroProducer(string endpoint, IFormatter formatter = null) : this (new Context(1), endpoint, formatter)
        {
            
        }

        public ZeroProducer(Context context, string endpoint, IFormatter formatter = null)
        {
            _endpoint = endpoint;
            _context = context;
            _formatter = formatter ?? new BinaryFormatter();
        }

        public void AttachConsumer(Consumes<T> consumer)
        {
            _consumer = consumer;
            if (_thread == null)
            {
                Start();
            }
        }

        public void Dispose()
        {
            Stop();
            if (_context == null)
            {
                return;
            }
            _context.Dispose();
            _context = null;
        }

        public override string ToString()
        {
            return _endpoint;
        }
        
        private void ProcessQueue()
        {
            using (var socket = _context.Socket(SocketType.REP))
            {
                socket.Bind(_endpoint);

                while (!_stop)
                {
                    T item;
                    if (TryDequeue(socket, out item))
                    {
                        _consumer.Handle(item);
                    }
                }
            }
        }

        private void Start()
        {
            _thread = new Thread(ProcessQueue) { IsBackground = true };
            _thread.Start();
        }

        public void Stop()
        {
            _stop = true;
            if (!_thread.Join(1000))
            {
                _thread.Abort();
            }
        }
        
        private bool TryDequeue(Socket socket, out T item)
        {
            try
            {
                var buffer = socket.Recv();
                var message = new MemoryStream(buffer);
                var r = _formatter.Deserialize(message);
                item = (T)r;

                socket.Send("ACK", Encoding.Unicode);
                return true;
            }
            catch (ThreadAbortException)
            {
                // We are often blocking on receiving when the producer thread is shut down
                item = default(T);
                return false;
            }
        }
    }
}