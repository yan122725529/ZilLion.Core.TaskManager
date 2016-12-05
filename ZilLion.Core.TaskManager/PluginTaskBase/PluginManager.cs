using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using PluginBase;

namespace PluginTaskBase
{
    public class PluginManager
    {
        private static readonly PluginManager Instance;

        private readonly ConcurrentDictionary<string, FileSystemEventArgs> _changedPlugins =
            new ConcurrentDictionary<string, FileSystemEventArgs>();

        private readonly ConcurrentDictionary<string, PluginCallerProxy> _plugins =
            new ConcurrentDictionary<string, PluginCallerProxy>();

        private readonly Timer _timerProcess;


        static PluginManager()

        {
            if (Instance != null) return;
            lock (typeof(PluginManager))

            {
                if (Instance == null)

                    Instance = new PluginManager();
            }
        }


        private PluginManager()

        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
            //监控
            var pluginWatcher = new FileSystemWatcher(path, "*.dll")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };


            pluginWatcher.Changed += OnPluginChanged;
            pluginWatcher.Created += OnPluginChanged;
            pluginWatcher.Deleted += OnPluginChanged;
            pluginWatcher.Renamed += OnPluginRenamed;
            pluginWatcher.EnableRaisingEvents = true;
            _timerProcess = new Timer(e => ProcessChangedPlugin());


            //加载所有
            Directory.GetFiles(path, "*.dll").ToList().ForEach(file =>
            {
                var fi = new FileInfo(file);
                _changedPlugins[fi.Name] = new FileSystemEventArgs(WatcherChangeTypes.Created, fi.DirectoryName, fi.Name);
            });
            _timerProcess.Change(10, -1);
        }


        private void OnPluginRenamed(object sender, RenamedEventArgs e)

        {
            //重命名，理解为去掉原来的，增加新命名的
            var old = new FileInfo(e.OldFullPath);
            _changedPlugins[old.Name] = new FileSystemEventArgs(WatcherChangeTypes.Deleted, old.DirectoryName, old.Name);
            var n = new FileInfo(e.FullPath);
            _changedPlugins[n.Name] = new FileSystemEventArgs(WatcherChangeTypes.Created, n.DirectoryName, n.Name);
            //1秒后再处理
            _timerProcess.Change(1000, -1);
        }


        private void OnPluginChanged(object sender, FileSystemEventArgs e)

        {
            Debug.Print(e.Name + e.ChangeType);
            //记录变更
            _changedPlugins[e.Name] = e;
            //1秒后再处理
            _timerProcess.Change(1000, -1);
        }


        protected void ProcessChangedPlugin()

        {
            foreach (var kv in _changedPlugins)
            {
                FileSystemEventArgs e;
                if (_changedPlugins.TryRemove(kv.Key, out e))
                {
                    Debug.Print(e.Name + "=>" + e.ChangeType);
                    switch (e.ChangeType)
                    {
                        case WatcherChangeTypes.Created:

                        {
                            //加载
                            var loader = new PluginLoader(e.Name);
                            var proxy = new PluginCallerProxy(loader);
                            _plugins.TryAdd(e.Name, proxy);
                            OnPluginChanged(this, e);
                        }

                            break;
                        case WatcherChangeTypes.Deleted:

                        {
                            PluginCallerProxy proxy;
                            if (_plugins.TryRemove(e.Name, out proxy))
                            {
                                OnPluginChanged(this, e);
                                var loader = proxy.PluginLoader;
                                proxy.PluginLoader = null;
                                loader.Unload();
                            }
                        }

                            break;
                        case WatcherChangeTypes.Changed:

                        {
                            PluginCallerProxy proxy;
                            if (_plugins.TryGetValue(e.Name, out proxy))
                            {
                                OnPluginChanged(this, e);
                                var loader = proxy.PluginLoader;
                                loader.Unload();
                                loader = new PluginLoader(e.Name);
                                proxy.PluginLoader = loader;
                                OnPluginChanged(this, e);
                            }
                        }

                            break;
                    }
                }
            }
        }
    }
}