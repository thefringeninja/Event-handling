namespace pvc.Core.Processing
{
    public interface IQueueProcessor
    {
        void ProcessQueue();
        void Stop();
    }
}