namespace pvc.Core
{
    public interface IDispatcher<TBaseMessage> : Consumes<TBaseMessage> where TBaseMessage : Message
    {
        void Subscribe<TDerivedMessage>(Consumes<TDerivedMessage> handler) where TDerivedMessage : TBaseMessage;
    }
}