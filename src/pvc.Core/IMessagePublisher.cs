namespace pvc.Core
{
	public interface IMessagePublisher<in T> where T : Message
	{
		void Publish(T message);
	}
}