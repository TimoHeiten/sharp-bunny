using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
namespace SharpBunny
{
    public interface IPublish<T> : IDisposable
        where T : class
    {
        IPublish<T> WithConfirm(Func<BasicAckEventArgs, Task> onAck, Func<BasicNackEventArgs, Task> onNack);
        IPublish<T> AsMandatory(Func<BasicReturnEventArgs, Task> onReturn);
        IPublish<T> AsPersistent();
        IPublish<T> WithExpire(uint expire);
        ///<summary>
        ///asynchronously sends the Message to the MessageBroker
        ///</summary>
        Task<OperationResult<T>> SendAsync(T message, bool force = false);
        IPublish<T> WithSerialize(Func<T, byte[]> serialize);
        ///<summary>
        /// If not specified the TypeName is used
        ///</summary>
        IPublish<T> WithRoutingKey(string routingKey);
        ///<summary>
        /// If QueueName == null --> use the typeof(T).FullName property as QueueName. If no routingKey is specified, use the Type name.
        /// Uses a durable Queue by default. If you need further Tuning, Declare a Queue by yourself and use the overload with IQueue
        ///</summary>
        IPublish<T> WithQueueDeclare(string queueName = null, string routingKey= null, string exchangeName = "amq.direct");
        IPublish<T> WithQueueDeclare(IQueue queue);
        IPublish<T> UseUniqueChannel(bool uniqueChannel=true);
    }

}