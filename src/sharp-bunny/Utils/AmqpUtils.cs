using System;
using System.Linq;
using RabbitMQ.Client;

namespace SharpBunny.Utils
{
    public static class AmqpUtils
    {
        public static AmqpTcpEndpoint ParseEndpoint(this string amqp_uri)
        {
            return new AmqpTcpEndpoint(new Uri(amqp_uri));
        }
    }
}