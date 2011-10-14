using System;
using System.Messaging;
using pvc.Core;

namespace pvc.Adapters.MicrosoftMQ
{
    public class MicrosoftQueue<T> : IQueue<T>, IDisposable
    {
        private readonly MessageQueue _receiveQueue;
        private readonly MessageQueue _sendQueue;
        
        private MicrosoftQueue(MessageQueue queue)
        {
            _sendQueue = queue;
        }

        public MicrosoftQueue(string queueName, IMessageFormatter formatter)
        {
            _receiveQueue = _sendQueue = new MessageQueue(queueName) { Formatter = formatter };
        }

        public MicrosoftQueue(string queueName) : this(queueName, new BinaryMessageFormatter())
        {
            
        }

        public MicrosoftQueue(string sendQueueName, string receiveQueueName, IMessageFormatter formatter)
        {
            _sendQueue = new MessageQueue(sendQueueName) { Formatter = formatter };
            _receiveQueue = new MessageQueue(receiveQueueName) { Formatter = formatter };
        }

        public MicrosoftQueue(string sendQueueName, string receiveQueueName)
            : this(sendQueueName, receiveQueueName, new BinaryMessageFormatter())
        {

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

