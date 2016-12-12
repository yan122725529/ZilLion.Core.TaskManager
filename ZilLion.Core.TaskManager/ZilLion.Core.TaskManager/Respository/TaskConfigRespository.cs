using System;
using System.Collections.Generic;
using System.Linq;
using ZilLion.Core.DatabaseWrapper.Dapper;
using ZilLion.Core.TaskManager.Config;

namespace ZilLion.Core.TaskManager.Respository
{
    public class TaskConfigRespository : Respository<TaskConfig>
    {
        public TaskConfigRespository()
        {
            Context = new SourceContext();
        }

        public IList<TaskConfig> GetjobConfigs()
        {
            return GetList<TaskConfig>(@"select * from task_job_config").ToList();
        }

        /// <summary>
        ///     获取单条config
        /// </summary>
        /// <returns></returns>
        public TaskConfig GetjobConfig(string jobid)
        {
            return
                GetList<TaskConfig>(@"select * from task_job_config where jobid=@jobid and isdeleted=0", new {jobid})
                    .FirstOrDefault();
        }


        /// <summary>
        ///     删除
        /// </summary>
        /// <param name="config"></param>
        public void RemoveTaskConfig(TaskConfig config)
        {
            config.IsDeleted = 1;//置状态
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