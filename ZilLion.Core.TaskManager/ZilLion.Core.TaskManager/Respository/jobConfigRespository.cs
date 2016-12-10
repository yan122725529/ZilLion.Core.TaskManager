using System;
using System.Collections.Generic;
using System.Linq;
using Quartz.Util;
using ZilLion.Core.DatabaseWrapper.Dapper;
using ZilLion.Core.TaskManager.Config;

namespace ZilLion.Core.TaskManager.Respository
{
    public class JobConfigRespository : Respository<Taskconfig>
    {
        public JobConfigRespository()
        {
            Context = new SourceContext();
        }

        public IList<Taskconfig> GetjobConfigs()
        {
            return GetList<Taskconfig>(@"select * from task_job_config").ToList();
        }

        /// <summary>
        ///     获取单条config
        /// </summary>
        /// <returns></returns>
        public Taskconfig GetjobConfig(string jobid)
        {
            return GetList<Taskconfig>(@"select * from task_job_config where jobid=@jobid", new {jobid}).FirstOrDefault();
        }



        public void SaveData(Taskconfig taskconfig)
        {
            if (taskconfig.Jobid.IsNullOrEmpty())//ID为空 则新增
            {
                taskconfig.Jobid = Guid.NewGuid().ToString();
                Add(taskconfig);
            }
            else
            {
                Modify(taskconfig);
            }
        }



    }
}