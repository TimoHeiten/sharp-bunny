using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace SharpBunny.Facade
{
    public class MultiBunny : IBunny
    {
        private IConnection _connection;
        private readonly IConnectionFactory _factory;
        private readonly IList<AmqpTcpEndpoint> _amqps;
        private readonly List<IModel> _models = new List<IModel>();
        public MultiBunny(IConnectionFactory factory, IList<AmqpTcpEndpoint> endpoints)
        {
            _factory = factory;
            _amqps = endpoints;

            _connection = factory.CreateConnection(endpoints);
        }

        public IModel Channel(bool newOne = false)
        {
            var open = _models.Where(x => x.IsOpen).ToList();
            _models.Clear();
            _models.AddRange(open);
            if (_models.Any() == false || newOne)
            {
                if (_connection.IsOpen == false)
                {
                    SetConnected();
                }
                var model = _connection.CreateModel();
                _models.Add(model);
                return model;
            }
            else
            {
                return _models.Last();
            }
        }

        private void SetConnected()
        {
            if (_connection.IsOpen == false)
            {
                _connection = _factory.CreateConnection(_amqps);
            }
        }

        public IConnection Connection
        {
            get
            {
                SetConnected();
                return _connection;
            }
        } 

        public void Dispose()
        {
            if (_connection.IsOpen)
                _connection.Dispose();
        }
    }
}