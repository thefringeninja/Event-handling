using System.Collections.Generic;
using System.Threading;

namespace pvc.Core
{
    public class InMemoryBlockingQueue<T> : IBlockingQueue<T>
    {
        private readonly Queue<T> _queue; // TODO make this a concurrent queue, need to build on 3.5 for mono now
        private readonly AutoResetEvent _dataAddedEvent;

        public InMemoryBlockingQueue(string name)
        {
            Name = name;
        }

        public InMemoryBlockingQueue(string name, int capacity)
        {
            Name = name;
            _queue = new Queue<T>(capacity);
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

        public T Dequeue()
        {
            T item;
            TryDequeue(out item);
            return item;
        }

        #region IBlockingQueue<T> Members

        public string Name { get; private set; }

        public bool TryDequeue(out T item)
        {
            while (true)
            {
                if (_queue.Count == 0)
                {
                    _dataAddedEvent.WaitOne(100000, true);
                }
                lock (_queue)
                {
                    if (_queue.Count <= 0)
                    {
                        continue;
                    }
                    item = _queue.Dequeue();
                    return true;
                }
            }
        }

        public void Enqueue(T data)
        {
            lock (_queue)
            {
                _queue.Enqueue(data);
                _dataAddedEvent.Set();
            }
        }

        public void MarkComplete(T item)
        {
            
        }

        public void Requeue(T item)
        {
           Enqueue(item);
        }

        public bool RecordAvailable
        {
            get { return _queue.Count > 0; }
        }
        
        #endregion
    }
}
