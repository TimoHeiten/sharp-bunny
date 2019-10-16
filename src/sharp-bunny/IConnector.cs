using System;

namespace SharpBunny
{
    public interface IConnector
    {
        IConnector AddNode(string amqp_uri);
        IConnector AddNode(IConnectPipe pipe);
        IConnector AddNode(ConnectionParameters pipe);
        IConnector WithRetry(int retry=5, int timeout=2);
        IBunny Connect();
    }
}