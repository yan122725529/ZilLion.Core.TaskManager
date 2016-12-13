using System;
using System.Collections.Generic;
using ZilLion.Core.TaskManager.Config;
using ZilLion.Core.TaskManager.Respository;
using ZilLion.Core.TaskManager.Unities;
using ZilLion.Core.TaskManager.Unities.Quartz;

namespace ZilLion.Core.TaskManager
{
    public class TaskRunner
    {
        #region 初始化

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="jobConfigDbConString"></param>
        /// <param name="taskServerId"></param>
        public static void InitTaskRunner(string jobConfigDbConString, string taskServerId)
        {
            _instance = new TaskRunner();
            TaskManagerConfig.TaskConfigDbConString = jobConfigDbConString; //获得task配置库数据库连接
            TaskManagerConfig.TaskServerId = taskServerId; //设置TaskRunner寄宿的serverid
            QuartzHelper.InitScheduler(); //任务调度初始化成功


            //todo LOG(初始化成功)
            //todo While 循环监控数据库变化？
        }

        #endregion

        #region 作业配置

        /// <summary>
        ///     可用作业配置
        /// </summary>
        public IList<TaskConfig> UseableJobconfigs { get; set; }

        private readonly ITaskConfigRespository _taskConfigRespository = new TaskConfigRespository();
        private readonly ITaskRunLogRespository _taskRunLogRespository = new TaskRunLogRespository();

        private void GetUseableJobconfigs()
        {
            if (UseableJobconfigs == null)
                UseableJobconfigs = new List<TaskConfig>();
            UseableJobconfigs.Clear();
            UseableJobconfigs = _taskConfigRespository.GetjobConfigs();
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
            foreach (var config in UseableJobconfigs)
            {
                var runlog = new TaskRunLog
                {
                    Pcip = Computer.IpAddress,
                    Pcmac = Computer.MacAddress,
                    Pcname = Computer.ComputerName,
                    Taskid = config.Taskid,
                    Taskname = config.Taskname,
                    Taskremark = config.TaskRemark,
                    Taskserverid = TaskManagerConfig.TaskServerId,
                    Taskstatus = 0,
                    Tasknextruntime = DateTime.Now,
                    Tasklastruntime = DateTime.Now,
                };
                _taskRunLogRespository.AddTaskRunLog(runlog);
            }
            QuartzHelper.StartScheduler(UseableJobconfigs); //开始任务

        }

        /// <summary>
        ///     停止所有执行任务，并清空执行状态表
        /// </summary>
        public void StopAllTask()
        {
            QuartzHelper.StopSchedule();
            _taskRunLogRespository.ClearTaskRunLog();// 清空执行状态表
        }

        /// <summary>
        ///     根据ID立即执行
        /// </summary>
        /// <param name="taskId"></param>
        public void RunOnceTaskById(string taskId)
        {
            QuartzHelper.RunOnceTask(taskId);
            var runlog = _taskRunLogRespository.GetTaskRunLogById(taskId);
            runlog.Tasklastruntime = DateTime.Now;
            _taskRunLogRespository.ModifyTaskRunLog(runlog);
        }

        /// <summary>
        ///     删除指定id任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        public void DeleteById(string taskId)
        {
            QuartzHelper.DeleteJob(taskId);

            #region 更新配置和运行状态表

            var config = _taskConfigRespository.GetjobConfigById(taskId);
            config.IsDeleted = 1;
            _taskConfigRespository.SaveData(config);

            var runlog = _taskRunLogRespository.GetTaskRunLogById(taskId);
            _taskRunLogRespository.RemoveTaskRunLog(runlog);

            #endregion
        }

        /// <summary>
        ///     更新任务运行状态
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="status">任务状态</param>
        public void UpdateTaskStatus(string taskId, TaskStatus status)
        {
            if (status == TaskStatus.Run)
                QuartzHelper.ResumeJob(taskId);
            else
                QuartzHelper.PauseJob(taskId);
            var runlog = _taskRunLogRespository.GetTaskRunLogById(taskId);
            runlog.Taskstatus = (short) (status == TaskStatus.Run ? 0 : 1);
            _taskRunLogRespository.ModifyTaskRunLog(runlog);
        }

        /// <summary>
        /// TODO    更新任务（更新来源：1：配置库有变化（增删改查）；2：监控到task文件夹变化）
        /// </summary>
        /// <param name="taskId"></param>
        public void RefreshChangedTask(string taskId)
        {
        }

        #endregion
    }
}