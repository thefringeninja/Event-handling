using System;
using System.Threading;
using log4net;

namespace pvc.Core
{
	public class QueueReader<T> : Produces<T> where T : Message
	{
        private readonly ILog _logger = LogManager.GetLogger(typeof(QueueReader<T>));

        private readonly IQueue<T> _queue;
		private Consumes<T> _consumer;
        private volatile bool _continue;
		private Thread _thread;

		public delegate void LogError(string format, params object[] args);

		public QueueReader(IQueue<T> queue)
		{
			_queue = queue;
		}

		public QueueReader(IQueue<T> queue, ILog logger)
		{
			_queue = queue;
			_logger = logger;
		}

		private void Run()
		{
			while (_continue)
			{
				try
				{
					T item;
					if (_queue.TryDequeue(out item))
					{
						try
						{
							_consumer.Handle(item);
							_queue.MarkComplete(item);
						}
						catch (Exception exception)
						{
						    var typeName = Equals(item, default(T)) ? "(null)" : item.GetType().Name;
						    _logger.ErrorFormat("Error handling object of type {0}:\n{1}", typeName, exception);
							// requeue?
						}
					}
				}
				catch (Exception exception)
				{
					_logger.ErrorFormat("Error dequeuing object of type {0}:\n{1}", typeof(T).Name, exception);
					//Dead letter
					//Stop?
					//??
				}
			}
		}

		public void Start()
		{
			if (_thread != null)
			{
				throw new InvalidOperationException("Start called while reader already running.");
			}
			_thread = new Thread(Run) { IsBackground = true };
			_thread.Start();
		}

		public void Stop()
		{
			var currentThread = _thread;
			_thread = null;
			if (currentThread == null)
			{
			    return;
			}
			_continue = false;
			currentThread.Join();
		}

		public void AttachConsumer(Consumes<T> consumer)
		{
			_consumer = consumer;
		}
	}
}
