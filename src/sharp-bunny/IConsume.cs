using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace SharpBunny
{
    public interface IConsume<TMsg> : IDisposable
    {
        IConsume<TMsg> Callback(Func<ICarrot<TMsg>, Task> callback);
        IConsume<TMsg> Prefetch(uint prefetchCount=50);
        IConsume<TMsg> UseUniqueChannel(bool useUnique = true);
        IConsume<TMsg> AsAutoAck(bool autoAck = true);
        IConsume<TMsg> DeserializeMessage(Func<byte[], TMsg> deserialize);

        OperationResult<TMsg> StartConsuming();
        ///<summary>
        /// leave out the carrot.SendAckAsync if you use AutoAck!
        ///</summary>
        Task<OperationResult<TMsg>> GetAsync(Func<ICarrot<TMsg>, Task> carrot);

        Task CancelAsync();
    }

    public interface ICarrot<TMsg>
    {
        TMsg Message { get; }
        Task<OperationResult<TMsg>> SendAckAsync();
        Task<OperationResult<TMsg>> SendNackAsync(bool withRequeue = true);
    }
}