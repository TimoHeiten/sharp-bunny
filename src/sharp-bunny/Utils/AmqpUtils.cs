using System;
using System.Linq;
using RabbitMQ.Client;

namespace SharpBunny.Utils
{
    public static class AmqpUtils
    {
        ///<summary>
        /// returns formattable amqp format like this:
        /// 0 --> user, 1 --> password, 2 --> host, 3 --> port, 4 --> vHost
        ///</summary>
        public static string AmqpFormatString(this string amqp)
        {
            return "amqp://{0}:{1}@{2}:{3}/{4}";
        }

        public static AmqpTcpEndpoint ParseEndpoint(this string amqp_uri)
        {
            return new AmqpTcpEndpoint(new Uri(amqp_uri));
        }
    }
}