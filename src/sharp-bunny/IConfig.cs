using System;
using System.Linq;

namespace SharpBunny
{
    ///<summary>
    /// Use to overwrite config during use
    ///</summary>
    public interface IConfig
    {
        IConfig SetLogging(Action log);
        IConfig SetSerialization(Func<object, byte[]> serialize);
    }
}