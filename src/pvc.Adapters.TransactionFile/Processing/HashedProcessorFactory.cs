using System;
using System.Collections.Generic;

namespace pvc.Adapters.TransactionFile.Processing
{
    public class HashedCommandProcessorFactory<T> : ICommandProcessorFactory<T>
    {
        private readonly Dictionary<Type, IList<ICommandProcessor<T>>> _table;

        public void AddProcessorToType(Type type, ICommandProcessor<T> processor)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (processor == null)
            {
                throw new ArgumentNullException("processor");
            }
            IList<ICommandProcessor<T>> tmp;
            _table.TryGetValue(type, out tmp);
            if (tmp == null)
            {
                tmp = new List<ICommandProcessor<T>>();
                _table.Add(type, tmp);
            }
            tmp.Add(processor);
        }

        public HashedCommandProcessorFactory()
        {
            _table = new Dictionary<Type, IList<ICommandProcessor<T>>>();
        }

        #region ICommandProcessorFactory<T> Members

        public IList<ICommandProcessor<T>> GetProcessorsForCommand(T command)
        {
            IList<ICommandProcessor<T>> ret;
            _table.TryGetValue(command.GetType(), out ret);
            return ret;
        }

        #endregion
    }
}
