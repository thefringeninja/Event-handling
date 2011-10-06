using System.Collections.Generic;
using System.Threading;

namespace pvc.Core
{
	public class InMemoryQueue<T> : IQueue<T> 
	{
        private readonly Queue<T> _queue; // TODO make this a concurrent queue, need to build on 3.5 for mono for now

        public string Name { get; private set; }

        public InMemoryQueue(string name)
        {
            Name = name;
            _queue = new Queue<T>();
        }

        public InMemoryQueue(string name, int capacity)
        {
            Name = name;
            _queue = new Queue<T>(capacity);
        }
        
	    public bool TryDequeue(out T item)
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    item = _queue.Dequeue();
                    return true;
                }
                Thread.Sleep(1);
                item = default(T);
                return false;
            }
        }

        public void Enqueue(T item)
        {
            lock (_queue)
            {
                _queue.Enqueue(item);
            }
        }

        public void MarkComplete(T item)
        {
            
        }

        public void Requeue(T item)
        {
            Enqueue(item);
        }
	}
}