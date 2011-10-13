using System;
using System.Runtime.Serialization.Formatters.Binary;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;
using pvc.Core;

namespace pvc.Adapters.TransactionFile
{
    /// <summary>
    /// A consumer for a given message type that handles the message by serializing it to a transaction file
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public class TransactionFileConsumer<T> : IDisposable, Consumes<T> where T : Message
    {
        internal TransactionFileWriter<T> Writer { get; private set; }

        public TransactionFileConsumer(string filename)
		{
			Writer = new TransactionFileWriter<T>(filename, new BinaryFormatter());
		}

		public void Handle(T message)
		{
			Writer.Enqueue(message);
		}

        public void Dispose()
        {
            if(Writer != null)
            {
                Writer.Dispose();
                Writer = null;
            }
        }
	}
}
