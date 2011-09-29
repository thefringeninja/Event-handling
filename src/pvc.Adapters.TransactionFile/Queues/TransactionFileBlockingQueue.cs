using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;

#if NET20
#define UseOlderReaderWriter
#endif

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
#if UseOlderReaderWriter
        private readonly ReaderWriterLock _lock = new ReaderWriterLock();
#else
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
#endif

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
                throw new ArgumentNullException("TransactionFileName");
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

        public void Enqueue(T data)
        {
            try
            {
#if UseOlderReaderWriter
                _lock.AcquireWriterLock(5000);
#else
                _lock.EnterWriteLock();
#endif
                _writer.Enqueue(data);
                Interlocked.Increment(ref _count);
            }
            finally
            {
#if UseOlderReaderWriter
                _lock.ReleaseWriterLock();
#else
                _lock.ExitWriteLock();
#endif
            }
        }

        public T Dequeue()
        {
            try
            {
#if UseOlderReaderWriter
                _lock.AcquireReaderLock();
#else
                _lock.EnterReadLock();
#endif
                var item = _reader.Dequeue();
                Interlocked.Decrement(ref _count);
                return item;
            }
            finally
            {
#if UseOlderReaderWriter
                _lock.ReleaseReaderLock();
#else
                _lock.ExitReadLock();
#endif
            }
        }

        private int _count;

        public int Count { get { return _count; } }
        
        #endregion
    }
}
