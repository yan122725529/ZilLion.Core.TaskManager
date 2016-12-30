using System;
using System.Collections.Concurrent;
using System.Configuration;
using ZilLion.Core.TaskManager;
using ZilLion.Core.Unities;

namespace TaskTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AppSettingContainer.InitAppSetting(() => new ConcurrentDictionary<string, AppSettingData>(), (s, data) => { });

            TaskRunner.InitTaskRunner(ConfigurationManager.AppSettings["TaskConfigDbConString"],
                ConfigurationManager.AppSettings["TaskServerId"]);
            TaskRunner.Getinstance().StartAllScheduler();
            Console.ReadKey();
        }
    }
}