using System.Runtime.Serialization;

namespace pvc.Adapters.RabbitMQ
{
	public class RabbitCreationParams
	{
		public string HostName { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public int Port { get; set; }
		public string Exchange { get; set; }
		public string Queue { get; set; }
		public IFormatter Formatter { get; set; }
		public bool RequiresAck { get; set; }
        public int PrefetchCount { get; set; }
        public bool PersistentMessages { get; set; }

		public RabbitCreationParams()
		{

		}

		public RabbitCreationParams(string hostName, string userName, string password, int port, string exchange, string queue, IFormatter formatter, bool requiresAck, int prefetchCount, bool persistentMessages)
		{
			HostName = hostName;
			UserName = userName;
			Password = password;
			Port = port;
			Exchange = exchange;
			Queue = queue;
			Formatter = formatter;
			RequiresAck = requiresAck;
            PrefetchCount = prefetchCount;
            PersistentMessages = persistentMessages;
		}
	}
}