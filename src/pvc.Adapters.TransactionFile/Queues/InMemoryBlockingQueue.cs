using System;
using System.Collections.Generic;
using System.Threading;

namespace pvc.Adapters.TransactionFile.Queues
{
    public class InMemoryBlockingQueue<T> : IBlockingQueue<T>
    {
        private readonly Queue<T> _queue;
        private readonly AutoResetEvent _dataAddedEvent;

        public InMemoryBlockingQueue(int initialSize, string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            Name = name;
            _queue = new Queue<T>(initialSize);
            _dataAddedEvent = new AutoResetEvent(false);
        }

        public int Count
        {
            get
            {
                lock (_queue)
                {
                    return _queue.Count;
                }
            }
        }

        public InMemoryBlockingQueue(string name) : this(8, name) { }

        #region IBlockingQueue<T> Members

        public string Name { get; private set; }

        public void Enqueue(T data)
        {
            lock (_queue)
            {
                _queue.Enqueue(data);
                _dataAddedEvent.Set();
            }
        }

        public bool RecordAvailable
        {
            get { return _queue.Count > 0; }
        }

        public T Dequeue()
        {
            while (true)
            {
                if (_queue.Count == 0)
                {
                    _dataAddedEvent.WaitOne(100000, true);
                }
                lock (_queue)
                {
                    if (_queue.Count > 0)
                    {
                        return _queue.Dequeue();
                    }
                }
            }
        }

        #endregion
    }
}
