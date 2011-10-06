namespace pvc.Core
{
	public interface IQueue<T>
	{
	    string Name { get; }
		bool TryDequeue(out T item);
		void Enqueue(T item);
		void MarkComplete(T item);
        void Requeue(T item);
	}
}