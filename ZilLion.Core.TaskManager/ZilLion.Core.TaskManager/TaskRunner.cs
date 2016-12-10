using System;
using System.Collections.Generic;
using System.Linq;
using ZilLion.Core.TaskManager.Config;
using ZilLion.Core.TaskManager.Respository;
using ZilLion.Core.TaskManager.Unities.Quartz;

namespace ZilLion.Core.TaskManager
{
    public class TaskRunner
    {
        #region 作业配置

        /// <summary>
        ///     可用作业配置
        /// </summary>
        public IList<Taskconfig> UseableJobconfigs { get; set; }

        private readonly JobConfigRespository _jobConfigRespository = new JobConfigRespository();

        private  void GetUseableJobconfigs()
        {
            if (UseableJobconfigs == null)
                UseableJobconfigs = new List<Taskconfig>();
            UseableJobconfigs.Clear();
            UseableJobconfigs = _jobConfigRespository.GetjobConfigs();
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

        #region 初始化
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="jobConfigDbConString"></param>
        public static void InitTaskRunner(string jobConfigDbConString)
        {
            _instance = new TaskRunner();
            TaskManagerConfig.JobConfigDbConString = jobConfigDbConString;//获得task配置库数据库连接
            //GetUseableJobconfigs();
            QuartzHelper.InitScheduler();//任务调度初始化成功

            //todo InitTastRunner日志
            //todo While 循环监控数据库变化？

        }


        #endregion

        #region 任务执行

        /// <summary>
        /// 开始所有执行的任务，并且结合配置执行状态表数据
        /// </summary>
        public void StartAllScheduler()
        {

            GetUseableJobconfigs();//读取任务配置
            QuartzHelper.StartScheduler(this.UseableJobconfigs);//开始任务

            //生成任务执行日志（包括状态等）
        }
        /// <summary>
        /// 停止所有执行任务，并清空执行状态表
        /// </summary>
        public void StopAllTask()
        {
            QuartzHelper.StopSchedule();
            //todo 清空执行状态表

        }

        /// <summary>
        /// 根据ID立即执行
        /// </summary>
        /// <param name="taskId"></param>
        public  void RunOnceTaskById(string taskId)
        {
            QuartzHelper.RunOnceTask(taskId);
        }

        /// <summary>
        /// 删除指定id任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        public void DeleteById(string taskId)
        {
            QuartzHelper.DeleteJob(taskId);
            //_jobConfigRespository.SaveData();//todo 保存数据库（配置表更新，任务状态表更新）
        }

        /// <summary>
        /// 更新任务运行状态
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="status">任务状态</param>
        public  void UpdateTaskStatus(string taskId, TaskStatus status)
        {
            if (status == TaskStatus.Run)
                QuartzHelper.ResumeJob(taskId);
            else
                QuartzHelper.PauseJob(taskId);
            var config = _jobConfigRespository.GetjobConfig(taskId);
            config.Jobstatus = status == TaskStatus.Run ? 0 : 1;
            _jobConfigRespository.SaveData(config);
        }

        /// <summary>
        /// 更新任务（更新来源：1：配置库有变化（增删改查）；2：监控到task文件夹变化）
        /// </summary>
        /// <param name="taskId"></param>

        public void RefreshChangedTask(string taskId)
        {

        }

        #endregion
    }
}
