using System;
using System.Collections.Generic;
using ZilLion.Core.TaskManager.Config;

namespace ZilLion.Core.TaskManager.Respository
{
    public interface ITaskConfigRespository
    {
        IList<TaskConfig> GetjobConfigs();

        /// <summary>
        ///     获取单条config
        /// </summary>
        /// <returns></returns>
        TaskConfig GetjobConfigById(string jobid);


        /// <summary>
        ///     删除
        /// </summary>
        /// <param name="config"></param>
         void RemoveTaskConfig(TaskConfig config);


        void SaveData(TaskConfig taskConfig);
    }
}