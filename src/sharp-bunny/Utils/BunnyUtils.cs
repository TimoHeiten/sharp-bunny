using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using SharpBunny.Exceptions;

namespace SharpBunny.Utils
{
    public static class BunnyUtils
    {
        internal static async Task<bool> QueueExistsAsync(this IBunny bunny, string name)
        {
            try
            {
                var channel = bunny.Channel(newOne:true);
                var result = await new TaskFactory().StartNew<QueueDeclareOk>(() => channel.QueueDeclarePassive(name));    

                return true;
            }
            catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw DeclarationException.DeclareFailed(ex);
            }

        }
    }
}