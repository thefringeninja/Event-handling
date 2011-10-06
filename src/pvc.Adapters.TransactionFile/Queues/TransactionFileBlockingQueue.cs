using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;
using pvc.Core;

namespace pvc.Adapters.TransactionFile.Queues
{
    /// <summary>
    /// An implementation of <see cref="IBlockingQueue{T}" /> that uses a transaction
    /// file for persistence and allows inter-process communication (IPC)
    /// </summary>
    /// <remarks>
    ///     - Many processes can read and write to this blocking queue; all processes will see all writes.     
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
            if (transactionFilename == null)
            {
                throw new ArgumentNullException("transactionFileName");
            }

            if (formatter == null)
            {
                formatter = new BinaryFormatter();
            }

            _name = transactionFilename;
            _writer = new TransactionFileWriter<T>(transactionFilename, formatter);

            _reader = checkSumName != null
                          ? new CheckSummedTransactionFileReader<T>(transactionFilename, checkSumName, formatter)
                          : new TransactionFileReader<T>(transactionFilename, formatter);
        }

        public TransactionFileBlockingQueue(string transactionFilename, string checkSumName) : this(transactionFilename, checkSumName, null) { }

        public TransactionFileBlockingQueue(string transactionFilename) : this(transactionFilename, null, null) { }

        #region IBlockingQueue<T> Members

        public string Name
        {
            get { return _name; }
        }

        public bool TryDequeue(out T item)
        {
            item = Dequeue();
            return true;
        }

        public void Enqueue(T data)
        {
            _writer.Enqueue(data);
            Interlocked.Increment(ref _count);
        }

        public void MarkComplete(T item)
        {
            
        }

        public void Requeue(T item)
        {
            Enqueue(item);
        }

        public T Dequeue()
        {
            var item = _reader.Dequeue();
            Interlocked.Decrement(ref _count);
            return item;
        }

        private long _count;

        public int Count { get { return (int)Interlocked.Read(ref _count); } }
        
        #endregion
    }
}
