using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RabbitMQ.Client;
using SharpBunny.Exceptions;
using SharpBunny.Utils;

[assembly: InternalsVisibleTo("tests")]
namespace SharpBunny.Declare
{
    public class DeclareQueue : IDeclare
    {
        private readonly IBunny _bunny;
        internal DeclareQueue(IBunny bunny, string name)
        { 
            Name = name;
            _bunny = bunny;
        }
        internal string Name {get;}
        internal int? Ttl { get; set; }
        internal bool? Durable { get; set; } = true;
        internal (string ex, string rKey)? BindingKey { get; set; }
        internal int? MaxBytes { get; set; }
        internal int? MaxLength { get; set; }
        internal bool? AutoDelete { get; set; }

        public async Task DeclareAsync()
        {
            bool exists = await _bunny.QueueExistsAsync(Name);
            if (exists)
            {
                return;
            }
            IModel channel = null;
            try
            {
                channel = _bunny.Channel();

                await Declare(channel);
                await Bind(channel);
            }
            catch (System.Exception exc)
            {
                throw DeclarationException.DeclareFailed(exc);
            }
            finally
            {
                channel.Close();
            }
        }

        private Task Declare(IModel channel)
        {
            var arguments = CreateArgs();
            return Task.Run(() => 
                    channel.QueueDeclare(Name,
                    durable: Durable.HasValue ? Durable.Value : true,
                    exclusive: false,
                    autoDelete: AutoDelete.HasValue ? AutoDelete.Value : false,
                    arguments: arguments)
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
                        channel = _bunny.Channel(newOne: true);
                    }
                    channel.QueueBind(Name, ex, bkey, null);
                });
            }
        }

        private Dictionary<string, object> CreateArgs()
        {
            var d = new Dictionary<string, object>();

            if (Ttl.HasValue)
            {
                d.Add("x-message-ttl", Ttl.Value);
            }
            if (MaxLength.HasValue)
            {
                d.Add("x-max-length", MaxLength.Value);
            }
            if (MaxBytes.HasValue)
            {
                d.Add("x-max-length-bytes", MaxBytes.Value);
            }
            if (MaxBytes.HasValue)
            {
                d.Add("x-max-length-bytes", MaxBytes.Value);
            }

            return d.Any() ? d : null;
        }
    }
}