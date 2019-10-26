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
        IQueue AddTag(string tag, object value);
    }
}