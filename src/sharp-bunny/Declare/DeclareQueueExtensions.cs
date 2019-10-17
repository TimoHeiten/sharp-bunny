using System;
using SharpBunny.Declare;
using SharpBunny.Exceptions;

namespace SharpBunny
{
    public static class DeclareQueueExtensions
    {
        public static IQueue Queue(this IDeclare declare, string name)
        {
            DeclareBase @base = QueueCheck<DeclareBase>(declare, name, "queue");
            return new DeclareQueue(@base.Bunny, name);
        }


        #region Checks
        private static T QueueCheck<T>(IDeclare declare, string toCheck, string errorPrefix)
            where T : IDeclare
        {
            CheckBaseOrThrow<T>(declare);
            if (string.IsNullOrWhiteSpace(toCheck))
            {
                var arg = new ArgumentException($"{errorPrefix}-name must not be null-or-whitespace");
                throw DeclarationException.Argument(arg);
            }
            if (toCheck.Length > 255)
            {
                var arg = new ArgumentException($"{errorPrefix}-length must be less than or equal to 255 character");
                throw DeclarationException.Argument(arg);
            }
            return (T)declare;
        }
        internal static T CheckBaseOrThrow<T>(this IDeclare declare)
        {
            bool isBase = declare is T;
            if (!isBase)
            {
                throw DeclarationException.WrongType(typeof(T), declare);
            }
            return (T)declare;
        }
        #endregion 
    }
}