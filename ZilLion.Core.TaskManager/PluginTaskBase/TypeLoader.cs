using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PluginBase
{
    public class TypeLoader 
    {
        protected RemoteTypeLoader remoteTypeLoader;
        public AppDomain RemoteDomain { get; private set; }

        public RemoteTypeLoader CreateRemoteTypeLoader(Type remoteLoaderType, string targetDomainName = null)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationName = "AppLoader";
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            setup.PrivateBinPath = "plugins";
            setup.CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CachePath");;
            setup.ShadowCopyFiles = "true";
            setup.ShadowCopyDirectories = string.Concat(setup.ApplicationBase, ";", setup.PrivateBinPath);

            this.RemoteDomain = AppDomain.CreateDomain(targetDomainName ?? string.Concat("AppLoaderDomain_", Guid.NewGuid().ToString()), null, setup);

            Type typeName = remoteLoaderType;

            RemoteTypeLoader ret = (RemoteTypeLoader)RemoteDomain.CreateInstanceAndUnwrap(typeName.Assembly.FullName, typeName.FullName);

            return ret;
        }
        protected TypeLoader()
        { }

        public TypeLoader(string targetAssembly)
        {
            this.remoteTypeLoader = CreateRemoteTypeLoader(typeof(RemoteTypeLoader));
            this.remoteTypeLoader.InitTypeLoader(targetAssembly);
        }

        public void Unload()
        {
            AppDomain.Unload(this.RemoteDomain);
        }
    }
}
