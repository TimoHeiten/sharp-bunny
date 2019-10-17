using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RabbitMQ.Client;
using SharpBunny.Exceptions;
using SharpBunny.Utils;
using System.Linq;

[assembly: InternalsVisibleTo("tests")]
namespace SharpBunny.Declare
{
    public class DeclareQueue : IQueue
    {
        public IBunny Bunny { get; set; }
        internal DeclareQueue(IBunny bunny, string name)
        { 
            Name = name;
            Bunny = bunny;
        }
        public string Name {get;}
        internal bool? Durable { get; set; } = false;
        internal (string ex, string rKey)? BindingKey { get; set; }
        internal bool? AutoDelete { get; set; }
        private readonly Dictionary<string, object> _arguments = new Dictionary<string, object>();

        public async Task DeclareAsync()
        {
            bool exists = await Bunny.QueueExistsAsync(Name);
            if (exists)
            {
                return;
            }
            IModel channel = null;
            try
            {
                channel = Bunny.Channel();

                await Declare(channel);
                await Bind(channel);
            }
            catch (System.Exception exc)
            {
                throw DeclarationException.DeclareFailed(exc, "queue-declare failed");
            }
            finally
            {
                channel.Close();
            }
        }

        private Task Declare(IModel channel)
        {
            return Task.Run(() => 
                    channel.QueueDeclare(Name,
                    durable: Durable.HasValue ? Durable.Value : true,
                    exclusive: false,
                    autoDelete: AutoDelete.HasValue ? AutoDelete.Value : false,
                    arguments: _arguments.Any() ? _arguments : null)
                );
        }

        private async Task Bind(IModel channel)
        {
            if (BindingKey.HasValue)
            {
                var (ex, bkey) = BindingKey.Value;
                await Task.Run(() => 
                {
                    if (channel.IsClosed)
                    {
                        channel = Bunny.Channel(newOne: true);
                    }
                    channel.QueueBind(Name, ex, bkey, null);
                });
            }
        }

        public IQueue AsAutoDelete()
        {
            AutoDelete = true;
            return this;
        }

        public IQueue Bind(string exchangeName, string routingKey)
        {
            if (exchangeName == null || string.IsNullOrWhiteSpace(routingKey))
            {
                throw DeclarationException.Argument(new System.ArgumentException("exchangename must not be null and routingKey must not be Null, Empty or Whitespace"));
            }
            BindingKey = (exchangeName, routingKey);
            return this;
        }

        public IQueue AsDurable()
        {
            Durable = true;
            return this;
        }

        public IQueue WithTTL(uint ttl)
        {
            _arguments.Add("x-message-ttl", (int)ttl);
            return this;
        }

        public IQueue MaxLength(uint maxLength)
        {
            _arguments.Add("x-max-length", (int)maxLength);
            return this;
        }

        public IQueue MaxBytes(uint maxBytes)
        {
            _arguments.Add("x-max-length-bytes", (int)maxBytes);
            return this;
        }

        public IQueue Expire(uint expire)
        {
            _arguments.Add("x-expires", (int)expire);
            return this;
        }

        public IQueue AsLazy()
        {
            _arguments.Add("x-queue-mode", "lazy");
            return this;
        }

        public IQueue OverflowReject()
        {
            _arguments.Add("x-overflow", "reject-publish");
            return this;
        }
    }
}