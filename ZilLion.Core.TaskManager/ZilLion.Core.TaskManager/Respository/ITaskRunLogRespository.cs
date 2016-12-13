using System.Collections.Generic;
using ZilLion.Core.TaskManager.Config;

namespace ZilLion.Core.TaskManager.Respository
{
    public interface ITaskRunLogRespository
    {
        /// <summary>
        ///     获取当前serverid所有记录
        /// </summary>
        /// <returns></returns>
        IList<TaskRunLog> GetAllTaskRunLog();

        /// <summary>
        ///     清空当前serverid所有记录
        /// </summary>
         void ClearTaskRunLog();

        /// <summary>
        /// 按ID 或者log
        /// </summary>
        /// <param name="taskid"></param>
        /// <returns></returns>
         TaskRunLog GetTaskRunLogById(string taskid);

        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="log"></param>
         void AddTaskRunLog(TaskRunLog log);

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="log"></param>
        void ModifyTaskRunLog(TaskRunLog log);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="log"></param>
        void RemoveTaskRunLog(TaskRunLog log);
    }
}