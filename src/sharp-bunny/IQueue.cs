using System;
using System.Linq;

namespace SharpBunny
{
    public interface IQueue : IDeclare
    {
        IQueue AsAutoDelete();
        IQueue Bind(string exchangeName, string routingKey);
        IQueue AsDurable();
        IQueue WithTTL(uint ttl);
        IQueue MaxLength(uint maxLength);
        IQueue MaxBytes(uint maxBytes);
        IQueue Expire(uint expire);
        IQueue AsLazy();
        IQueue OverflowReject();
    }
}