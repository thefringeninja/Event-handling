using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using pvc.Adapters.TransactionFile.Checksums;
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
        private readonly IChecksum _recordChecksum;
        private readonly TransactionFileWriter<T> _writer;
        private readonly TransactionFileReader<T> _reader;
        private readonly string _name;
        
        public bool RecordAvailable
        {
            get
            {
                var value = _recordChecksum.GetValue();
                return value > 0;
            }
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

            _recordChecksum = new FileChecksum(_writer.ChecksumName);
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

        public bool IsBlocking
        {
            get { return _reader.IsBlocking; }
        }

        private long _count;

        /// <summary>
        /// If this queue instance was responsible for all <see cref="Enqueue" /> or <see cref="Requeue"/> actions, this count
        /// will reflect the current number of items in the queue; if it was constructed from an existing <see cref="IChecksum"/>,
        /// then this number will not reflect the actual count.
        /// </summary>
        public int Count { get { return (int)Interlocked.Read(ref _count); } }
        
        #endregion
    }
}
