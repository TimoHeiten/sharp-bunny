using System;
using System.Threading.Tasks;
using BunnyTests;
using RabbitMQ.Client.Events;
using SharpBunny;
using Xunit;

namespace tests
{
    public class PublisherTests
    {
        private string Exchange = ""; // default --> " "

        [Fact]
        public async Task PublisherSimplySendsWithoutQueueReturnsFailure()
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
            IPublish<TestMessage> publisher = bunny.Publisher<TestMessage>(Exchange);

            OperationResult<TestMessage> result = await publisher.SendAsync(new TestMessage());

            Assert.True(result.IsSuccess);
            bunny.Dispose();
        }

        [Fact]
        public async Task PublisherSimplySendsWitQueueReturnsSuccess()
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
            IPublish<TestMessage> publisher = bunny.Publisher<TestMessage>(Exchange);

            OperationResult<TestMessage> result = await publisher
                                                    .WithQueueDeclare()
                                                    .SendAsync(new TestMessage());

            bool success = await bunny.Setup().DeleteQueueAsync(typeof(TestMessage).FullName, force: true);

            Assert.True(result.IsSuccess);
            Assert.True(success);
            bunny.Dispose();
        }

        [Fact]
        public async Task ForceCreatesTheExchangeIfNotExists()
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
            IPublish<TestMessage> publisher = bunny.Publisher<TestMessage>("test-exchange");

            OperationResult<TestMessage> result = await publisher.SendAsync(new TestMessage(), force: true);

            Assert.True(result.IsSuccess);
            bool removed_exchange = await bunny.Setup().DeleteExchangeAsync("test-exchange", force: true);
            Assert.True(removed_exchange);
            bunny.Dispose();
        }

        [Fact]
        public async Task ConfirmsAndAcksWork()
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
            IQueue queue =  bunny.Setup()
                             .Queue("constraint")
                             .MaxLength(1)
                             .Expire(1500)
                             .Bind("amq.direct", "constraint-key")
                             .OverflowReject();

            bool isNacked = false;
            bool isAcked = false;
            var publisher = bunny.Publisher<TestMessage>("amq.direct");
            Func<BasicNackEventArgs, Task> nacker = ea => { isNacked = true; return Task.CompletedTask; };
            Func<BasicAckEventArgs, Task> acker = ea => { isAcked = true; return Task.CompletedTask; };

            await publisher.WithQueueDeclare(queue)
                           .WithConfirm(acker, nacker)
                           .WithRoutingKey("constraint-key")
                           .SendAsync(new TestMessage(){Text = "Confirm-1st"});

            await publisher.WithQueueDeclare(queue)
                           .WithConfirm(acker, nacker)
                           .SendAsync(new TestMessage(){Text = "Confirm-2nd"});

            Assert.True(isAcked);
            Assert.True(isNacked);
            bunny.Dispose();
        }

        [Fact]
        public async Task MandatoryFailsWithoutQueue()
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);

            bool isReturned = false;
            var publisher = bunny.Publisher<object>("amq.direct");
            Func<BasicReturnEventArgs, Task> nacker = ea => { isReturned= true; return Task.CompletedTask; };

            await publisher.AsMandatory(nacker)
                           .WithRoutingKey("not-any-bound-queue")
                           .SendAsync(new object());

            Assert.True(isReturned);
            bunny.Dispose();
        }

        [Fact]
        public async Task MandatoryWorksWithQueue()
        {
             IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);

            bool isReturned = true;
            var publisher = bunny.Publisher<TestMessage>("amq.direct");
            Func<BasicReturnEventArgs, Task> nacker = ea => { isReturned= false; return Task.CompletedTask; };

            await publisher.AsMandatory(nacker)
                           .WithQueueDeclare()
                           .SendAsync(new TestMessage(){Text = "Mandatory-succeeds"});

            bool removed = await bunny.Setup().DeleteQueueAsync(typeof(TestMessage).FullName);

            Assert.True(isReturned);
            bunny.Dispose();
        }

        public class TestMessage 
        { 
            public string Text { get; set; } = "Test";
            public int Number { get; set; } = 42;
        }
    }
}