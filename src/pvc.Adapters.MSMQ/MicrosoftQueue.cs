using System;
using System.Messaging;
using pvc.Core;

namespace pvc.Adapters.MicrosoftMQ
{
    public class MicrosoftQueue<T> : IQueue<T>, IDisposable
    {
// ReSharper disable StaticFieldInGenericType
        private static readonly IMessageFormatter _formatter;
// ReSharper restore StaticFieldInGenericType

        private readonly MessageQueue _receiveQueue;
        private readonly MessageQueue _sendQueue;

        static MicrosoftQueue()
        {
            _formatter = new BinaryMessageFormatter();
        }

        private MicrosoftQueue(MessageQueue queue)
        {
            _sendQueue = queue;
        }

        public MicrosoftQueue(string queueName)
        {
            _receiveQueue = _sendQueue = new MessageQueue(queueName) { Formatter = _formatter };
        }

        public MicrosoftQueue(string sendQueueName, string receiveQueueName)
        {
            _sendQueue = new MessageQueue(sendQueueName) { Formatter = _formatter };
            _receiveQueue = new MessageQueue(receiveQueueName) { Formatter = _formatter };
        }

        public static MicrosoftQueue<T> Create(string queueName)
        {
            return new MicrosoftQueue<T>(new MessageQueue(queueName) { Formatter = new BinaryMessageFormatter() });
        }

        public void Dispose()
        {
            if (ReferenceEquals(_sendQueue, _receiveQueue) && (_receiveQueue != null))
            {
                _receiveQueue.Dispose();
            }
            if (_sendQueue != null)
            {
                _sendQueue.Dispose();
            }
        }

        public void Enqueue(T item)
        {
            _sendQueue.Send(new System.Messaging.Message(item, _sendQueue.Formatter));
        }

        public virtual void MarkComplete(T item)
        {

        }

        public void Requeue(T item)
        {
            Enqueue(item);
        }

        public string Name
        {
            get { return _sendQueue.QueueName; }
        }

        public virtual bool TryDequeue(out T item)
        {
            using (var message = _receiveQueue.Receive())
            {
                if (message == null)
                {
                    item = default(T);
                    return false;
                }
                message.Formatter = _receiveQueue.Formatter;
                item = (T)message.Body;
            }
            return true;
        }
    }
}

