using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;

namespace pvc.Adapters.TransactionFile.Queues
{
    /// <summary>
    /// An implementation of <see cref="IBlockingQueue{T}" /> that uses a transaction
    /// file for persistence and allows inter-process communication (IPC)
    /// </summary>
    /// <remarks>
    ///     - Many processes can read and write to this blocking queue. All processes will see all writes.
    /// </remarks>
    public class TransactionFileBlockingQueue<T> : IBlockingQueue<T>
    {
        private readonly TransactionFileWriter<T> _writer;
        private readonly TransactionFileReader<T> _reader;
        private readonly string _name;

        public bool RecordAvailable
        {
            get { return true; } // TODO: make look at checksums
        }

        public TransactionFileBlockingQueue(string transactionFilename, string checkSumName, IFormatter formatter)
        {
            if(transactionFilename == null)
            {
                throw new ArgumentNullException("TransactionFileName");
            }

            if(formatter == null)
            {
                formatter = new BinaryFormatter();
            }

            _name = transactionFilename;
            _writer = new TransactionFileWriter<T>(transactionFilename, formatter);

            _reader = checkSumName != null
                          ? new CheckSummedTransactionFileReader<T>(transactionFilename, checkSumName, formatter)
                          : new TransactionFileReader<T>(transactionFilename, formatter);
        }

        public TransactionFileBlockingQueue(string transactionFilename) : this(transactionFilename, null, null) { }

        #region IBlockingQueue<T> Members

        public string Name
        {
            get { return _name; }
        }

        public void Enqueue(T data)
        {
            _writer.Enqueue(data);
        }

        public T Dequeue()
        {
            return _reader.Dequeue();
        }

        public int Count { get; private set; }

        #endregion
    }
}
