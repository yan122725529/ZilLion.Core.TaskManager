using System.Collections.Generic;
using System.Linq;
using ZilLion.Core.DatabaseWrapper.Dapper;
using ZilLion.Core.TaskManager.Config;

namespace ZilLion.Core.TaskManager.Respository
{
    public class TaskRunLogRespository : Respository<TaskRunLog>, ITaskRunLogRespository
    {
        public TaskRunLogRespository()
        {
            Context = new SourceContext();
        }

        /// <summary>
        ///     获取当前serverid所有记录
        /// </summary>
        /// <returns></returns>
        public IList<TaskRunLog> GetAllTaskRunLog()
        {
            return
                GetList<TaskRunLog>(@"select * from task_run_log where taskserverid=@serverid",
                    new {serverid = TaskManagerConfig.TaskServerId}).ToList();
        }

        /// <summary>
        ///     清空当前serverid所有记录
        /// </summary>
        public void ClearTaskRunLog()
        {
            Execute(@"delete from task_run_log where taskserverid=@serverid",
                new {serverid = TaskManagerConfig.TaskServerId});
        }

        /// <summary>
        /// 按ID 或者log
        /// </summary>
        /// <param name="taskid"></param>
        /// <returns></returns>
        public TaskRunLog GetTaskRunLogById(string taskid)
        {
            return
                GetList<TaskRunLog>(@"select * from task_run_log where taskid=@taskid and taskserverid=@serverid",
                    new {serverid = TaskManagerConfig.TaskServerId, taskid}).FirstOrDefault();
        }

        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="log"></param>
        public void AddTaskRunLog(TaskRunLog log)
        {
            Add(log);
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="log"></param>
        public void ModifyTaskRunLog(TaskRunLog log)
        {
            Modify(log);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="log"></param>
        public void RemoveTaskRunLog(TaskRunLog log)
        {
            Remove(log);
        }
    }
}