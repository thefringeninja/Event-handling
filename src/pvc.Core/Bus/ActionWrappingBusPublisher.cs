using System;

namespace pvc.Core.Bus
{
    public class ActionWrappingBusPublisher : IBus, Produces<Message>
    {
        private readonly Action<Event> _eventAction;
        private readonly Action<Command> _commandAction;
        private Consumes<Message> _consumer;

        public ActionWrappingBusPublisher(Action<Event> eventAction, Action<Command> commandAction)
        {
            _eventAction = eventAction;
            _commandAction = commandAction;
        }

        public void Publish(Event @event)
        {
            _eventAction(@event);
            if (_consumer != null)
            {
                _consumer.Handle(@event);
            }
        }

        public void Send(Command command)
        {
            _commandAction(command);
            if (_consumer != null)
            {
                _consumer.Handle(command);
            }
        }

        public void AttachConsumer(Consumes<Message> consumer)
        {
            _consumer = consumer;
        }
    }
}