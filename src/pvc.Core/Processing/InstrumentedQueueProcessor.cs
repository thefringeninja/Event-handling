using System;
using System.Threading;
using log4net;
using pvc.Core.Processing.Instrumentation;

namespace pvc.Core.Processing
{
    /// <summary>
    /// Handles processing of a <see cref="IQueue{T}" />, allowing for instrumentation and diagnostics
    /// </summary>
    /// <typeparam name="T">The type of commands to process</typeparam>
    public class InstrumentedQueueProcessor<T> : IQueueProcessor
    {
        private readonly ICommandProcessorFactory<T> _factory;
        private readonly IQueue<T> _queue;
		private readonly Thread _thread;
        private volatile bool _stop;
        private readonly int _sleep;
        readonly IQueueProcessorInstrumentation _instrumentation;
        private readonly ILog _logger;

        /// <summary>
        /// Run in a background thread to process the queue
        /// </summary>
        public void ProcessQueue()
        {
            while (!_stop)
            {
                try
                {
                    T item;
                    if(_queue.TryDequeue(out item))
                    {
                        if (_instrumentation != null)
                        {
                            _instrumentation.IncrementMessage();
                        }

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
                                    _logger.Error(string.Format("[{0}] Error occurred processing command of type {1} failed in processor {2}", _queue.Name, item.GetType(), processor.GetType()), ex);
                                    Console.WriteLine("[{0}] Error occurred processing command of type {2} failed in processor {3} {1}", _queue.Name, ex, item.GetType(), processor.GetType());
                                }
                            }
                        }
                    }
                    
                    Thread.Sleep(_sleep); // in debug on single proc machine disk based thread starves dispatch thread
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Error occurred in processing of queue {0}", _queue.Name), ex);
					Console.WriteLine("Error occurred in processing of queue {0} {1}", _queue.Name, ex);
                }
            }
        }

        public void Stop()
        {
            _stop = true;
            if (!_thread.Join(10000))
            {
                _thread.Abort();
            }
        }

        public InstrumentedQueueProcessor(IQueue<T> queue, ICommandProcessorFactory<T> factory) : this(queue, factory, null, 0) { }
        
        public InstrumentedQueueProcessor(IQueue<T> queue, ICommandProcessorFactory<T> factory, IQueueProcessorInstrumentation instrumentation, int sleep)
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
            _instrumentation = instrumentation;
            _factory = factory;
            _queue = queue;

			_logger = LogManager.GetLogger(GetType());
			_logger.Info(string.Format("Creating QueueProcessor connecting to queue {0}", _queue.Name));

            var ts = new ThreadStart(ProcessQueue);
            _thread = new Thread(ts) { IsBackground = true };
            _thread.Start();
        }
    }
}
