namespace pvc.Core
{
	public class MessagePublisher<T> : Produces<T>, IMessagePublisher<T> where T : Message
	{
		private readonly Multiplexor<T> _multiplexor = new Multiplexor<T>();

		public void AttachConsumer(Consumes<T> consumer)
		{
			_multiplexor.AttachConsumer(consumer);
		}

		public void Publish(T message)
		{
			_multiplexor.Handle(message);
		}
	} 
}
