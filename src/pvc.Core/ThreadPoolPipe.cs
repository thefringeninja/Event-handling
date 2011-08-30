using System.Threading;

namespace pvc.Core
{
    public class ThreadPoolPipe<T> : Pipe<T, T> where T : Message
    {
        private Consumes<T> _consumer;

        public void Handle(T message)
        {
            ThreadPool.QueueUserWorkItem(x => _consumer.Handle(message));
        }

        public void AttachConsumer(Consumes<T> consumer)
        {
            _consumer = consumer;
        }
    }
}