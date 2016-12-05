using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginBase
{
    public class RemotePluginLoader : RemoteTypeLoader, IPlugin
    {
        protected Type pluginType;
        public IPlugin Plugin { get; private set; }
        
        protected override System.Reflection.Assembly LoadAssembly(string assemblyPath)
        {
            var ass =  base.LoadAssembly(assemblyPath);

            //查找插件
            this.pluginType = ass.GetTypes().Where(t => t.GetInterface("PluginBase.IPlugin") != null).FirstOrDefault();
            this.Plugin = (IPlugin)System.Activator.CreateInstance(pluginType);
            return ass;
        }

        public void Execute(Action action)
        {
            action();
        }

        public T Execute<T>(Func<T> fun)
        {
            return fun();
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
            return Plugin.RunWithRemoteAction(args, action);
        }
    }
}
