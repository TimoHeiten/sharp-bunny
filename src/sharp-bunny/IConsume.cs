using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace SharpBunny
{
    public interface IConsume<TMsg> : IDisposable
    {
        ///<summary>
        /// Define what your consumer does with the message. Carrot helps to ack/nack messages
        ///</summary>
        IConsume<TMsg> Callback(Func<ICarrot<TMsg>, Task> callback);
        ///<summary>
        /// define basic quality of service
        ///</summary>
        IConsume<TMsg> Prefetch(uint prefetchCount=50);
        ///<summary>
        ///your_comment_here
        ///</summary>
        IConsume<TMsg> UseUniqueChannel(bool useUnique = true);
        IConsume<TMsg> AsAutoAck(bool autoAck = true);
        IConsume<TMsg> DeserializeMessage(Func<byte[], TMsg> deserialize);

        ///<summary>
        /// Starts consuming from the specified or forceDeclared Queue
        /// force: creates the queue to be consumed from
        ///</summary>
        Task<OperationResult<TMsg>> StartConsumingAsync(IQueue force = null);
        ///<summary>
        /// leave out the carrot.SendAckAsync if you use AutoAck!
        ///</summary>
        Task<OperationResult<TMsg>> GetAsync(Func<ICarrot<TMsg>, Task> carrot);

        Task CancelAsync();

        IConsume<TMsg> AddTag(string tag, object value);
    }

    public interface ICarrot<TMsg>
    {
        TMsg Message { get; }
        IBasicProperties MessageProperties { get; }
        Task<OperationResult<TMsg>> SendAckAsync();
        Task<OperationResult<TMsg>> SendNackAsync(bool withRequeue = true);
    }
}