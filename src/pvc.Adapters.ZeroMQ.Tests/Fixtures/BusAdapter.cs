using pvc.Core;
using pvc.Core.Bus;

namespace pvc.Adapters.ZeroMQ.Tests.Fixtures
{
    public class BusAdapter : IBus, Produces<Message>
    {
        private Consumes<Message> _consumer;

        public virtual void Publish(Event theEvent)
        {
            _consumer.Handle(theEvent);
        }

        public virtual void Send(Command command)
        {
            _consumer.Handle(command);
        }

        public void AttachConsumer(Consumes<Message> consumer)
        {
            _consumer = consumer;
        }
    }
}