using System.Threading.Tasks;
using BunnyTests;
using SharpBunny;
using Xunit;

namespace tests.RpcTests
{
    public class RequestTests
    {
        [Fact]
        public async Task DirectReplyWorks()
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
            string rpcExchange = "rpc-exchange";

            await bunny.Respond<MyRequest, MyResponse>(rpcExchange, rq => 
            {
                return Task.FromResult(new MyResponse());
            })
            .StartRespondingAsync();

            OperationResult<MyResponse> result = await bunny.Request<MyRequest, MyResponse>(rpcExchange)
            .RequestAsync(new MyRequest(), force: true);

            await Task.Delay(500);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Message);
        }

        private class MyRequest { }
        private class MyResponse { }

        [Fact]
        public async Task WithTemporaryQueueWorksAlso()
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
            string rpcExchange = "rpc-exchange";

            await bunny.Respond<MyRequest, MyResponse>(rpcExchange, rq => 
            {
                return Task.FromResult(new MyResponse());
            })
            .StartRespondingAsync();

            OperationResult<MyResponse> result = await bunny.Request<MyRequest, MyResponse>(rpcExchange)
            .RequestAsync(new MyRequest(), force: true);

            await Task.Delay(500);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Message);
        }
    }
}