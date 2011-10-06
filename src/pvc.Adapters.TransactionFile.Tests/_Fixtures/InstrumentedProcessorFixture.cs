using pvc.Core.Processing.Instrumentation;

namespace pvc.Adapters.TransactionFile.Tests._Fixtures
{
    public class InstrumentedProcessorFixture
    {
        private readonly WMIQueueProcessorInstrumentation _instrument;

        public WMIQueueProcessorInstrumentation Instrument
        {
            get { return _instrument; }
        }

        public InstrumentedProcessorFixture(string groupName)
        {
            _instrument = new WMIQueueProcessorInstrumentation(groupName);
        }
    }
}