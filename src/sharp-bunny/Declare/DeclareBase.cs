using System.Threading.Tasks;
using SharpBunny.Exceptions;

namespace SharpBunny.Declare
{
    ///<summary>
    ///entry for declaration builder
    ///</summary>
    public class DeclareBase : IDeclare
    {
        internal IBunny Bunny { get; set; }
        public Task DeclareAsync()
        {
            throw DeclarationException.BaseNotValid();
        }
    }
}