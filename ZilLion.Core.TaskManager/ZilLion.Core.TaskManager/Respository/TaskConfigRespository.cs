using System;
using System.Collections.Generic;
using System.Linq;
using ZilLion.Core.DatabaseWrapper.Dapper;
using ZilLion.Core.DatabaseWrapper.SqlServer;
using ZilLion.Core.QuartzWrapper.Config;
using ZilLion.Core.QuartzWrapper.Respository;

namespace ZilLion.Core.TaskManager.Respository
{
    public class TaskConfigRespository : SqlServerRespository<TaskConfig>, ITaskConfigRespository
    {
        public TaskConfigRespository(TaskRunnerConfig runnerConfig)
        {
            Context = new TaskRunnerContext(runnerConfig.TaskConfigDbConString);
        }


        public IList<TaskConfig> GetjobConfigs()
        {
            return GetList(@"select * from task_config").ToList();
        }

        /// <summary>
        ///     获取单条config
        /// </summary>
        /// <returns></returns>
        public TaskConfig GetjobConfigById(string jobid)
        {
            return
                GetList(@"select * from task_config where jobid=@jobid and isdeleted=0", new {jobid})
                    .FirstOrDefault();
        }


        /// <summary>
        ///     删除
        /// </summary>
        /// <param name="config"></param>
        public void RemoveTaskConfig(TaskConfig config)
        {
            config.IsDeleted = 1; //置状态
            SaveData(config);
        }


        public void SaveData(TaskConfig taskConfig)
        {
            if (taskConfig.Taskid.IsNullOrEmpty()) //ID为空 则新增
            {
                taskConfig.Taskid = Guid.NewGuid().ToString();
                Add(taskConfig);
            }
            else
            {
                Modify(taskConfig);
            }
        }
    }
}