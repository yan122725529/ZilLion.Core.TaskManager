﻿using System;
using System.IO;

namespace PluginTaskBase
{
    public class TypeLoader 
    {
        protected RemoteTypeLoader RemoteTypeLoader;
        public AppDomain RemoteDomain { get; private set; }

        public RemoteTypeLoader CreateRemoteTypeLoader(Type remoteLoaderType, string targetDomainName = null)
        {
            var setup = new AppDomainSetup
            {
                ApplicationName = "JobLoader",
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                PrivateBinPath = "Jobs",
                CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JobCachePath")
            };
            ;
            setup.ShadowCopyFiles = "true";
            setup.ShadowCopyDirectories = string.Concat(setup.ApplicationBase, ";", setup.PrivateBinPath);
            this.RemoteDomain = AppDomain.CreateDomain(targetDomainName ?? string.Concat("JobLoaderDomain_", Guid.NewGuid().ToString()), null, setup);
            var typeName = remoteLoaderType;

            var ret = (RemoteTypeLoader)RemoteDomain.CreateInstanceAndUnwrap(typeName.Assembly.FullName, typeName.FullName);

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
            AppDomain.Unload(this.RemoteDomain);
        }
    }
}
