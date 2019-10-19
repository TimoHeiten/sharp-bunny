using System;
using System.Threading.Tasks;
using SharpBunny.Consume;
using SharpBunny.Declare;
using SharpBunny.Publish;

namespace SharpBunny
{
    public static class OutwardExtensions
    { 
        public static IPublish<TMsg> Publisher<TMsg>(this IBunny bunny, string publishToExchange)
            where TMsg : class
        {
            return new DeclarePublisher<TMsg>(bunny, publishToExchange);
        }

        public static IConsume<TMsg> Consumer<TMsg>(this IBunny bunny, string fromQueue = null)
        {
            if (fromQueue == null)
            {
                fromQueue = typeof(TMsg).FullName;
            }
            return new DeclareConsumer<TMsg>(bunny, fromQueue);
        }

        public static IRequest<TRequest, TResponse> Request<TRequest, TResponse>(this IBunny bunny, string rpcExchange, string routingKey = null)
            where TRequest : class
            where TResponse : class
        {
            if (routingKey == null)
            {
                routingKey = typeof(TRequest).FullName;
            }
            return new DeclareRequest<TRequest, TResponse>(bunny, rpcExchange, routingKey);
        }

        public static IRespond<TRequest, TResponse> Respond<TRequest, TResponse>(this IBunny bunny, string rpcExchange
            , Func<TRequest, Task<TResponse>> respond, string fromQueue = null)
        where TRequest : class
        where TResponse : class
        {
            if (fromQueue == null)
            {
                fromQueue = typeof(TRequest).FullName;
            }
            return new DeclareResponder<TRequest, TResponse>(bunny, rpcExchange, fromQueue, respond);
        }

        ///<summary>
        /// Interface for building Queues, Exchanges, Bindings and so on
        ///</summary>
        public static IDeclare Setup(this IBunny bunny)
        {
            return new DeclareBase() { Bunny = bunny };
        }
    }
}