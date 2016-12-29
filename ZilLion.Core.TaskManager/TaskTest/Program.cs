using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppDomainToolkit;
using ZilLion.Core.TaskManager;
using ZilLion.Core.TaskManager.Config;
using ZilLion.Core.TaskManager.Respository;
using ZilLion.Core.TaskManager.Unities;
using ZilLion.Core.TaskManager.Unities.File;

namespace TaskTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(AppDomain.CurrentDomain.FriendlyName);


            //for (int i = 0; i < 10; i++)
            //{
            var setup = new AppDomainSetup
            {
                ApplicationName = "JobLoader",
                ApplicationBase = FileHelper.GetRootPath(),
                PrivateBinPath = "ZilLionTask",
                CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JobCachePath"),
                ShadowCopyFiles = "true"
            };
            setup.ShadowCopyDirectories = string.Concat(setup.ApplicationBase, ";", setup.PrivateBinPath);
            setup.ApplicationName = string.Concat("JobLoaderDomain_", Guid.NewGuid().ToString());
            var remoteDomain = AppDomain.CreateDomain(string.Concat("JobLoaderDomain_", Guid.NewGuid().ToString()),
                null, setup);
            var context = AppDomainContext.Wrap(remoteDomain);

            RemoteAction.Invoke<string>(context.Domain, "", (s) => { Console.WriteLine(s + "*****remote****" + AppDomain.CurrentDomain.FriendlyName); });

            //}




            //Console.ReadLine();
            //return;
            var test =
                @"data source=123.57.226.114;initial catalog=作业调度配置20161212;user id=sa;password=84519741;pooling=true;min pool size=1;max pool size=1;Connection Timeout=0";


            var serverid = Guid.NewGuid().ToString();
            TaskRunner.InitTaskRunner(test, serverid);

            //var taskConfigRespository = new TaskConfigRespository();
            //var taskRunLogRespository = new TaskRunLogRespository();

        
            var instance = TaskRunner.Getinstance();

            instance.StartAllScheduler();
            Console.ReadKey();
        }
    }
}
