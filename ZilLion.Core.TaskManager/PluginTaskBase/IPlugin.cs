using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginBase
{
    public interface IPlugin
    {
        Guid PluginId { get; }

        string Run(string args);

        string Run(string args, Action action);

        string RunWithRemoteAction(string args, Action action);
    }
}
