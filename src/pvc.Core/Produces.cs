namespace pvc.Core
{
	public interface Produces<out T> where T:Message
	{
		void AttachConsumer(Consumes<T> consumer);
	}
}