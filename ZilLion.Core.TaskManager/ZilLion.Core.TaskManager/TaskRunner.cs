using System;
using System.Collections.Generic;
using ZilLion.Core.TaskManager.Config;

namespace ZilLion.Core.TaskManager
{
    public class TaskRunner
    {
        #region 作业配置

        /// <summary>
        /// 可用作业配置
        /// </summary>
        public IList<Jobconfig> UseableJobconfigs { get; set; }

        private void GetUseableJobconfigs()
        {
            if (UseableJobconfigs==null)
                UseableJobconfigs=new List<Jobconfig>();
            UseableJobconfigs.Clear();
            //todo 从数据库获取
            UseableJobconfigs=new List<Jobconfig>() {new Jobconfig() {} };
        }


        #endregion



        #region 单例

        private static TaskRunner _instance;

        private TaskRunner()
        {
        }

        public static void InitTastRunner(string jobConfigDbConString)
        {
            _instance = new TaskRunner();
            TaskManagerConfig.JobConfigDbConString = jobConfigDbConString;
        }

        public static TaskRunner Getinstance()
        {
            if (_instance == null)
                throw new Exception("TaskRunner尚未初始化，请调用InitTastRunner方法初始化。");

            return _instance;
        }

        #endregion

        #region 刷新配置

        public void RefreshJob(List<string> joblist = null)
        {
            var todolist = new List<Jobconfig>();
            if (joblist == null)
            {
                //刷新全部配置
                GetUseableJobconfigs();

            }
            else
            {

            }


        }
        /// <summary>
        /// 重启job
        /// </summary>
        /// <param name="jobid"></param>
        private void Restartjob(string jobid)
        {

        }


        #endregion




    }
}


//public partial class TaskManagerService : ServiceBase
//{
//    public TaskManagerService()
//    {
//        InitializeComponent();
//    }

//    protected override void OnStart(string[] args)
//    {
//        DebuggableAttribute att = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttribute<DebuggableAttribute>();
//        if (att.IsJITTrackingEnabled)
//        {
//            //Debug模式才让线程停止10s,方便附加到进程调试
//            Thread.Sleep(10000);
//        }
//        //配置信息读取
//        ConfigInit.InitConfig();
//        //3.系统参数配置初始化
//        MefConfig.Init();
//        ConfigManager configManager = MefConfig.TryResolve<ConfigManager>();
//        configManager.Init();

//        QuartzHelper.InitScheduler();
//        QuartzHelper.StartScheduler();

//        // 保持web服务运行  
//        ThreadPool.QueueUserWorkItem((o) =>
//        {
//            //启动站点
//            Startup.Start(SystemConfig.WebPort);
//        });
//    }

//    protected override void OnStop()
//    {
//        QuartzHelper.StopSchedule();
//        //回收资源
//        Startup.Dispose();
//        System.Environment.Exit(0);
//    }
//}