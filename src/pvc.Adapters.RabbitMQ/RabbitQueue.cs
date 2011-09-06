using System.IO;
using System.Runtime.Serialization;
using pvc.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;

namespace pvc.Adapters.RabbitMQ
{
	public class RabbitQueue<T> : IQueue<T>
	{
		private readonly string _queue;
		private readonly IFormatter _formatter;
	    private readonly ConnectionFactory _connectionFactory;
		private readonly IConnection _connection;
		private readonly IModel _model;
		private readonly string _hostName;
		private readonly string _exchange;
		private readonly Subscription _subscription;
		private BasicDeliverEventArgs _currentDeliveryArgs;

		public RabbitQueue(RabbitCreationParams rabbitCreationParams)
		{
			_hostName = rabbitCreationParams.HostName;
			_exchange = rabbitCreationParams.Exchange;
			_queue = rabbitCreationParams.Queue;
			_formatter = rabbitCreationParams.Formatter;
		    _connectionFactory = new ConnectionFactory
			{
				HostName = rabbitCreationParams.HostName,
				UserName = rabbitCreationParams.UserName,
				Password = rabbitCreationParams.Password,
				Port = rabbitCreationParams.Port
			};
			_connection = _connectionFactory.CreateConnection();
			_model = _connection.CreateModel();
			_subscription = new Subscription(_model, rabbitCreationParams.Queue, false);
		}

		public bool TryDequeue(out T item)
		{
		    _currentDeliveryArgs = _subscription.Next();
			item = (T)_formatter.Deserialize(new MemoryStream(_currentDeliveryArgs.Body));
			return true;
		}

		public void Enqueue(T item)
		{
			var ibp = _model.CreateBasicProperties();
			using(var ms = new MemoryStream())
			{
                _formatter.Serialize(ms, item);
			    
                var body = ms.ToArray();
			    _model.BasicPublish(_exchange, string.Empty, false, false, ibp, body);
			}
		}

		public void MarkComplete(T item)
		{
			if (_currentDeliveryArgs != null)
			{
			    _subscription.Ack(_currentDeliveryArgs);
			}
		}

        public void Requeue(T item)
        {
            if (_currentDeliveryArgs == null)
            {
                return;
            }

            _model.BasicReject(_currentDeliveryArgs.DeliveryTag, true);
        }

		public override string ToString()
		{
			return string.Format("{0}:{1}:{2}", _hostName, _exchange, _queue);
		}
	}
}