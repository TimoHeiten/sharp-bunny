using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using SharpBunny.Connect;

namespace SharpBunny.Consume
{
    public class DeclareConsumer<TMsg> : IConsume<TMsg>
    {
        private readonly IBunny _bunny;
        private readonly string _consumeFromQueue;
        private  readonly PermanentChannel _thisChannel;

        #region mutable fields
        private EventingBasicConsumer _consumer;
        private bool _useUniqueChannel;
        private Func<ICarrot<TMsg>, Task> _receive;
        private Func<byte[], TMsg> _deserialize;
        private bool _autoAck = false;

        private uint _prefetchCount = 50;
        #endregion

        public DeclareConsumer(IBunny bunny, string fromQueue)
        {
            _bunny = bunny;
            _deserialize = Config.Deserialize<TMsg>;
            _consumeFromQueue = fromQueue;
            _thisChannel = new PermanentChannel(bunny);
            _receive = async carrot => await carrot.SendAckAsync();
        }

        public IConsume<TMsg> AsAutoAck(bool autoAck = true)
        {
            _autoAck = autoAck;
            return this;
        }

        public IConsume<TMsg> Callback(Func<ICarrot<TMsg>, Task> callback)
        {
            _receive = callback;
            return this;
        }

        public async Task<OperationResult<TMsg>> GetAsync(Func<ICarrot<TMsg>, Task> handle)
        {
            var operationResult = new OperationResult<TMsg>();

            try
            {
                await Task.Run(async () =>
                {
                    var result = _thisChannel.Channel.BasicGet(_consumeFromQueue, _autoAck);
                    if (result != null)
                    {
                        var msg = _deserialize(result.Body);
                        var carrot = new Carrot(msg, result.DeliveryTag, _thisChannel);
                        await handle(carrot);
                        operationResult.IsSuccess = true;
                        operationResult.State = OperationState.Get;
                        operationResult.Message = msg;
                    }
                    else
                    {
                        operationResult.IsSuccess = false;
                        operationResult.State = OperationState.GetFailed;
                    }
                });
            }
            catch (System.Exception ex)
            {
                operationResult.IsSuccess = false;
                operationResult.Error = ex;
            }
            return operationResult;
        }

        public IConsume<TMsg> Prefetch(uint prefetchCount = 50)
        {
            _prefetchCount = prefetchCount;
            return this;
        }

        public OperationResult<TMsg> StartConsuming()
        {
            var result = new OperationResult<TMsg>();
            if (_consumer == null)
            {
                try
                {
                    var channel = _thisChannel.Channel;
                    _consumer = new EventingBasicConsumer(channel);
                    _consumer.ConsumerTag = Guid.NewGuid().ToString();
                    _consumer.Received += HandleReceived;

                    int rnd = new Random().Next(0, 999);
                    string consumerTag = typeof(TMsg)+"-"+_consumeFromQueue+"-"+rnd;
                    channel.BasicConsume(_consumeFromQueue, 
                                        _autoAck, 
                                        consumerTag,
                                        noLocal:false, 
                                        exclusive:false, 
                                        arguments:null,
                                        consumer: _consumer);
                 
                    result.State = OperationState.ConsumerAttached;
                    result.IsSuccess = true;
                    result.Message = default(TMsg);
                    return result;
                }
                catch (System.Exception ex)
                {
                    result.IsSuccess = false;
                    result.Error = ex;
                    result.State = OperationState.Failed;
                }
            }
            else 
            {
                result.IsSuccess = true;
                result.State = OperationState.ConsumerAttached;
            }
            return result;
        }

        private async void HandleReceived(object channel, BasicDeliverEventArgs deliverd)
        {
            Carrot carrot = null;
            try
            {
                TMsg message = _deserialize(deliverd.Body);
                carrot = new Carrot(message, deliverd.DeliveryTag, _thisChannel);

                await _receive(carrot);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                if (carrot != null)
                {
                    await carrot.SendNackAsync(withRequeue: true);
                }
            }
        }

        public IConsume<TMsg> UseUniqueChannel(bool useUnique = true)
        {
            _useUniqueChannel = useUnique;
            return this;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_consumer != null)
                    {
                        _thisChannel.Channel.BasicCancel(_consumer.ConsumerTag);
                        _consumer.Received -= HandleReceived;
                    }
                    _thisChannel.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }

        public IConsume<TMsg> DeserializeMessage(Func<byte[], TMsg> deserialize)
        {
            _deserialize = deserialize;
            return this;
        }

        public Task CancelAsync()
        {
            Dispose(true);
            return Task.CompletedTask;
        }
        #endregion

        private class Carrot : ICarrot<TMsg>
        {
            private readonly TMsg _message;
            private ulong _deilvered;
            private PermanentChannel _thisChannel;
            

            public Carrot(TMsg message, ulong deilvered, PermanentChannel thisChannel)
            {
                _message = message;
                _deilvered = deilvered;
                _thisChannel = thisChannel;
            }

            public TMsg Message => _message;

            public async Task<OperationResult<TMsg>> SendAckAsync()
            {
                var result = new OperationResult<TMsg>();
                try
                {
                    await Task.Run(() => _thisChannel.Channel.BasicAck(_deilvered, multiple:false));
                    result.IsSuccess = true;
                    result.State = OperationState.Acked;
                    return result;
                }
                catch (System.Exception ex)
                {
                    result.IsSuccess = false;
                    result.Error = ex;
                    result.State = OperationState.Failed;
                    System.Console.WriteLine(ex);
                }

                return result;
            }

            public async Task<OperationResult<TMsg>> SendNackAsync(bool withRequeue = true)
            {
                var result = new OperationResult<TMsg>();
                try
                {
                    await Task.Run(() => _thisChannel.Channel.BasicReject(_deilvered, requeue:withRequeue));
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
}