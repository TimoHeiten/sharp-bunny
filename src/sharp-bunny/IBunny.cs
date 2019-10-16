using RabbitMQ.Client;

namespace SharpBunny
{
    public interface IBunny
    {
        ///<summary>
        /// use only if absolutely necessary
        /// always a new Channel from RabbitMQ.Client and the current Connection
        ///</summary>
        IModel Channel { get; }
    }
}