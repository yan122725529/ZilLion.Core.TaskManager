using System;
using System.IO;
using System.Reflection;

namespace ZilLion.Core.TaskManager.AppDomainTypeLoder
{
    public class RemoteTypeLoader : MarshalByRefObject
    {
        private string _assemblyPath;
        public Assembly LoadedAssembly { get; set; }

        public RemoteTypeLoader()
        {
            System.AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        /// <summary>
        /// appdomain AssemblyResolve事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Path.IsPathRooted(args.Name) ? null : Assembly.LoadFrom(Path.Combine("ZilLionTask", args.Name));
        }
        /// <summary>
        /// 加载程序集
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        protected virtual Assembly LoadAssembly(string assemblyName)
        {
            return Assembly.Load(assemblyName);
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="fullTypeName"></param>
        /// <returns></returns>
        public object CreateInstance(string fullTypeName)
        {
            if (LoadedAssembly == null)
                return null;

            return LoadedAssembly.CreateInstance(fullTypeName,false);
        }
        /// <summary>
        /// 初始化RemoteTypeLoader
        /// </summary>
        /// <param name="assemblyPath"></param>
        public void InitTypeLoader(string assemblyPath)
        {
            this._assemblyPath = assemblyPath;
            this.LoadedAssembly = this.LoadAssembly(this._assemblyPath);
        }


    }


}
