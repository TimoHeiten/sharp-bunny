using System.Threading.Tasks;
using SharpBunny.Exceptions;

namespace SharpBunny.Declare
{
    ///<summary>
    ///entry for declaration builder
    ///</summary>
    public class DeclareBase : IDeclare
    {
        public IBunny Bunny { get; set; }
        public Task DeclareAsync()
        {
            throw DeclarationException.BaseNotValid();
        }
    }
}