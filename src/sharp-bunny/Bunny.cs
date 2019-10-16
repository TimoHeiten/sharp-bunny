using System;
using System.Linq;

namespace SharpBunny
{
    public static class Bunny
    {
        public static IBunny ConnectSingle(ConnectionParameters parameters)
        {
            throw new InvalidOperationException();
        }

        public static IConnector ConnectSingle(string amqp_uri)
        {
            throw new InvalidOperationException();
        }

        public static IConnectPipe ConnectSingleWith()
        {
            throw new InvalidOperationException();
        }

        public static IConnector ClusterConnect()
        {
            throw new InvalidOperationException();
        }
    }

    public interface IConnector
    {
        IConnector AddNode(IConnectPipe pipe);
        IConnector AddNode(string amqp_uri);
        IConnector AddNode(ConnectionParameters pipe);
        IBunny Connect();
    }

    public class ConnectionParameters
    {
        public string Host { get; set; }
        public uint Port { get; set; }
        public string  User { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
    }
}