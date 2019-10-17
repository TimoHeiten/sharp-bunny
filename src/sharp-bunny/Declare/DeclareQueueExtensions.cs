using System;
using SharpBunny.Declare;
using SharpBunny.Exceptions;

namespace SharpBunny
{
    public static class DeclareQueueExtensions
    {
        #region Queue
        public static IDeclare Queue(this IDeclare declare, string name)
        {
            DeclareBase @base = QueueCheck<DeclareBase>(declare, name, "queue");
            return new DeclareQueue(@base.Bunny, name);
        }

        public static IDeclare AutoDelete(this IDeclare declare, bool delete=false)
        {
            DeclareQueue queue = QueueCheck<DeclareQueue>(declare, () => false);
            queue.AutoDelete = delete;
            return queue;
        }

        public static IDeclare BindAs(this IDeclare declare, string exchangeName, string routingKey)
        {
            if (exchangeName == null)
            {
                throw DeclarationException.Argument(new ArgumentException("exchangeName must not be null for bindings"));
            }
            var queue = QueueCheck<DeclareQueue>(declare, routingKey, "binding-key");
            queue.BindingKey = (exchangeName, routingKey);
            return queue;
        }

        public static IDeclare AsDurable(this IDeclare declare, bool durable = false)
        {
            bool isError = false;
            var queue = QueueCheck<DeclareQueue>(declare, () => isError);

            queue.Durable = durable;
            return queue;
        }

        public static IDeclare WithTTL(this IDeclare declare, int ttl)
        {
            QueueCheck<DeclareQueue>(declare, () => ttl < 0);
            var queue = declare as DeclareQueue;

            queue.Ttl = ttl;

            return queue;
        }

        public static IDeclare MaxLength(this IDeclare declare, int maxLength)
        {
            QueueCheck<DeclareQueue>(declare, () => maxLength <= 0);
            var queue = declare as DeclareQueue;

            queue.MaxLength = maxLength;

            return queue;
        }

        public static IDeclare MaxBytes(this IDeclare declare, int maxBytes)
        {
            QueueCheck<DeclareQueue>(declare, () => maxBytes <= 0);
            var queue = declare as DeclareQueue;

            queue.MaxBytes = maxBytes;

            return queue;
        }

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

        private static T QueueCheck<T>(IDeclare declare, Func<bool> isErrorCondition, string error = "")
        {
            CheckBaseOrThrow<T>(declare);
            if (isErrorCondition())
            {
                var arg = new ArgumentException(error);
                throw DeclarationException.Argument(arg);
            }
            return (T)declare;
        }
        #endregion

        #region Checks
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