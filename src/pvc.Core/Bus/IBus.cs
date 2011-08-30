namespace pvc.Core.Bus
{
    public interface IBus
    {
        void Publish(Event @event);
        void Send(Command command);
    }
}