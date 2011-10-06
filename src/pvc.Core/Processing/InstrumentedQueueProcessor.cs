using System;
using log4net;
using pvc.Core.Processing.Instrumentation;

namespace pvc.Core.Processing
{
    /// <summary>
    /// Handles processing of a <see cref="IQueue{T}" />, allowing for instrumentation and diagnostics
    /// </summary>
    /// <typeparam name="T">The type of commands to process</typeparam>
    public class InstrumentedQueueProcessor<T> : QueueProcessor<T>
    {
        readonly IQueueProcessorInstrumentation _instrumentation;
        private readonly ILog _logger;

        public override void BeforeProcessing(IQueue<T> queue)
        {
            if (_instrumentation != null)
            {
                _instrumentation.IncrementMessage();
            }
        }

        public override void OnProcessingException(IQueue<T> queue, T item, ICommandProcessor<T> processor, Exception ex)
        {
            _logger.Error(string.Format("[{0}] Error occurred processing command of type {1}, failed in processor {2}", queue.Name, item.GetType(), processor.GetType()), ex);
            Console.WriteLine("[{0}] Error occurred processing command of type {2}, failed in processor {3} {1}", queue.Name, ex, item.GetType(), processor.GetType());
        }

        public override void OnQueueException(IQueue<T> queue, Exception ex)
        {
            _logger.Error(string.Format("Error occurred in processing of queue {0}", queue.Name), ex);
            Console.WriteLine("Error occurred in processing of queue {0} {1}", queue.Name, ex);
        }

        public InstrumentedQueueProcessor(IQueue<T> queue, ICommandProcessorFactory<T> factory, IQueueProcessorInstrumentation instrumentation)
            : base(queue, factory, 0)
        {
            
        }

        public InstrumentedQueueProcessor(IQueue<T> queue, ICommandProcessorFactory<T> factory, IQueueProcessorInstrumentation instrumentation, int sleep)
            : base(queue, factory, sleep)
        {
            _instrumentation = instrumentation;
            _logger = LogManager.GetLogger(GetType());
            _logger.Info(string.Format("Creating QueueProcessor connecting to queue {0}", queue.Name));
        }
    }
}
