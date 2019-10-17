using System;
using System.Linq;
using SharpBunny.Declare;

namespace SharpBunny
{
    public static class OutwardExtensions
    { 
        public static IPublish PublishAsync<TMsg>(this IBunny bunny, TMsg msg)
            where TMsg : class
        {
            return null;
        }

        public static IPublish RequestAsync<TRequest, TResponse>(this IBunny bunny, TRequest msg)
            where TRequest : class
        {
            return null;
        }

        ///<summary>
        /// Interface for building Queues, Exchanges, Bindings and so on
        ///</summary>
        public static IDeclare Declare(this IBunny bunny)
        {
            return new DeclareBase() { Bunny = bunny };
        }
    }
}