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
                fromQueue = SerializeTypeName<TMsg>();
            }
            return new DeclareConsumer<TMsg>(bunny, fromQueue);
        }

        public static IRequest<TRequest, TResponse> Request<TRequest, TResponse>(this IBunny bunny, string rpcExchange, string routingKey = null)
            where TRequest : class
            where TResponse : class
        {
            if (routingKey == null)
            {
                routingKey = SerializeTypeName<TRequest>();
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
                fromQueue = SerializeTypeName<TRequest>();
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

        private static string SerializeTypeName<T>()
            => SerializeTypeName(typeof(T));

        private static string SerializeTypeName(Type t)
            =>  $"{t.Assembly.GetName().Name}.{t.Name}";


        public static IQueue DeadLetterExchange(this IQueue queue, string deadLetterExchange)
            => queue.AddTag("x-dead-letter-exchange", deadLetterExchange);

        public static IQueue DeadLetterRoutingKey(this IQueue queue, string routingKey)
            => queue.AddTag("x-dead-letter-routing-key", routingKey);

        public static IQueue QueueExpiry(this IQueue queue, int expiry)
            => queue.AddTag("x-expires", expiry);

        public static IQueue MaxLength(this IQueue queue, int length)
            => queue.AddTag("x-max-length", length);

        public static IQueue MaxLengthBytes(this IQueue queue, int lengthBytes)
            => queue.AddTag("x-max-length-bytes", lengthBytes);

        public static IQueue AsLazy(this IQueue queue)
             => queue.AddTag("x-queue-mode", "lazy");

        public static IQueue WithTTL(this IQueue queue, uint ttl)
            => queue.AddTag("x-message-ttl", ttl);

        public static IQueue OverflowReject(this IQueue queue)
            => queue.AddTag("x-overflow", "reject-publish");
    }
}