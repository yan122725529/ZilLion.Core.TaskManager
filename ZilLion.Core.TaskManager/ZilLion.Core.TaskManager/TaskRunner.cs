using System;
using System.Collections.Generic;
using System.IO;
using AppDomainToolkit;
using ZilLion.Core.QuartzWrapper.Config;
using ZilLion.Core.QuartzWrapper.Quartz;
using ZilLion.Core.QuartzWrapper.Respository;
using ZilLion.Core.TaskManager.Respository;
using ZilLion.Core.Unities.UnitiesMethods;

namespace ZilLion.Core.TaskManager
{
    public class TaskRunner
    {
        /// <summary>
        ///     作业执行appdomain
        /// </summary>
        public readonly Dictionary<string, IAppDomainContext> AppDomainContextDic =
            new Dictionary<string, IAppDomainContext>();

        /// <summary>
        ///     可用作业配置
        /// </summary>
        public readonly Dictionary<string, IList<TaskConfig>> ConfigDic = new Dictionary<string, IList<TaskConfig>>();

        #region 初始化

        private static TaskRunnerConfig _runnerConfig;

        private static bool _hasInited;

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="jobConfigDbConString"></param>
        /// <param name="taskServerId"></param>
        public static void InitTaskRunner(string jobConfigDbConString, string taskServerId)
        {
            if (_hasInited)
                throw new Exception("TaskRunner职能初始化一次");
            _instance = new TaskRunner();

            _runnerConfig = new TaskRunnerConfig
            {
                TaskConfigDbConString = jobConfigDbConString,
                TaskServerId = taskServerId
            };
            _taskConfigRespository = new TaskConfigRespository(_runnerConfig);
            _taskRunLogRespository = new TaskRunLogRespository(_runnerConfig);
            //获得task配置库数据库连接
            //设置TaskRunner寄宿的serverid
            _hasInited = true;
            //QuartzHelper.InitScheduler(); //任务调度初始化成功


            //todo LOG(初始化成功)
            //todo While 循环监控数据库变化？
        }

        #endregion

        #region 作业配置

        //public IList<TaskConfig> UseableJobconfigs { get; set; }

        private static ITaskConfigRespository _taskConfigRespository;
        private static ITaskRunLogRespository _taskRunLogRespository;

        private void GetUseableJobconfigs()
        {
            ConfigDic.Clear();
            foreach (var config in _taskConfigRespository.GetjobConfigs())
                if (ConfigDic.ContainsKey(config.TaskModule))
                    ConfigDic[config.TaskModule].Add(config);
                else
                    ConfigDic.Add(config.TaskModule, new List<TaskConfig> {config});
        }

        #endregion

        #region 单例

        private static TaskRunner _instance;

        private TaskRunner()
        {
        }


        public static TaskRunner Getinstance()
        {
            if (_instance == null)
                throw new Exception("TaskRunner尚未初始化，请调用InitTastRunner方法初始化。");
            return _instance;
        }

        #endregion

        #region 任务执行

