using RabbitMQ.Client;

namespace SharpBunny.Facade
{
    public class Bunny : IBunny
    {
        private readonly IConnection _connection;
        public Bunny(IConnection connection)
        {
            _connection = connection;
        }

        public IModel Channel => _connection.CreateModel();

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}