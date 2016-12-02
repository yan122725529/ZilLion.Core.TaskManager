using System;
using System.Collections.Generic;
using System.Linq;
using ZilLion.Core.TaskManager.Config;
using ZilLion.Core.TaskManager.Respository;

namespace ZilLion.Core.TaskManager.Unities.Quartz
{
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
            if (status == TaskStatus.Run)
                QuartzHelper.ResumeJob(taskId);
            else
                QuartzHelper.PauseJob(taskId);
            var config = GetById(taskId);
            config.Jobstatus = status == TaskStatus.Run ? 0 : 1;
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
        /// <param name="taskId">任务id</param>
        /// <param name="lastRunTime">下次运行时间</param>
        public static void UpdateLastRunTime(string taskId, DateTime lastRunTime)
        {
            //SQLHelper.ExecuteNonQuery("UPDATE p_Task SET LastRunTime=@LastRunTime WHERE TaskID=@TaskID", new { TaskID = TaskID, LastRunTime = LastRunTime });
        }

        /// <summary>
        ///     更新任务最近运行时间
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="lastRunTime"></param>
        public static void UpdateRecentRunTime(string taskId, DateTime lastRunTime)
        {
            //SQLHelper.ExecuteNonQuery("UPDATE p_Task SET RecentRunTime=GETDATE(),LastRunTime=@LastRunTime WHERE TaskID=@TaskID", new { TaskID = TaskID, LastRunTime = LastRunTime });
        }


        
    }
}