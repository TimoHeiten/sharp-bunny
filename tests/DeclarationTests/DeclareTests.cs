using System;
using Xunit;
using SharpBunny;
using System.Linq;
using SharpBunny.Declare;
using SharpBunny.Exceptions;
using System.Threading.Tasks;

namespace BunnyTests
{
    public class DeclareTests
    {
        [Fact]
        public void QueueCallThrowsIfNotDeclareBase()
        {
            var fail = new PurposeIsFail();
            Assert.Throws<DeclarationException>(() => fail.Queue("name"));
        }
        private class PurposeIsFail : IDeclare 
        {
            public Task DeclareAsync()
            {
                throw new System.NotImplementedException();
            }
        }

        [Fact]
        public void QueueCallReturnsTypeQueueDeclareIfIsBase()
        {
            var @base = new DeclareBase();
            var queue = @base.Queue("my-queue");
            Assert.Equal(typeof(DeclareQueue), queue.GetType());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("255")]
        public void ThrowsOnNameNullOrToLong(string name)
        {
            if (name == "255")
            {
                name = name.PadRight(500, '-');
            }
            var @base = new DeclareBase();
            Assert.Throws<DeclarationException>(() => @base.Queue(name));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("255")]
        public void CheckBindingDeclare(string name)
        {
            if (name == "255")
            {
                name = name.PadRight(500, '-');
            }
            var @base = new DeclareQueue(ConnectSimple.Connect(), "queue");
            Assert.Throws<DeclarationException>(() => @base.BindAs(name, ""));
        }

        [Fact]
        public void BindAsSetsBindingKeyOn()
        {
            var @base = new DeclareQueue(ConnectSimple.Connect(), "queue");
            @base.BindAs("ex", "bind-key");
            Assert.Equal("bind-key", @base.BindingKey.HasValue ? @base.BindingKey.Value.rKey : "null");
        }

        [Fact]
        public async Task DeclareAndBindDefaultAmqDirectSucceeds()
        {
            IBunny bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
            var declare = bunny.Declare().Queue("bind-test")
                           .BindAs("amq.direct", "bind-test-key")
                           .AsDurable()
                           .WithTTL(500)
                           .MaxLength(10);

            await declare.DeclareAsync();

            Assert.Equal(1, 1);

        }
    }
}