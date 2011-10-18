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
        private volatile bool _stop;
        private readonly Context _context;
        private readonly string _endpoint;
        private readonly IFormatter _formatter;
        private Socket _socket;
        private const int Timeout = 1000;

        public event EventHandler<EventArgs> TimedOut;
        public virtual void OnTimedOut(EventArgs e)
        {
            var handler = TimedOut;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public ZeroProducer(string endpoint, IFormatter formatter = null) : this (new Context(1), endpoint, formatter)
        {
            
        }

        public ZeroProducer(Context context, string endpoint, IFormatter formatter = null)
        {
            _endpoint = endpoint;
            _context = context;
            _formatter = formatter ?? new BinaryFormatter();
            RebuildSocket();
        }

        public void RebuildSocket()
        {
            if (_socket != null)
            {
                return;
            }

            _socket = _context.Socket(SocketType.REP);
            _socket.Bind(_endpoint);
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
            _thread.Join();
        }

        private bool TryDequeue(out T item)
        {
            if (_stop)
            {
                _socket.Dispose();
                item = default(T);
                return false;
            }

            var buffer = _socket.Recv(Timeout);
            if (buffer == null)
            {
                OnTimedOut(EventArgs.Empty);
                _socket.Dispose();
                RebuildSocket();
                return TryDequeue(out item);
            }

            var message = new MemoryStream(buffer);
            var r = _formatter.Deserialize(message);
            item = (T) r;

            _socket.Send("", Encoding.Unicode); // ACK
            return true;
        }
    }
}