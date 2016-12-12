using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZilLion.Core.TaskManager;
using ZilLion.Core.TaskManager.Config;
using ZilLion.Core.TaskManager.Respository;

namespace TaskTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var test =
                @"data source=123.57.226.114;initial catalog=作业调度配置20161212;user id=sa;password=84519741;pooling=true;min pool size=1;max pool size=1;Connection Timeout=0";

            TaskRunner.InitTaskRunner(test);
           var instance=  TaskRunner.Getinstance();

            instance.StartAllScheduler();
            Console.ReadKey();
        }
    }
}
