using System.Collections.Generic;

namespace pvc.Core.Processing
{
    public interface ICommandProcessorFactory<T>
    {
        IList<ICommandProcessor<T>> GetProcessorsForCommand(T command);
    }
}
