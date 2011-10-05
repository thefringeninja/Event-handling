using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;
using pvc.Core;

namespace pvc.Adapters.TransactionFile
{
    /// <summary>
    /// A consumer for a given message type that handles the message by serializing it to a transaction file
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public class TransactionFileConsumer<T> : Consumes<T> where T : Message
	{
		private readonly TransactionFileWriter<T> _writer;

		public TransactionFileConsumer(string filename)
		{
			_writer = new TransactionFileWriter<T>(filename, (IFormatter) new BinaryFormatter());
		}

		public void Handle(T message)
		{
			_writer.Enqueue(message);
		}
	}
}
