using System;
using RabbitMQ.Client;

namespace SharpBunny
{
    public interface IBunny : IDisposable
    {
        ///<summary>
        /// returns the current channel. Creates One if none exists. 
        /// if newOne is set to true, you get a new Channel object
        ///</summary>
        IModel Channel(bool newOne=false);
        ///<summary>
        ///the connection that is established with the Bunny.Connect methods.
        /// Only one exists for every Bunny.
        ///</summary>
        IConnection Connection { get; }
    }
}