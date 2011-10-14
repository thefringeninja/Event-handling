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
}