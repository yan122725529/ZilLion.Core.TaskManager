using System;
using System.Collections.Generic;
using System.Linq;
using Quartz.Util;
using ZilLion.Core.DatabaseWrapper.Dapper;
using ZilLion.Core.TaskManager.Config;

namespace ZilLion.Core.TaskManager.Respository
{
    public class JobConfigRespository : Respository<Jobconfig>
    {
        public JobConfigRespository()
        {
            Context = new SourceContext();
        }

        public IList<Jobconfig> GetjobConfigs()
        {
            return GetList<Jobconfig>(@"select * from task_job_config").ToList();
        }

        /// <summary>
        ///     获取单条config
        /// </summary>
        /// <returns></returns>
        public Jobconfig GetjobConfig(string jobid)
        {
            return GetList<Jobconfig>(@"select * from task_job_config where jobid=@jobid", new {jobid}).FirstOrDefault();
        }



        public void SaveData(Jobconfig jobconfig)
        {
            if (jobconfig.Jobid.IsNullOrEmpty())//ID为空 则新增
            {
                jobconfig.Jobid = Guid.NewGuid().ToString();
                Add(jobconfig);
            }
            else
            {
                Modify(jobconfig);
            }
        }



    }
}