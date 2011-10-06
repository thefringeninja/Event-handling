namespace pvc.Core.Processing
{
    public interface ICommandProcessor<in T>
    {
        void ProcessCommand(T command);
    }
}
