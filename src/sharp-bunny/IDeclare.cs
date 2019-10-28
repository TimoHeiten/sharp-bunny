using System.Threading.Tasks;

namespace SharpBunny
{
    ///<summary>
    /// Entry point to setup RabbitMQ Entities.
    ///</summary>
    public interface IDeclare
    {
        ///<summary>
        /// Execute the Declaration
        ///</summary>
        Task DeclareAsync();
        IBunny Bunny { get; }
    }
}