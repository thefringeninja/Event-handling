using System;
using System.Diagnostics;

namespace pvc.Adapters.TransactionFile.Processing.Instrumentation
{
    public class WMIQueueProcessorInstrumentation : IQueueProcessorInstrumentation
    {
        PerformanceCounter _totalMessages;
        PerformanceCounter _messagesPerSecond;

        public void IncrementMessage()
        {
            _totalMessages.Increment();
            _messagesPerSecond.Increment();
        }

        public int TotalMessages
        {
            get
            {
                return Convert.ToInt32(_totalMessages.NextValue());
            }
        }

        public decimal TotalMessagesPerSecond
        {
            get
            {
                return Convert.ToDecimal(_messagesPerSecond.NextValue());
            }
        }

        private void InitCounters(string groupName)
        {
            _totalMessages = new PerformanceCounter();
            _totalMessages.CategoryName = groupName;
            _totalMessages.CounterName = "Messages Read";
            _totalMessages.MachineName = ".";
            _totalMessages.ReadOnly = false;
            _totalMessages.RawValue = 0;

            _messagesPerSecond = new PerformanceCounter();
            _messagesPerSecond.CategoryName = groupName;
            _messagesPerSecond.CounterName = "Messages Read / Sec";
            _messagesPerSecond.MachineName = ".";
            _messagesPerSecond.ReadOnly = false;
        }

        private static void CreateCounters(string groupName)
        {
            if (PerformanceCounterCategory.Exists(groupName))
            {
                PerformanceCounterCategory.Delete(groupName);
            }

            var counters = new CounterCreationDataCollection();

            var totalOps = new CounterCreationData();
            totalOps.CounterName = "Messages Read";
            totalOps.CounterHelp = "Total number of messages read";
            totalOps.CounterType = PerformanceCounterType.NumberOfItems32;
            counters.Add(totalOps);

            var opsPerSecond = new CounterCreationData();
            opsPerSecond.CounterName = "Messages Read / Sec";
            opsPerSecond.CounterHelp = "Messages read per second";
            opsPerSecond.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
            counters.Add(opsPerSecond);

            PerformanceCounterCategory.Create(groupName, "PVC", counters);
        }

        public WMIQueueProcessorInstrumentation(string groupName)
        {
            CreateCounters(groupName);
            InitCounters(groupName);
        }
    }

}
