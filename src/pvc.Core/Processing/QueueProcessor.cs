using System;
using System.Threading;

namespace pvc.Core.Processing
{
    /// <summary>
    /// Handles processing of a <see cref="IQueue{T}" />, providing hooks for derived processors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueProcessor<T> : IQueueProcessor
    {
        private readonly ICommandProcessorFactory<T> _factory;
        private readonly IQueue<T> _queue;
        private readonly Thread _thread;
        private volatile bool _stop;
        private readonly int _sleep;

        public virtual void BeforeProcessing(IQueue<T> queue) { }

        public virtual void AfterProcessing(IQueue<T> queue) { }

        public virtual void OnProcessingException(IQueue<T> queue, T item, ICommandProcessor<T> processor, Exception exception) { }

        public virtual void OnQueueException(IQueue<T> queue, Exception ex) { }

        public void ProcessQueue()
        {
            while (!_stop)
            {
                try
                {
                    T item;
                    if (_queue.TryDequeue(out item))
                    {
                        BeforeProcessing(_queue);

                        var processors = _factory.GetProcessorsForCommand(item);
                        if (processors != null)
                        {
                            foreach (var processor in processors)
                            {
                                try
                                {
                                    processor.ProcessCommand(item);
                                }
                                catch (Exception ex)
                                {
                                    OnProcessingException(_queue, item, processor, ex);
                                }
                            }
                        }

                        AfterProcessing(_queue);
                    }

                    Thread.Sleep(_sleep); // in debug on single proc machine disk based thread starves dispatch thread
                }
                catch (Exception ex)
                {
                    OnQueueException(_queue, ex);
                }
            }
        }

        public QueueProcessor(IQueue<T> queue, ICommandProcessorFactory<T> factory) : this(queue, factory, 0) { }
        
        public QueueProcessor(IQueue<T> queue, ICommandProcessorFactory<T> factory, int sleep)
        {
            if (queue == null)
            {
                throw new ArgumentNullException("queue");
            }

            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            _sleep = sleep;
            _factory = factory;
            _queue = queue;

            var ts = new ThreadStart(ProcessQueue);
            _thread = new Thread(ts) { IsBackground = true };
            _thread.Start();
        }

        public void Stop()
        {
            _stop = true;
            if (!_thread.Join(10000))
            {
                _thread.Abort();
            }
        }
    }
}