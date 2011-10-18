using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ZMQ;
using pvc.Core;

namespace pvc.Adapters.ZeroMQ
{
    // http://nichol.as/zeromq-an-introduction
    
    /// <summary>
    /// A point-to-point consumer that handles messages by sending them through a 0MQ REQ socket
    /// </summary>
    public class ZeroConsumer<T> : Consumes<T>, IDisposable where T : Message
    {
        private Context _context;
        private readonly IFormatter _formatter;
        private readonly string _endpoint;
        private Socket _socket;

        public ZeroConsumer(string endpoint, IFormatter formatter = null) : this(new Context(), endpoint, formatter)
        {

        }
        
        public ZeroConsumer(Context context, string endpoint, IFormatter formatter = null)
        {
            _endpoint = endpoint;
            _context = context;
            _socket = _context.Socket(SocketType.REQ);
            _socket.Connect(_endpoint);
            _formatter = formatter ?? new BinaryFormatter();
        }

        public void Handle(T message)
        {
            using (var ms = new MemoryStream())
            {
                _formatter.Serialize(ms, message);
                _socket.Send(ms.ToArray());
            }
        }

        public override string ToString()
        {
            return _endpoint;
        }
        
        public void Dispose()
        {
            if(_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            if(_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }
    }
}
