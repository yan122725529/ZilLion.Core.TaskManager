using System;

namespace PluginBase
{
    [Serializable]
    public class RemoteActionProxy : MarshalByRefObject
    {
        private readonly Action _action;

        private RemoteActionProxy()

        {
        }


        private RemoteActionProxy(Action action)

        {
            _action = action;
        }


        public void Execute()

        {
            _action();
        }


        public static Action CreateProxyAction(Action action)

        {
            var proxy = new RemoteActionProxy(action);
            return proxy.Execute;
        }
    }
    [Serializable]
    public class RemoteFuncProxy<T> : MarshalByRefObject
    {
        private readonly Func<T> _func;

        private RemoteFuncProxy()

        {
        }


        private RemoteFuncProxy(Func<T> func)

        {
            _func = func;
        }


        public T Execute()

        {
            return _func();
        }


        public static Func<T> CreateProxyFunc(Func<T> func)

        {
            var proxy = new RemoteFuncProxy<T>(func);

            return proxy.Execute;
        }
    }


    public static class RemoteDomainHelper

    {
        public static Action CreateRemoteAppDomainProxy(this Action action)

        {
            return RemoteActionProxy.CreateProxyAction(action);
        }

        public static Func<T> CreateRemoteAppDomainProxy<T>(this Func<T> func)

        {
            return RemoteFuncProxy<T>.CreateProxyFunc(func);
        }
    }
}