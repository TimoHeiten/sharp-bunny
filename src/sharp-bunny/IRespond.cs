using System;
using System.Threading.Tasks;

namespace SharpBunny
{
    public interface IRespond<TRequest, TResponse> : IDisposable
    { 
        ///<summary>
        /// StartsResponding asynchronously to the specified Request. Make sure to have a matching IRequest on the other side.
        /// Be aware of changing the defaults (with regards to queue naming etc.)--> apply on both sides
        ///</summary>
        Task<OperationResult<TResponse>> StartRespondingAsync();
        ///<summary>
        /// Override serialize method
        ///</summary>
        IRespond<TRequest, TResponse> WithSerialize(Func<TResponse, byte[]> serialize);
        ///<summary>
        /// Override Deserialize method
        ///</summary>
        IRespond<TRequest, TResponse> WithDeserialize(Func<byte[], TRequest> deserialize);
        ///<summary>
        /// If applied, uses a new channel for each Response (publish with reply_to tag)
        ///</summary>
        IRespond<TRequest, TResponse> WithUniqueChannel(bool useUniqueChannel = true);
    }
}