using System;
using Xunit;
using System.Linq;
using SharpBunny;

namespace BunnyTests
{
    public class ConnectSimple
    {
        public ConnectSimple()
        {

        }

        [Fact]
        public void ConnectToSingleNodeViaAmqp()
        {
            IBunny bunny = Bunny.ConnectSingle("amqp://guest:guest@localhost:5672/%2F");

            Assert.NotNull(bunny);
        }

        [Fact]
        public void ConnectToSingleNodeWithConnectionPipe()
        {
            var pipe = Bunny.ConnectSingleWith();
            // not configuring anything uses default
            IBunny bunny = pipe.Connect();

            Assert.NotNull(bunny);
        }

        [Fact]
        public void ConnectMultipleFailsFirstConnectsSecond()
        {
            string node1 = "amqp://guest:guest@localhost:5673/%2F";
            string node2 = "amqp://guest:guest@localhost:5672/%2F";

            var multi = Bunny.ClusterConnect();
            multi.AddNode(node1);
            multi.AddNode(node2);

            IBunny bunny = multi.Connect();

            Assert.NotNull(bunny);
        }
    }
}