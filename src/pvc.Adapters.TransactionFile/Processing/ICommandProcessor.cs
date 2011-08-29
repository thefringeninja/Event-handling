namespace pvc.Adapters.TransactionFile.Processing
{
    public interface ICommandProcessor<in T>
    {
        void ProcessCommand(T command);
    }
}
