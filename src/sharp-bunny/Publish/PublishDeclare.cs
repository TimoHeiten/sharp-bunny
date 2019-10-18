using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharpBunny.Connect;

namespace SharpBunny.Publish
{
    public class PublishDeclare<T> : IPublish<T>
        where T : class
    {
        private readonly IBunny _bunny;
        private Func<T, byte[]> _serialize;
        private readonly PermanentChannel _thisChannel;
        private readonly string _publishTo;
        public PublishDeclare(IBunny bunny, string publishTo)
        {
            _bunny = bunny;
            _publishTo = publishTo;
            _serialize = Config.Serialize;
            _thisChannel = new PermanentChannel(bunny);
        }

        private bool Mandatory { get; set; }
        private bool ConfirmActivated { get; set; }
        private bool Persistent { get; set; }
        private int? Expires { get; set; }
        private string RoutingKey 
        {
            get
            {
                if (_routingKey != null)
                {
                    return _routingKey;
                }
                if (_queueDeclare != null)
                {
                    return _queueDeclare.RoutingKey;
                }

                return typeof(T).FullName;
            }
        } 
        private string _routingKey;
        private bool _uniqueChannel;
        private IQueue _queueDeclare;
        private Func<BasicReturnEventArgs, Task> _returnCallback = context => Task.CompletedTask;
        private bool _useConfirm;
        private Func<BasicAckEventArgs, Task> _ackCallback = context => Task.CompletedTask;
        private Func<BasicNackEventArgs, Task> _nackCallback = context => Task.CompletedTask;

        public IPublish<T> AsMandatory(Func<BasicReturnEventArgs, Task> onReturn)
        {
            _returnCallback = onReturn;
            Mandatory = true;
            return this;
        }

        public IPublish<T> AsPersistent()
        {
            Persistent = true;
            return this;
        }

        public IPublish<T> WithConfirm(Func<BasicAckEventArgs, Task> onAck, Func<BasicNackEventArgs, Task> onNack)
        {
            _useConfirm = true;
            _ackCallback = onAck;
            _nackCallback = onNack;
            return this;
        }

        public IPublish<T> WithExpire(uint expire)
        {
            Expires = (int)expire;
            return this;
        }

        public IPublish<T> WithSerialize(Func<T, byte[]> serialize)
        {
            _serialize = serialize;
            return this;
        }

        public IPublish<T> WithRoutingKey(string routingKey)
        {
            _routingKey = routingKey;
            return this;
        }

        public virtual async Task<OperationResult<T>> SendAsync(T msg, bool force = false)
        {
            var operationResult = new OperationResult<T>();
            operationResult.Message = msg;
            IModel channel = null;
            try
            {
                channel = _thisChannel.Channel;

                var properties = ConstructProperties(channel.CreateBasicProperties());
                Handlers(channel);

                if (_queueDeclare != null)
                {
                   await _queueDeclare.DeclareAsync();
                }
                if (force)
                {
                    await _bunny.Setup()
                                .Exchange(_publishTo)
                                .AsDurable()
                                .DeclareAsync();
                }

                await Task.Run(() => 
                {
                    if (_useConfirm)
                        channel.ConfirmSelect();

                    channel.BasicPublish(_publishTo, RoutingKey, mandatory: Mandatory, properties, _serialize(msg));

                    if (_useConfirm)
                        channel.WaitForConfirmsOrDie();
                });

                operationResult.IsSuccess = true;
            }
            catch (System.Exception ex)
            {
                operationResult.IsSuccess = false;
                operationResult.Error = ex;
            }
            finally
            {
                if (_uniqueChannel)
                {
                    Handlers(channel, dismantle: true);
                    channel?.Close();
                }
            }
            return operationResult;
        }

        public IPublish<T> UseUniqueChannel(bool uniqueChannel = true)
        {
            _uniqueChannel = uniqueChannel;
            return this;
        }

        private void Handlers(IModel channel, bool dismantle = false)
        {
            if (Mandatory)
            {
                if (dismantle)
                {
                    channel.BasicReturn -= HandleReturn;
                }
                else
                {
                    channel.BasicReturn += HandleReturn;
                }
            }
            if (_useConfirm)
            {
                if (dismantle)
                {
                    channel.BasicNacks -= HandleNack;
                    channel.BasicAcks -= HandleAck;
                }
                else
                {
                    channel.BasicNacks += HandleNack;
                    channel.BasicAcks += HandleAck;
                }
            }
        }

        private async void HandleReturn(object sender, BasicReturnEventArgs eventArgs)
        {
            await _returnCallback(eventArgs);
        }

        private async void HandleAck(object sender, BasicAckEventArgs eventArgs)
        {
            await _ackCallback(eventArgs);
        }

        private async void HandleNack(object sender, BasicNackEventArgs eventArgs)
        {
            await _nackCallback(eventArgs);
        }

        protected virtual IBasicProperties ConstructProperties(IBasicProperties basicProperties)
        {
            basicProperties.Persistent = Persistent;
            basicProperties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            basicProperties.Type = typeof(T).FullName;
            if (Expires.HasValue)
            {
                basicProperties.Expiration = Expires.Value.ToString();
            }
            basicProperties.CorrelationId = Guid.NewGuid().ToString();
            basicProperties.ContentType = Config.ContentType;
            basicProperties.ContentEncoding = Config.ContentEncoding;

            return basicProperties;
        }

        public IPublish<T> WithQueueDeclare(string queueName = null,string routingKey = null, string exchangeName = "amq.direct")
        {
            string name = queueName ?? typeof(T).FullName;
            string rKey = routingKey ?? typeof(T).FullName;
            _queueDeclare = _bunny.Setup().Queue(name).Bind(exchangeName, rKey).AsDurable();
            return this;
        }

        public IPublish<T> WithQueueDeclare(IQueue queueDeclare)
        {
            _queueDeclare = queueDeclare;
            return this;
        }

        public void Dispose()
        {
            Handlers(_thisChannel.Channel, dismantle: true);
            _thisChannel.Dispose();
        }
    }
}