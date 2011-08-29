using System.Runtime.Serialization.Formatters.Binary;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;
using pvc.Core;

namespace pvc.Adapters.TransactionFile
{
	public class TransactionFileConsumer<T> : Consumes<T> where T : Message
	{
		private readonly TransactionFileWriter<T> _writer;

		public TransactionFileConsumer(string filename)
		{
			_writer = new TransactionFileWriter<T>(filename, true, new BinaryFormatter());
		}

		public void Handle(T message)
		{
			_writer.Enqueue(message);
		}
	}
}
