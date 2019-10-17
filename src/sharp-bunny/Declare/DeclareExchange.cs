using System;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using SharpBunny.Exceptions;
using SharpBunny.Utils;

namespace SharpBunny.Declare
{
    public class DeclareExchange : IDeclare
    {
        private readonly IBunny _bunny;
        public DeclareExchange(IBunny bunny)
        {
            _bunny = bunny;
        }

        internal string Name { get; set; }
        internal string ExchangeType { get; set; } = "direct";
        internal bool Durable { get; set; } = true;
        internal bool AutoDelete { get; set; } = false;

        public async Task DeclareAsync()
        {
            bool exists = await _bunny.ExchangeExistsAsync(Name);
             if (exists)
            {
                return;
            }
            IModel channel = null;
            try
            {
                channel = _bunny.Channel();

                await Task.Run(() => 
                {
                    channel.ExchangeDeclare(Name, ExchangeType, Durable, AutoDelete, null);
                });
            }
            catch (System.Exception exc)
            {
                throw DeclarationException.DeclareFailed(exc, "exchange-declare failed!");
            }
            finally
            {
                channel.Close();
            }
        }
    }
}