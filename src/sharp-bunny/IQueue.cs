using System;
using System.Linq;

namespace SharpBunny
{
    public interface IQueue : IDeclare
    {
        ///<summary>
        /// Defaults to Queue.Name
        ///</summary>
        string RoutingKey { get; }
        string Name { get; }
        ///<summary>
        /// the Queue will be dismantled if the last Channel has disconnected
        ///</summary>
        IQueue AsAutoDelete();
        ///<summary>
        /// Bind a Queue to [exchangeName] with [routingKey]
        ///</summary>
        IQueue Bind(string exchangeName, string routingKey);
        ///<summary>
        /// The declared Queue will survive a Broker restart
        ///</summary>
        IQueue AsDurable();
        ///<summary>
        /// Messages in this Queue will live for the configured [ttl] in ms
        ///</summary>
        IQueue WithTTL(uint ttl);
        ///<summary>
        /// the Queue Allows for a maximum of [maxLength] messages
        ///</summary>
        IQueue MaxLength(uint maxLength);
        ///<summary>
        /// The Queue allows for a maximum of [maxBytes] messges
        ///</summary>
        IQueue MaxBytes(uint maxBytes);
        ///<summary>
        /// The Queue disappears after [expire], when it is not used in between
        ///</summary>
        IQueue Expire(uint expire);
        ///<summary>
        /// Lazy Mode on Queues enables a sort of throttling. Use Carefully because it slows down performance
        ///</summary>
        IQueue AsLazy();
        ///<summary>
        /// Sets the overflow behavior from drop-head (default) to reject-publish
        ///</summary>
        IQueue OverflowReject();
    }
}