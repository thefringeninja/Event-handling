namespace pvc.Adapters.TransactionFile
{
    /// <summary>
    /// Interface to represent a blocking queue. The Dequeue method will block until an item is available
    /// </summary>
    /// <typeparam name="T">The type of objects to be placed in the queue</typeparam>
    public interface IBlockingQueue<T>
    {
        string Name { get; }

        bool RecordAvailable
        {
            get;
        }

        void Enqueue(T data);

        T Dequeue();

        int Count
        {
            get;
        }
    }

}
