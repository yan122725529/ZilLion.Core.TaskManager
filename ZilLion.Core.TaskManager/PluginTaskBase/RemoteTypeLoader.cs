using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PluginBase
{
    public class RemoteTypeLoader : MarshalByRefObject
    {
        private string _assemblyPath;
        private Assembly _assembly;

        public RemoteTypeLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if(Path.IsPathRooted(args.Name))
                return null;

            return Assembly.LoadFrom(Path.Combine("plugins", args.Name));
        }

        protected virtual Assembly LoadAssembly(string assemblyPath)
        {
            return Assembly.Load(assemblyPath);
        }

        public object CreateInstance(string fullTypeName)
        {
            if (_assembly == null)
                return null;

            return _assembly.CreateInstance(fullTypeName,false);
        }

        public void InitTypeLoader(string assemblyPath)
        {
            this._assemblyPath = assemblyPath;
            this._assembly = this.LoadAssembly(this._assemblyPath);
        }

      
    }


}
