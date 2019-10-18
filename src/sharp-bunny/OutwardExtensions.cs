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

        public static IConsume Consume<TMsg>(this IBunny bunny)
        {
            return null;
        }

        public static IRequest Request<TRequest, TResponse>(this IBunny bunny, TRequest msg)
            where TRequest : class
        {
            return null;
        }

        public static IRespond Respond<TRequest, TResponse>(this IBunny bunny)
        {
            return null;
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