using System;
using System.Collections.Generic;
using System.Linq;
using ZilLion.Core.TaskManager.Config;
using ZilLion.Core.TaskManager.Respository;

namespace ZilLion.Core.TaskManager.Unities.Quartz
{
    /// <summary>
    ///     任务实体
    /// </summary>
    public class TaskUtil
    {
        /// <summary>
        ///     任务ID
        /// </summary>
        public Guid TaskID { get; set; }

        /// <summary>
        ///     任务名称
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        ///     运行频率设置
        /// </summary>
        public string CronExpressionString { get; set; }

        /// <summary>
        ///     任务运频率中文说明
        /// </summary>
        public string CronRemark { get; set; }

        /// <summary>
        ///     任务所在DLL对应的程序集名称
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        ///     任务所在类
        /// </summary>
        public string Class { get; set; }

        public TaskStatus Status { get; set; }

        /// <summary>
        ///     任务状态中文说明
        /// </summary>
        public string StatusCn
        {
            get { return Status == TaskStatus.STOP ? "停止" : "运行"; }
        }

        /// <summary>
        ///     任务创建时间
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        ///     任务修改时间
        /// </summary>
        public DateTime? ModifyOn { get; set; }

        /// <summary>
        ///     任务最近运行时间
        /// </summary>
        public DateTime? RecentRunTime { get; set; }

        /// <summary>
        ///     任务下次运行时间
        /// </summary>
        public DateTime? LastRunTime { get; set; }

        /// <summary>
        ///     任务备注
        /// </summary>
        public string Remark { get; set; }
    }

    /// <summary>
    ///     任务状态枚举
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        ///     运行状态
        /// </summary>
        RUN = 0,

        /// <summary>
        ///     停止状态
        /// </summary>
        STOP = 1
    }

    /// <summary>
    ///     任务帮助类
    /// </summary>
    public class TaskHelper
    {
        private static readonly JobConfigRespository JobConfigRespository;


        static TaskHelper()
        {
            JobConfigRespository = new JobConfigRespository();
        }

        /// <summary>
        ///     获取指定id任务数据
        /// </summary>
        /// <returns>任务数据</returns>
        public static Jobconfig GetById(string jobid)
        {
            return JobConfigRespository.GetjobConfig(jobid);
        }


        /// <summary>
        ///     根据jobid运行任务
        /// </summary>
        /// <param name="jobid"></param>
        public static void RunById(string jobid)
        {
            QuartzHelper.RunOnceTask(jobid);
        }

        /// <summary>
        ///     删除指定id任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        public static void DeleteById(string taskId)
        {
            QuartzHelper.DeleteJob(taskId);
        }

        /// <summary>
        ///     更新任务运行状态
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="status">任务状态</param>
        public static void UpdateTaskStatus(string taskId, TaskStatus status)
        {
            if (status == TaskStatus.RUN)
                QuartzHelper.ResumeJob(taskId);
            else
                QuartzHelper.PauseJob(taskId);
            var config = GetById(taskId);
            config.Jobstatus = status == TaskStatus.RUN ? 0 : 1;
            JobConfigRespository.SaveData(config);
        }


        /// <summary>
        ///     获取所有启用的任务
        /// </summary>
        /// <returns>所有启用的任务</returns>
        public static IList<Jobconfig> ReadConfig()
        {
            return JobConfigRespository.GetjobConfigs().Where(x => x.Jobstatus == 0).ToList();
        }

   
        /// <summary>
        ///     更新任务下次运行时间
        /// </summary>
        /// <param name="TaskID">任务id</param>
        /// <param name="LastRunTime">下次运行时间</param>
        public static void UpdateLastRunTime(string TaskID, DateTime LastRunTime)
        {
            //SQLHelper.ExecuteNonQuery("UPDATE p_Task SET LastRunTime=@LastRunTime WHERE TaskID=@TaskID", new { TaskID = TaskID, LastRunTime = LastRunTime });
        }

        /// <summary>
        ///     更新任务最近运行时间
        /// </summary>
        /// <param name="TaskID">任务id</param>
        public static void UpdateRecentRunTime(string TaskID, DateTime LastRunTime)
        {
            //SQLHelper.ExecuteNonQuery("UPDATE p_Task SET RecentRunTime=GETDATE(),LastRunTime=@LastRunTime WHERE TaskID=@TaskID", new { TaskID = TaskID, LastRunTime = LastRunTime });
        }


        
    }
}