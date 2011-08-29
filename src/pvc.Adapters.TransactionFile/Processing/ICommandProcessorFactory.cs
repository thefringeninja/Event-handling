using System.Collections.Generic;

namespace pvc.Adapters.TransactionFile.Processing
{
    public interface ICommandProcessorFactory<T>
    {
        IList<ICommandProcessor<T>> GetProcessorsForCommand(T command);
    }
}
