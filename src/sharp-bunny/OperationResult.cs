using System;

namespace SharpBunny
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; internal set; }
        public T Message { get; internal set; }
        public Exception Error { get; set; }
    }
}