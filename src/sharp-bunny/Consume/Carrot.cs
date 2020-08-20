using System.Threading.Tasks;
using RabbitMQ.Client;
using SharpBunny.Connect;

namespace SharpBunny.Consume
{
    public class Carrot<TMsg> : ICarrot<TMsg>
    {
        private readonly PermanentChannel _thisChannel;

        public Carrot(TMsg message, ulong deliveryTag, PermanentChannel thisChannel)
        {
            Message = message;
            DeliveryTag = deliveryTag;
            _thisChannel = thisChannel;
        }

        public TMsg Message { get; }
        
        public ulong DeliveryTag { get; }

        public IBasicProperties MessageProperties { get; set; }

        public async Task<OperationResult<TMsg>> SendAckAsync()
            {
                var result = new OperationResult<TMsg>();
                try
                {
                    await Task.Run(() => 
                                    _thisChannel.Channel.BasicAck(DeliveryTag, multiple: false)
                    );
                    result.IsSuccess = true;
                    result.State = OperationState.Acked;
                    return result;
                }
                catch (System.Exception ex)
                {
                    result.Error = ex;
                    result.IsSuccess = false;
                    result.State = OperationState.Failed;
                }

                return result;
            }

            public async Task<OperationResult<TMsg>> SendNackAsync(bool withRequeue = true)
            {
                var result = new OperationResult<TMsg>();
                try
                {
                    await Task.Run(() => 
                                    _thisChannel.Channel.BasicReject(DeliveryTag, requeue: withRequeue)
                    );
                    result.IsSuccess = true;
                    result.State = OperationState.Nacked;

                    return result;
                }
                catch (System.Exception ex)
                {
                    result.IsSuccess = false;
                    result.Error = ex;
                    result.State = OperationState.Failed;
                }
                return result;
            }
        }
}