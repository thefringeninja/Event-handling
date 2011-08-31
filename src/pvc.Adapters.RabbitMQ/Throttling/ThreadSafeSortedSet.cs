using System;
using System.Collections;
using System.Collections.Generic;

namespace pvc.Adapters.RabbitMQ.Throttling
{
    /// <summary>
    /// A <see cref="ThreadSafeSortedSet{T}" /> decorator that monitors changes when consumed from multiple threads
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThreadSafeSortedSet<T> : ISet<T>
    {
        private readonly SortedSet<T> _inner;

        private readonly object _sync = new object();

        public ThreadSafeSortedSet()
        {
            _inner = new SortedSet<T>();
        }

        public ThreadSafeSortedSet(SortedSet<T> inner)
        {
            _inner = inner;
        }

        public int RemoveWhere(Predicate<T> predicate)
        {
            return _inner.RemoveWhere(predicate);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            lock (_sync)
            {
                _inner.Add(item);
            }
        }

        bool ISet<T>.Add(T item)
        {
            lock (_sync)
            {
                _inner.Add(item);
            }
            return true;
        }

        public void UnionWith(IEnumerable<T> other)
        {
            lock (_sync)
            {
                _inner.UnionWith(other);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            lock (_sync)
            {
                _inner.IntersectWith(other);
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            lock (_sync)
            {
                _inner.ExceptWith(other);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            lock (_sync)
            {
                _inner.SymmetricExceptWith(other);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _inner.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _inner.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _inner.IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _inner.IsProperSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _inner.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _inner.SetEquals(other);
        }

        public void Clear()
        {
            lock (_sync)
            {
                _inner.Clear();
            }
        }

        public bool Contains(T item)
        {
            return _inner.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_sync)
            {
                _inner.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (_sync)
            {
                return _inner.Remove(item);
            }
        }

        public int Count
        {
            get { return _inner.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ISet<T>)_inner).IsReadOnly; }
        }
    }
}