using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using PluginBase;

namespace PluginTaskBase
{
    public class PluginCallerProxy: IPlugin
    {
        private IPlugin _plugin;

        private PluginLoader _pluginLoader;

        private System.Threading.ReaderWriterLockSlim locker = new ReaderWriterLockSlim();



        public PluginLoader PluginLoader

        {

            get

            {

                return _pluginLoader;

            }

            set

            {

                _pluginLoader = value;

                this.Plugin = _pluginLoader == null ? null : _pluginLoader.RemotePlugin;

            }

        }



        internal IPlugin Plugin

        {

            get

            {

                locker.EnterReadLock();

                try

                {

                    if (_plugin == null)

                    {

                        throw new Exception("插件已经卸载");

                    }

                    return _plugin;

                }

                finally

                {

                    locker.ExitReadLock();

                }



            }

            set

            {

                locker.EnterWriteLock();

                try

                {

                    _plugin = value;

                }

                finally

                {

                    locker.ExitWriteLock();

                }

            }

        }





        public PluginCallerProxy(PluginLoader loader)

        {

            this.PluginLoader = loader;

            this.Plugin = loader.RemotePlugin;

        }





        public Guid PluginId

        {

            get { return Plugin.PluginId; }

        }



        public string Run(string args)

        {

            return Plugin.Run(args);

        }



        public string Run(string args, Action action)

        {

            return Plugin.Run(args, action);

        }

        public string RunWithRemoteAction(string args, Action action)
        {
            throw new NotImplementedException();
        }


        public string Run(string args, Func<string> func)

        {

            return Plugin.Run(args, func);

        }



    }





    

        
}