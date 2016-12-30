using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caliburn.Micro
{
    /// <summary>
    ///     Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public class EventAggregator : IEventAggregator
    {
        /// <summary>
        ///     Processing of handler results on publication thread.
        /// </summary>
        public static Action<object, object> HandlerResultProcessing = (target, result) => { };

        private readonly List<Handler> handlers = new List<Handler>();

        /// <summary>
        ///     Searches the subscribed handlers to check if we have a handler for
        ///     the message type supplied.
        /// </summary>
        /// <param name="messageType">The message type to check with</param>
        /// <returns>True if any handler is found, false if not.</returns>
        public bool HandlerExistsFor(Type messageType)
        {
            return handlers.Any(handler => handler.Handles(messageType) & !handler.IsDead);
        }

        /// <summary>
        ///     Subscribes an instance to all events declared through implementations of <see cref="IHandle{T}" />
        /// </summary>
        /// <param name="subscriber">The instance to subscribe for event publication.</param>
        public virtual void Subscribe(object subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }
            lock (handlers)
            {
                if (handlers.Any(x => x.Matches(subscriber)))
                {
                    return;
                }

                handlers.Add(new Handler(subscriber));
            }
        }

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="subscriber">The instance to unsubscribe.</param>
        public virtual void Unsubscribe(object subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }
            lock (handlers)
            {
                var found = handlers.FirstOrDefault(x => x.Matches(subscriber));

                if (found != null)
                {
                    handlers.Remove(found);
                }
            }
        }

        /// <summary>
        ///     Publishes a message.
        /// </summary>
        /// <param name="message">The message instance.</param>
        /// <param name="marshal">Allows the publisher to provide a custom thread marshaller for the message publication.</param>
        public virtual int Publish(object message, Action<Action> marshal)
        {
            int result = 0;//返回订阅者数量
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (marshal == null)
            {
                throw new ArgumentNullException("marshal");
            }

            Handler[] toNotify;
            lock (handlers)
            {
                toNotify = handlers.ToArray();
                result = toNotify.Length;
            }

            marshal(() =>
            {
                var messageType = message.GetType();

                var dead = toNotify
                    .Where(handler => !handler.Handle(messageType, message))
                    .ToList();

                if (dead.Any())
                {
                    lock (handlers)
                    {
                        dead.Apply(x => handlers.Remove(x));
                    }
                }
            });

            return result;
        }

        private class Handler
        {
            private readonly WeakReference reference;
            private readonly Dictionary<Type, MethodInfo> supportedHandlers = new Dictionary<Type, MethodInfo>();

            public Handler(object handler)
            {
                reference = new WeakReference(handler);

                var interfaces = handler.GetType().GetInterfaces()
                    .Where(x => typeof (IHandle).IsAssignableFrom(x) && x.IsGenericType());

                foreach (var @interface in interfaces)
                {
                    var type = @interface.GetGenericArguments()[0];
                    var method = @interface.GetMethod("Handle", new[] {type});

                    if (method != null)
                    {
                        supportedHandlers[type] = method;
                    }
                }
            }

            public bool IsDead
            {
                get { return reference.Target == null; }
            }

            public bool Matches(object instance)
            {
                return reference.Target == instance;
            }

            public bool Handle(Type messageType, object message)
            {
                var target = reference.Target;
                if (target == null)
                {
                    return false;
                }

                foreach (var pair in supportedHandlers)
                {
                    if (pair.Key.IsAssignableFrom(messageType))
                    {
                        var result = pair.Value.Invoke(target, new[] {message});
                        if (result != null)
                        {
                            HandlerResultProcessing(target, result);
                        }
                    }
                }

                return true;
            }

            public bool Handles(Type messageType)
            {
                return supportedHandlers.Any(pair => pair.Key.IsAssignableFrom(messageType));
            }
        }
    }

    #region zillion 自定义事件总线

    /// <summary>
    ///     用于跨模块调用事件总线info
    /// </summary>
    public class CrossModuleEventArgs
    {
        /// <summary>
        ///     事件Key值
        /// </summary>
        public string EventKey { get; set; }

        /// <summary>
        ///     事件数据上下文
        /// </summary>
        public object EventContext { get; set; }
    }

    /// <summary>
    ///     事件监视器
    /// </summary>
    public interface IModuleEventMonitor
    {
        /// <summary>
        ///     获取程序集下所有事件处理策略名称
        /// </summary>
        /// <returns></returns>
        IList<string> AllEventStrategy { get; }

        /// <summary>
        ///     处理策略实例缓存（避免重复创建实例性能消耗）
        /// </summary>
        Dictionary<string, WeakReference> EventStrategyCache { get; set; }

        /// <summary>
        ///     获取策略实例
        /// </summary>
        /// <param name="eventKey"></param>
        /// <returns></returns>
        IModuleEventHandleStrategy GetStrategyInstance(string eventKey);


        Assembly CurrentAssembly { get; }
    }

    /// <summary>
    /// </summary>
    public interface IModuleEventHandleStrategy
    {
        /// <summary>
        ///     执行事件处理
        /// </summary>
        /// <param name="context"></param>
        void HandleEvent(object context);
    }

    #endregion
}