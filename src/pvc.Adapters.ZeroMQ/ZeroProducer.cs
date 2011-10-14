using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using ZMQ;
using pvc.Core;

namespace pvc.Adapters.ZeroMQ
{
    /// <summary>
    /// A point-to-point producer that publishes messages received from a 0MQ subscriber socket
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ZeroProducer<T> : Produces<T>, IDisposable where T : Message
    {
        private Consumes<T> _consumer;
        private Thread _thread;
        private bool _stop;
        private Context _context;
        private Socket _socket;
        private readonly string _endpoint;
        private readonly IFormatter _formatter;

        public ZeroProducer(string endpoint, IFormatter formatter = null)
        {
            _endpoint = endpoint;
            _context = new Context(1);
            _formatter = formatter ?? new BinaryFormatter();

            _socket = _context.Socket(SocketType.REP);
            _socket.Connect(_endpoint);
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
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }

        public override string ToString()
        {
            return _endpoint;
        }
        
        private void ProcessQueue()
        {
            while (!_stop)
            {
                T item;
                if (TryDequeue(out item))
                {
                    _consumer.Handle(item);
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

        private bool TryDequeue(out T item)
        {
            try
            {
                var buffer = _socket.Recv(); // blocking
                var message = new MemoryStream(buffer);
                var r = _formatter.Deserialize(message);
                item = (T) r;
                return true;
            }
            catch
            {
                item = default(T);
                return false;
            }
        }
    }
}