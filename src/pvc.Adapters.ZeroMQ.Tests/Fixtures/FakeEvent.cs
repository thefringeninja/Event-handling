using System;
using pvc.Core.Bus;

namespace pvc.Adapters.ZeroMQ.Tests.Bus
{
    [Serializable]
    public class FakeEvent : Event
    {
        public string Id { get; set; }

        public FakeEvent()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}