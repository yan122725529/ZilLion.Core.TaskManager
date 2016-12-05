using System;
using System.IO;

namespace ZilLion.Core.TaskManager.AppDomainTypeLoder
{
    public class TypeLoader 
    {
        public RemoteTypeLoader RemoteTypeLoader;
        public System.AppDomain RemoteDomain { get; private set; }

        public RemoteTypeLoader CreateRemoteTypeLoader(Type remoteLoaderType, string targetDomainName = null)
        {
            var setup = new AppDomainSetup
            {
                ApplicationName = "JobLoader",
                ApplicationBase = System.AppDomain.CurrentDomain.BaseDirectory,
                PrivateBinPath = "Jobs",
                CachePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "JobCachePath")
            };
            ;
            setup.ShadowCopyFiles = "true";
            setup.ShadowCopyDirectories = string.Concat(setup.ApplicationBase, ";", setup.PrivateBinPath);
            this.RemoteDomain = System.AppDomain.CreateDomain(targetDomainName ?? string.Concat("JobLoaderDomain_", Guid.NewGuid().ToString()), null, setup);

            Type typeName = remoteLoaderType;

            RemoteTypeLoader ret = (RemoteTypeLoader)RemoteDomain.CreateInstanceAndUnwrap(typeName.Assembly.FullName, typeName.FullName);

            return ret;
        }
        protected TypeLoader()
        { }

        public TypeLoader(string targetAssembly)
        {
            this.RemoteTypeLoader = CreateRemoteTypeLoader(typeof(RemoteTypeLoader));
            this.RemoteTypeLoader.InitTypeLoader(targetAssembly);
        }

        public void Unload()
        {
            System.AppDomain.Unload(this.RemoteDomain);
        }
    }
}
