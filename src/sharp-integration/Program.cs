using System;
using SharpBunny;

namespace sharp_integration
{
    class Program
    {
        static void Main(string[] args)
        {
            var pipe = Bunny.ConnectSingleWith();
            IBunny bunny = pipe.Connect();

            Console.ReadLine();
        }
    }
}
