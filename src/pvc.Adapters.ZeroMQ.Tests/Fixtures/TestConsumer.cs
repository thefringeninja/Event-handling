using pvc.Adapters.ZeroMQ.Tests.Bus;
using pvc.Core;

namespace pvc.Adapters.ZeroMQ.Tests.Fixtures
{
    public class TestConsumer : Consumes<Message>
    {
        public bool Received { get; set; }
        public void Handle(Message message)
        {
            Received = true;
        }
    }

    public class FakeEventConsumer : Consumes<FakeEvent>
    {
        public string Id { get; private set; }

        public void Handle(FakeEvent message)
        {
            Id = message.Id;
        }
    }
}