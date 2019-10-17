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

        public static IDeclare Declare(this IBunny bunny)
        {
            return new DeclareBase() { Bunny = bunny };
        }
    }
}