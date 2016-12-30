using System.Collections.Generic;
using System.Linq;
using ZilLion.Core.DatabaseWrapper.Dapper;
using ZilLion.Core.DatabaseWrapper.SqlServer;
using ZilLion.Core.QuartzWrapper.Config;
using ZilLion.Core.QuartzWrapper.Quartz;

namespace ZilLion.Core.QuartzWrapper.Respository
{
    public class TaskRunLogRespository : SqlServerRespository<TaskRunLog>, ITaskRunLogRespository
    {
        private readonly TaskRunnerConfig _runnerConfig;

      
        public TaskRunLogRespository(TaskRunnerConfig runnerConfig)
        {
            _runnerConfig = runnerConfig;
            Context = new TaskRunnerContext(_runnerConfig.TaskConfigDbConString);
        }
        /// <summary>
        ///     获取当前serverid所有记录
        /// </summary>
        /// <returns></returns>
        public IList<TaskRunLog> GetAllTaskRunLog()
        {
            return
                GetList(@"select * from task_run_log where taskserverid=@serverid",
                    new {serverid = _runnerConfig.TaskServerId}).ToList();
        }

        /// <summary>
        ///     清空当前serverid所有记录
        /// </summary>
        public void ClearTaskRunLog()
        {
            DapperHelper.Execute(@"delete from task_run_log where taskserverid=@serverid", Context,
                new {serverid = _runnerConfig.TaskServerId});
        }

        /// <summary>
        ///     按ID 或者log
        /// </summary>
        /// <param name="taskid"></param>
        /// <returns></returns>
        public TaskRunLog GetTaskRunLogById(string taskid)
        {
            return
                GetList(@"select * from task_run_log where taskid=@taskid and taskserverid=@serverid",
                    new {serverid = _runnerConfig.TaskServerId, taskid}).FirstOrDefault();
        }

      
        /// <summary>
        ///     修改
        /// </summary>
        /// <param name="log"></param>
        public void SaveTaskRunLog(TaskRunLog log)
        {

            var old = GetTaskRunLogById(log.Taskid);
            if (old != null)
            {
                Modify(log);
            }
            else
            {
                Add(log);
            }
           
        }

        /// <summary>
        ///     删除
        /// </summary>
        /// <param name="log"></param>
        public void RemoveTaskRunLog(TaskRunLog log)
        {
            Remove(log);
        }
    }
}