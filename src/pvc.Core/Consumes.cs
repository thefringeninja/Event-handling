namespace pvc.Core
{
	public interface Consumes<in TMessage> where TMessage : Message
	{
		void Handle(TMessage message);
	}
}