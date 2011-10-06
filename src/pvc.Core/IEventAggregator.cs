namespace pvc.Core
{
    public interface IEventAggregator<in T> where T : Message
    {
        void AttachTo<TDerived>(Produces<TDerived> producer) where TDerived : T;
        void SubscribeTo<TDerived>(Consumes<TDerived> consumer) where TDerived : T;
    }
}