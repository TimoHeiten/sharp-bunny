using System;
using RabbitMQ.Client;

namespace SharpBunny
{
    public interface IBunny : IDisposable
    {
        ///<summary>
        /// use only if absolutely necessary
        /// always a new Channel from RabbitMQ.Client and the current Connection
        ///</summary>
        IModel Channel(bool newOne=false);
        IConnection Connection { get; }
    }
}