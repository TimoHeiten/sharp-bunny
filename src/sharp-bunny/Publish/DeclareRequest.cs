using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharpBunny.Connect;
using SharpBunny.Utils;

namespace SharpBunny.Publish
{
    public class DeclareRequest<TRequest, TResponse> : IRequest<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        public const string DIRECT_REPLY_TO = "amq.rabbitmq.reply-to";
        #region immutable fields
        private readonly IBunny _bunny;
        private readonly string _toExchange;
        private readonly string _routingKey;
        private readonly PermanentChannel _thisChannel;
        private readonly AsyncManualResetEvent _turnstile = new AsyncManualResetEvent();
        #endregion 

        #region mutable fields
        private Func<byte[], TResponse> _deserialize;
        private Func<TRequest, byte[]> _serialize;
        private bool _useTempQueue;
        private bool _useUniqueChannel;
        private IQueue _queueDeclare;
        private string RoutingKey 
        {
            get
            {
                if (_queueDeclare != null)
                {
                    return _queueDeclare.RoutingKey;
                }

                return _routingKey;
            }
        } 
        #endregion
        internal DeclareRequest(IBunny bunny, string toExchange, string routingKey)
        {
            _bunny = bunny;
            _toExchange = toExchange;
            _routingKey = routingKey;
            _serialize = Config.Serialize;
            _deserialize = Config.Deserialize<TResponse>;
            _thisChannel = new PermanentChannel(_bunny);
        }
        
        public async Task<OperationResult<TResponse>> RequestAsync(TRequest request, bool force = false)
        {
            // serialize
            var bytes = _serialize(request);
            var result = new OperationResult<TResponse>();

            var channel = _thisChannel.Channel;
            if (force)
            {
                channel.ExchangeDeclare(_toExchange, 
                                    type:"direct", 
                                    durable: true, 
                                    autoDelete: false,
                                    arguments: null);
            }

            string reply_to = _useTempQueue ? channel.QueueDeclare().QueueName : DIRECT_REPLY_TO;
            result = await PublishAsync(channel, reply_to, bytes, result);
            if (result.IsSuccess)
            {
                result = await ConsumeAsync(channel, reply_to, result);
            }

            if (_useUniqueChannel)
            {
                _thisChannel.Channel.Close();
            }

            return result;
        }

        private async Task<OperationResult<TResponse>> PublishAsync(IModel channel, string reply_to, byte[] payload
            , OperationResult<TResponse> result)
        {
            // publish
            var props = channel.CreateBasicProperties();
            props.ReplyTo = reply_to;
            DeclarePublisher<TRequest>.ConstructProperties(props, persistent:false, expires:500);
            try
            {
                await Task.Run(() => 
                    channel.BasicPublish(_toExchange, RoutingKey, mandatory: false, props, payload)
                );
            }
            catch (System.Exception ex)
            {
                result.IsSuccess = false;
                result.Error = ex;
                result.State = OperationState.RequestFailed;
            }

            return result;
        }

        private async Task<OperationResult<TResponse>> ConsumeAsync(IModel channel, string reply_to, OperationResult<TResponse> result)
        {
            EventHandler<BasicDeliverEventArgs> handle = (s, ea) => 
            {
                try
                {
                    TResponse response = _deserialize(ea.Body);
                    result.Message = response;
                    result.IsSuccess = true;
                    result.State = OperationState.RpcSucceeded;
                }
                catch (System.Exception ex)
                {
                    result.Error = ex;
                    result.IsSuccess = false;
                    result.State = OperationState.ResponseFailed;
                }
                finally
                {
                    _turnstile.Reset();
                }
            };
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += handle;
            string tag = $"temp-consumer {typeof(TRequest)}-{typeof(TResponse)}-{Guid.NewGuid()}";
            await Task.Run(() => channel.BasicConsume(reply_to, 
                                autoAck:true,
                                consumerTag:$"temp-consumer {typeof(TRequest)}-{typeof(TResponse)}",
                                noLocal: false,
                                exclusive: false,
                                arguments: null,
                                consumer: consumer));

            // secure wait
            await _turnstile.WaitAsync();
            // dispose handler
            consumer.Received -= handle;
            await Task.Run(() => channel.BasicCancel(tag));

            return result;
        }

        public IRequest<TRequest, TResponse> WithTemporaryQueue(bool useTempQueue = true)
        {
            _useTempQueue = useTempQueue;
            return this;
        }

        public IRequest<TRequest, TResponse> WithQueueDeclare(string queue=null, string exchange=null, string routingKey=null)
        {
            string name = queue ?? typeof(TRequest).FullName;
            string rKey = routingKey ?? typeof(TRequest).FullName;
            _queueDeclare = _bunny.Setup().Queue(name).Bind(_toExchange, rKey).AsDurable();
            return this;
        }

        public IRequest<TRequest, TResponse> WithQueueDeclare(IQueue queue)
        {
            _queueDeclare = queue;
            return this;
        }

        public IRequest<TRequest, TResponse> SerializeRequest(Func<TRequest, byte[]> serialize)
        {
            _serialize = serialize;
            return this;
        }

        public IRequest<TRequest, TResponse> DeserializeResponse(Func<byte[], TResponse> deserialize)
        {
            _deserialize = deserialize;
            return this;
        }

        public IRequest<TRequest, TResponse> UseUniqueChannel(bool useUnique = true)
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
                    _turnstile.Reset();
                    _thisChannel.Dispose();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true); 
        }
        #endregion
    }
}