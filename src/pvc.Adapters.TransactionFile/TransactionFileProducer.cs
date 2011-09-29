using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using pvc.Adapters.TransactionFile.Processing;
using pvc.Adapters.TransactionFile.Processing.Instrumentation;
using pvc.Adapters.TransactionFile.Queues;
using pvc.Core;

namespace pvc.Adapters.TransactionFile
{
	public class TransactionFileProducer<T> : Produces<T> where T : Message
	{
		private readonly TransactionFileBlockingQueue<T> _reader;
		private QueueProcessor<T> _processor;
		private Consumes<T> _consumer;

		public TransactionFileProducer(string filename, string checksumFilename)
		{
			_reader = new TransactionFileBlockingQueue<T>(filename, checksumFilename, new BinaryFormatter());
			_processor = new QueueProcessor<T>(_reader, new AlwaysSendToMeFactory(new SendToMeCommandProcessor(this)), new WMIQueueProcessorInstrumentation(checksumFilename), 0); 
		}

		internal class AlwaysSendToMeFactory : ICommandProcessorFactory<T>
		{
			private readonly List<ICommandProcessor<T>> _toSend = new List<ICommandProcessor<T>>();

			public AlwaysSendToMeFactory(ICommandProcessor<T> toSend)
			{
				_toSend.Add(toSend);
			}

			public IList<ICommandProcessor<T>> GetProcessorsForCommand(T command)
			{
				return _toSend;
			}
		}

		internal class SendToMeCommandProcessor : ICommandProcessor<T>
		{
			private readonly TransactionFileProducer<T> _parent;

			public SendToMeCommandProcessor(TransactionFileProducer<T> parent)
			{
				_parent = parent;
			}

			public void ProcessCommand(T command)
			{
				_parent.Send(command);
			}
		}

		private void Send(T message)
		{
			_consumer.TryConsume(message);
		}

		public void AttachConsumer(Consumes<T> consumer)
		{
			_consumer = consumer;
		}
	}
}
