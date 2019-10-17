namespace SharpBunny
{
    public interface IExchange : IDeclare
    {
        string Name { get; }
        ///<summary>
        /// Exchange defined as autodelete
        ///</summary>
        IExchange AsAutoDelete();
        ///<summary>
        /// Durable Exchanges will survive a broker restart
        ///</summary>
        IExchange AsDurable();
        IExchange Internal();
        ///<summary>
        /// non routeable messages will be sent to this alternate exchange
        ///</summary>
        IExchange AlternateExchange(string alternate);
    }
}