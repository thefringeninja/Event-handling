using System;
using System.Threading;
using log4net;
using pvc.Adapters.TransactionFile.Processing.Instrumentation;

namespace pvc.Adapters.TransactionFile.Processing
{
    /// <summary>
    /// Handles processing of a <see cref="IBlockingQueue{T}" />
    /// </summary>
    /// <typeparam name="T">The type of commands to process</typeparam>
    public class QueueProcessor<T>
    {
        private readonly ICommandProcessorFactory<T> _factory;
        private readonly IBlockingQueue<T> _queue;
		private readonly Thread _thread;
        private volatile bool _stop;
        private readonly int _sleep;
        readonly IQueueProcessorInstrumentation _instrumentation;
        private readonly ILog _logger;

        /// <summary>
        /// Run in another thread to process the queue.
        /// </summary>
        private void ProcessQueue()
        {
            while (!_stop)
            {
                try
                {
                    var obj = _queue.Dequeue();
                    if (_instrumentation != null)
                    {
                        _instrumentation.IncrementMessage();
                    }

                    var processors = _factory.GetProcessorsForCommand(obj);
                    if (processors != null)
                    {
                        foreach (var processor in processors)
                        {
                            try
                            {
                                processor.ProcessCommand(obj);
                            }
                            catch (Exception ex)
                            {
								_logger.Error(string.Format("[{0}] Error occurred processing command of type {1} failed in processor {2}", _queue.Name, obj.GetType(), processor.GetType()), ex);
								Console.WriteLine("[{0}] Error occurred processing command of type {2} failed in processor {3} {1}", _queue.Name, ex, obj.GetType(), processor.GetType());
                            }
                        }
                    }
                    Thread.Sleep(_sleep); //in debug on single proc machine disk based thread starves dispatch thread
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

        public QueueProcessor(IBlockingQueue<T> queue, ICommandProcessorFactory<T> factory) : this(queue, factory, null, 0) { }
        
        public QueueProcessor(IBlockingQueue<T> queue, ICommandProcessorFactory<T> factory, IQueueProcessorInstrumentation instrumentation, int sleep)
        {
            if (queue == null)
            {
                throw new ArgumentNullException("queue");
            }

            if (factory == null)
            {
                throw new ArgumentNullException("");
            }

            _sleep = sleep;
            _instrumentation = instrumentation;
            _factory = factory;
            _queue = queue;

			_logger = LogManager.GetLogger(GetType());
			_logger.Info(string.Format("Creating QueueProcessor connecting to queue {0}", _queue.Name));

            var ts = new ThreadStart(ProcessQueue);
            _thread = new Thread(ts);
            _thread.IsBackground = true;
            _thread.Start();
        }
    }
}
