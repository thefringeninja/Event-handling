namespace pvc.Core
{
    /// <summary>
    /// Interface to represent a blocking queue. The Dequeue method will block until an item is available
    /// </summary>
    /// <typeparam name="T">The type of objects to be placed in the queue</typeparam>
    public interface IBlockingQueue<T> : IQueue<T>
    {
        bool RecordAvailable
        {
            get;
        }

        int Count
        {
            get;
        }

        T Dequeue();
    }

}
