using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginBase
{
    public class PluginLoader : TypeLoader, IPlugin
    {
        public  IPlugin RemotePlugin{get;private set;}
        public PluginLoader(string targetAssembly)
        {
            this.remoteTypeLoader = CreateRemoteTypeLoader(typeof(RemotePluginLoader));
            this.remoteTypeLoader.InitTypeLoader(targetAssembly);

            this.RemotePlugin = (RemotePluginLoader)remoteTypeLoader;
        }

        public Guid PluginId
        {
            get { return RemotePlugin.PluginId; }
        }

        public string Run(string args)
        {
            return this.RemotePlugin.Run(args);
        }


        public string Run(string args, Action action)
        {
            return this.RemotePlugin.Run(args, action.CreateRemoteAppDomainProxy());
        }

        public string RunWithRemoteAction(string args, Action action)
        {
            return this.RemotePlugin.RunWithRemoteAction(args, action);
        }
    }
}
