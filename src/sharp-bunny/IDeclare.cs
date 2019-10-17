using System.Threading.Tasks;

namespace SharpBunny
{
    public interface IDeclare
    {
        ///<summary>
        /// Execute the Declaration
        ///</summary>
        Task DeclareAsync();
        IBunny Bunny { get; }
    }
}