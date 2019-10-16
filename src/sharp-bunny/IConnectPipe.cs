using System;
using System.Linq;
using System.Threading.Tasks;

namespace SharpBunny
{
    //  IBunny bunny = Bunny.Connect(uri / Parameters / Fluent);
    //                 || Bunny.Connect().ToHost().ToPort().ToVirtualHost()
    //                    .WithPlain(guest, guest)
    ///<summary>
    /// Use to configure the Connection parameters in a fluent manner
    ///</summary>
    public interface IConnectPipe
    {
        IConnectPipe ToHost(string hostName = "localhost");
        IConnectPipe ToPort(uint port = 5672);
        IConnectPipe ToVirtualHost(string vHost = "/");
        IConnectPipe AuthenticatePlain(string user, string password);

        IBunny Connect();
    }
}