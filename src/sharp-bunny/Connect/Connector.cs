using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client.Exceptions;

namespace SharpBunny.Connect
{
    public class Connector : IConnector
    {
        private readonly List<string> _nodes = new List<string>();

        public IConnector AddNode(IConnectPipe pipe)
        {
            AddNodeInternal(pipe);
            return this;
        }

        public IConnector AddNode(string amqp_uri)
        {
            if (_nodes.All(x => x != amqp_uri));
                _nodes.Add(amqp_uri);
            return this;
        }

        public IConnector AddNode(ConnectionParameters pipe)
        {
            AddNodeInternal(pipe);
            return this;
        }

        private void AddNodeInternal(IFormattable formattable = null, string amqpTry = null)
        {
            string amqp = amqpTry ?? formattable.ToString("amqp", null);
            if (_nodes.All(x => x != amqp))
            {
                _nodes.Add(amqp);
            }
        }   

        public IBunny Connect()
        {
            foreach (var node in _nodes)
            {
                try
                {
                    return Bunny.ConnectSingle(node);
                }
                catch
                {
                    // purposefully left blank
                }
            }
            var nodeNames = string.Join(" | ", _nodes);
            var inner = new ArgumentException($"none of the specified can be connected to\n: {nodeNames}");
            throw new BrokerUnreachableException(inner);
        }

        private uint _retry;
        private uint _timeout;
        public IConnector WithRetry(int retry = 5, int timeout = 2)
        {
            _retry = (uint)Math.Abs(retry);
            _timeout = (uint)Math.Abs(timeout);
            return this;
        }
    }
}