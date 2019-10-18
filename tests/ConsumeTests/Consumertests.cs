using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BunnyTests;
using Newtonsoft.Json;
using SharpBunny;
using Xunit;

namespace tests
{
    public class Consumertests
    {
        const string get = "get-queue";
        const string queue = "consume-queue";
        const string nackQueue = "nack-no-requeue";
        const string nackReQueue = "nack-requeue";

        [Fact]
        public void ConsumerAttachReturnsOperationResult()
        {
            ConsumeGeneric(async carrot => 
            {
                var result = carrot.Message;
                var opResult = await carrot.SendAckAsync();
                Assert.True(opResult.IsSuccess);

                Assert.NotNull(result);
                Assert.Equal(nameof(ConsumeMessage), result.MyText);
            });
        }

        private IBunny ConsumeGeneric(Func<ICarrot<ConsumeMessage>, Task> carrot, string toQueue = queue)
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);

            var consumer = bunny.Consumer<ConsumeMessage>(toQueue).Callback(carrot);
            var operationResult = consumer.StartConsuming();

            Assert.True(operationResult.IsSuccess);
            Assert.Equal(OperationState.ConsumerAttached, operationResult.State);

            SetupAndPublish(bunny, toQueue);
            return bunny;
        }

        private void SetupAndPublish(IBunny bunny, string queueName = queue)
        {
            var msg = new ConsumeMessage();
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));

            var channel = bunny.Channel(newOne:true);
            var prop = channel.CreateBasicProperties();
            prop.Persistent = true;

            channel.BasicPublish("", queueName, false, prop, bytes);
        }

        [Fact]
        public void MultipleCallToConsumeAlwaysReturnUccess()
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);

            ConsumeMessage result = null;
            var consumer = bunny.Consumer<ConsumeMessage>(queue).Callback(async carrot => 
            {
                result = carrot.Message;
                var opResult = await carrot.SendAckAsync();
                Assert.True(opResult.IsSuccess);
                Assert.NotNull(result);
                Assert.Equal(nameof(ConsumeMessage), result.MyText);
            });
            var result1 = consumer.StartConsuming();
            var result2 = consumer.StartConsuming();
            var result3 = consumer.StartConsuming();

            Assert.Equal(result1, result2, new EqualityOpResult());
            Assert.Equal(result1, result3, new EqualityOpResult());
            Assert.Equal(result2, result2, new EqualityOpResult());
        }

        private class EqualityOpResult : IEqualityComparer<OperationResult<ConsumeMessage>>
        {
            public bool Equals(OperationResult<ConsumeMessage> x, OperationResult<ConsumeMessage> y)
            {
                return x.State == y.State && x.IsSuccess == y.IsSuccess;
            }

            public int GetHashCode(OperationResult<ConsumeMessage> obj)
            {
                // does not matter
                return 42;
            }
        }

        [Fact]
        public void NackRequeues()
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);

            var consumer = bunny.Consumer<ConsumeMessage>(queue).Callback(async carrot => 
            {
                var result = await carrot.SendNackAsync(withRequeue:true);
                Assert.Equal(OperationState.Nacked, result.State);
                uint count = bunny.Channel().MessageCount(nackReQueue);
                System.Console.WriteLine($"messages:{count}");
                Assert.Equal((uint)1, count);
            });
            SetupAndPublish(bunny, nackReQueue);
        }

        [Fact]
        public void NackNoRequeue()
        {
            var bunny = ConsumeGeneric(async carrot => 
            {
                var result = await carrot.SendNackAsync(false);
                Assert.Equal(OperationState.Nacked, result.State);
            }, nackQueue);

            uint count = bunny.Channel().MessageCount(nackQueue);
            Assert.Equal((uint)0, count);
        }

        [Fact]
        public async Task GetReturnsOperationResultFailIfNoMessages()
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
            var opResult = await bunny.Consumer<ConsumeMessage>(get).AsAutoAck().GetAsync(carrot => Task.CompletedTask);
            
            Assert.NotNull(opResult);
            Assert.Equal(OperationState.GetFailed, opResult.State);
        }

        [Fact]
        public async Task GetSucceedsIfMessageIsAvailable()
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
            var msg = new ConsumeMessage();
            var bytes = Config.Serialize(msg);
            bunny.Channel(newOne:true).BasicPublish("", get, false, null, bytes);
            var opResult = await bunny.Consumer<ConsumeMessage>(get).AsAutoAck().GetAsync(carrot => Task.CompletedTask);
            
            Assert.NotNull(opResult);
            Assert.Equal(OperationState.Get, opResult.State);
            Assert.NotNull(opResult.Message.MyText);
        }

        // get
        // simulate exceptions‚

        public class ConsumeMessage
        {
            public string MyText { get; set; } = nameof(ConsumeMessage);
        }
    }
}