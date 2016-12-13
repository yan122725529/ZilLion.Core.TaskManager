using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZilLion.Core.TaskManager;
using ZilLion.Core.TaskManager.Config;
using ZilLion.Core.TaskManager.Respository;
using ZilLion.Core.TaskManager.Unities;

namespace TaskTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var test =
                @"data source=123.57.226.114;initial catalog=作业调度配置20161212;user id=sa;password=84519741;pooling=true;min pool size=1;max pool size=1;Connection Timeout=0";


            var serverid = Guid.NewGuid().ToString();
            TaskRunner.InitTaskRunner(test, serverid);

            var taskConfigRespository = new TaskConfigRespository();
            var taskRunLogRespository = new TaskRunLogRespository();

            for (var i = 0; i < 20; i++)
            {
                taskConfigRespository.SaveData(new TaskConfig() { });
                var runlog = new TaskRunLog
                {
                    Pcip = Computer.IpAddress,
                    Pcmac = Computer.MacAddress,
                    Pcname = Computer.ComputerName,
                    Taskid = Guid.NewGuid().ToString(),
                    Taskname = Guid.NewGuid().ToString(),
                    Taskremark = Guid.NewGuid().ToString(),
                    Taskserverid = TaskManagerConfig.TaskServerId,
                    Taskstatus = 0,Tasknextruntime = DateTime.Now,Tasklastruntime = DateTime.Now
                };


                taskRunLogRespository.AddTaskRunLog(runlog);
                taskRunLogRespository.ModifyTaskRunLog(runlog);
            }
         var list=   taskConfigRespository.GetjobConfigs();
            var logs = taskRunLogRespository.GetAllTaskRunLog();
            //var instance=  TaskRunner.Getinstance();

            //    instance.StartAllScheduler();
            //    Console.ReadKey();
        }
    }
}
