using System;
using System.Diagnostics;

namespace pvc.Core.Processing.Instrumentation
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
            _totalMessages = new PerformanceCounter
                                 {
                                     CategoryName = groupName,
                                     CounterName = "Messages Read",
                                     MachineName = ".",
                                     ReadOnly = false,
                                     RawValue = 0
                                 };

            _messagesPerSecond = new PerformanceCounter
                                     {
                                         CategoryName = groupName,
                                         CounterName = "Messages Read / Sec",
                                         MachineName = ".",
                                         ReadOnly = false
                                     };
        }

        private static void CreateCounters(string groupName)
        {
            if (PerformanceCounterCategory.Exists(groupName))
            {
                PerformanceCounterCategory.Delete(groupName);
            }

            var counters = new CounterCreationDataCollection();

            var totalOps = new CounterCreationData
                               {
                                   CounterName = "Messages Read",
                                   CounterHelp = "Total number of messages read",
                                   CounterType = PerformanceCounterType.NumberOfItems32
                               };
            counters.Add(totalOps);

            var opsPerSecond = new CounterCreationData
                                   {
                                       CounterName = "Messages Read / Sec",
                                       CounterHelp = "Messages read per second",
                                       CounterType = PerformanceCounterType.RateOfCountsPerSecond32
                                   };
            counters.Add(opsPerSecond);

            PerformanceCounterCategory.Create(groupName, "PVC", PerformanceCounterCategoryType.SingleInstance, counters);
        }

        public WMIQueueProcessorInstrumentation(string groupName)
        {
            CreateCounters(groupName);
            InitCounters(groupName);
        }
    }
}
