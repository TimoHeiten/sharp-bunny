using System;
using System.Threading.Tasks;

namespace SharpBunny
{
    public interface IRequest<TRequest, TResponse> : IDisposable
        where TRequest : class
        where TResponse : class
    {
        IRequest<TRequest, TResponse> WithQueueDeclare(IQueue queue);
        IRequest<TRequest, TResponse> UseUniqueChannel(bool useUnique = true);
        IRequest<TRequest, TResponse> WithTemporaryQueue(bool useTempQueue = true);
        IRequest<TRequest, TResponse> SerializeRequest(Func<TRequest, byte[]> serialize);
        Task<OperationResult<TResponse>> RequestAsync(TRequest request, bool force = false);
        IRequest<TRequest, TResponse> DeserializeResponse(Func<byte[], TResponse> deserialize);
        IRequest<TRequest, TResponse> WithQueueDeclare(string queue=null, string exchange=null, string routingKey=null);

        IRequest<TRequest, TResponse> WithTimeOut(uint timeOut);
    }
}