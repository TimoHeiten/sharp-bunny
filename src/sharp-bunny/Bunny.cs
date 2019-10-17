using System;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using SharpBunny.Connect;

namespace SharpBunny
{
    public static class Bunny
    {
        public static uint RetryCount { get; set; } = 3;
        public static uint RetryPauseInMS { get; set; } = 1500;
        public static IBunny ConnectSingle(ConnectionParameters parameters)
        {
            return Connect(parameters);
        }

        public static IBunny ConnectSingle(string amqp_uri)
        {
            return Connect(new AmqpTransport { AMQP = amqp_uri} );
        }

        private class AmqpTransport : IFormattable
        {
            public string AMQP { get; set; }

            public string ToString(string format, IFormatProvider formatProvider)
            {
                return AMQP;
            }
        }

        public static IConnectPipe ConnectSingleWith()
        {
            return new ConnectionPipe();
        }

        public static IConnector ClusterConnect()
        {
            return new Cnnctr();
        }

        private static IBunny Connect(IFormattable formattable)
        {
            var factory = new ConnectionFactory();
            var amqp = formattable.ToString("amqp", null);
            factory.Uri = new Uri(amqp);

            int count = 0;
            while (count <= RetryCount)
            {
                try
                {
                    return new Facade.Bunny(factory);
                }
                catch
                {
                    count++;
                    Thread.Sleep((int)RetryPauseInMS);
                }
            }
            throw new BrokerUnreachableException(new InvalidOperationException($"cannot find any broker at {amqp}"));
        }
    }   
}