        /// <summary>
        ///     开始所有执行的任务，并且结合配置执行状态表数据
        /// </summary>
        public void StartAllScheduler()
        {
            GetUseableJobconfigs(); //读取任务配置
            foreach (var configs in ConfigDic.Values)
                foreach (var config in configs)
                {
                    var runlog = new TaskRunLog
                    {
                        Pcip = Computer.IpAddress,
                        Pcmac = Computer.MacAddress,
                        Pcname = Computer.ComputerName,
                        Taskid = config.Taskid,
                        Taskname = config.Taskname,
                        Taskremark = config.TaskRemark,
                        Taskserverid = _runnerConfig.TaskServerId,
                        Taskstatus = 0,
                        Tasknextruntime = DateTime.Now,
                        Tasklastruntime = DateTime.Now
                    };
                    _taskRunLogRespository.SaveTaskRunLog(runlog);
                }

            #region 根据模块创建新的appdomain,并开始任务

            foreach (var config in ConfigDic)
            {
                var setup = new AppDomainSetup
                {
                    ApplicationName = $"JobLoader_{config.Key}",
                    ApplicationBase = FileHelper.GetRootPath(),
                    PrivateBinPath = "ZilLionTask",
                    CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JobCachePath"),
                    ShadowCopyFiles = "true"
                };
                setup.ShadowCopyDirectories = string.Concat(setup.ApplicationBase, ";", setup.PrivateBinPath);

                var remoteDomain = AppDomain.CreateDomain($"{config.Key}Domain_{Guid.NewGuid():N}", null, setup);
                var context = AppDomainContext.Wrap(remoteDomain);
                if (!AppDomainContextDic.ContainsKey(config.Key))
                    AppDomainContextDic.Add(config.Key, context);

                RemoteAction.Invoke(context.Domain, config.Key, config.Value, (k, v) =>
                {
                    var container = SchedulerContainer.GetContainerInstance();
                    container.InitScheduler(k, v, _runnerConfig);
                    container.StartScheduler();
                });
            }

            #endregion
        }

        /// <summary>
        ///     停止所有执行任务，并清空执行状态表
        /// </summary>
        public void StopAllTask()
        {
            foreach (var context in AppDomainContextDic.Values)
                RemoteAction.Invoke(context.Domain, () => { SchedulerContainer.GetContainerInstance().StopSchedule(); });
            _taskRunLogRespository.ClearTaskRunLog(); // 清空执行状态表
        }

        /// <summary>
        ///     根据ID立即执行
        /// </summary>
        /// <param name="config"></param>
        public void RunOnceTaskById(TaskConfig config)
        {
            if (!AppDomainContextDic.ContainsKey(config.TaskModule)) return;
            var context = AppDomainContextDic[config.TaskModule];
            RemoteAction.Invoke(context.Domain, config.Taskid,
                taskid => { SchedulerContainer.GetContainerInstance().RunOnceTask(taskid); });
            var runlog = _taskRunLogRespository.GetTaskRunLogById(config.Taskid);
            runlog.Tasklastruntime = DateTime.Now;
            _taskRunLogRespository.SaveTaskRunLog(runlog);
        }

        /// <summary>
        ///     删除指定id任务
        /// </summary>
        /// <param name="config"></param>
        public void DeleteById(TaskConfig config)
        {
            if (!AppDomainContextDic.ContainsKey(config.TaskModule)) return;
            var context = AppDomainContextDic[config.TaskModule];
            RemoteAction.Invoke(context.Domain, config.Taskid,
                taskid => { SchedulerContainer.GetContainerInstance().DeleteJob(taskid); });
            config.IsDeleted = 1;
            _taskConfigRespository.SaveData(config);
            var runlog = _taskRunLogRespository.GetTaskRunLogById(config.Taskid);
            _taskRunLogRespository.RemoveTaskRunLog(runlog);
        }


        /// <summary>
        ///     更新任务运行状态//todo 拆分为停止和恢复两个过程
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="status">任务状态</param>
        public void UpdateTaskStatus(string taskId, TaskStatus status)
        {
            //if (status == TaskStatus.Run)
            //    QuartzHelper.ResumeJob(taskId);
            //else
            //    QuartzHelper.PauseJob(taskId);
            var runlog = _taskRunLogRespository.GetTaskRunLogById(taskId);
            runlog.Taskstatus = (short) (status == TaskStatus.Run ? 0 : 1);
            _taskRunLogRespository.SaveTaskRunLog(runlog);
        }

        /// <summary>
        ///     TODO    更新任务（更新来源：1：配置库有变化（增删改查）；2：监控到task文件夹变化）
        /// </summary>
        /// <param name="taskId"></param>
        public void RefreshChangedTask(string taskId)
        {
        }

        #endregion
    }
}