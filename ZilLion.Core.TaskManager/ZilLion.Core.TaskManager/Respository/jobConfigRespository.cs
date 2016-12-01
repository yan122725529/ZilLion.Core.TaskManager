using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ZilLion.Core.TaskManager.Config;
using ZilLion.Core.TaskManager.Unities;

namespace ZilLion.Core.TaskManager.Respository
{
    public class JobConfigRespository
    {
        private SqlConnection _connection;

        public JobConfigRespository()
        {
            _connection = new SqlConnection(TaskManagerConfig.JobConfigDbConString);
           
        }

        public List<Jobconfig> GetjobConfigs()
        {
            var result = new List<Jobconfig>();
            var dataset = SqlHelper.GetData(_connection, @"select * from task_job_config");

            if (dataset != null)
            {
                var datatable = dataset.Tables[0];
                if (datatable != null)
                {
                    foreach (DataRow row in datatable.Rows)
                    {

                        var config = new Jobconfig
                        {
                            Jobid = row["jobid"].ToString(),
                            Jobname = row["jobname"].ToString(),
                            Jobremark = row["jobremark"].ToString(),
                            Jobmodule = row["jobmodule"].ToString(),
                            JobAction = row["jobAction"].ToString(),
                            Jobparam = row["jobparam"].ToString(),
                            Jobronexpression = row["jobronexpression"].ToString(),
                            Jobstatus = Convert.ToInt32(row["jobstatus"].ToString())
                        };

                        result.Add(config);


                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 获取单条config
        /// </summary>
        /// <returns></returns>
        public Jobconfig GetjobConfig(string jobid)
        {
            var result = new List<Jobconfig>();
            var dataset = SqlHelper.GetData(_connection, $@"select * from task_job_config where jobid='{jobid}'");

            if (dataset != null)
            {
                var datatable = dataset.Tables[0];
                if (datatable != null)
                {
                    foreach (DataRow row in datatable.Rows)
                    {

                        var config = new Jobconfig
                        {
                            Jobid = row["jobid"].ToString(),
                            Jobname = row["jobname"].ToString(),
                            Jobremark = row["jobremark"].ToString(),
                            Jobmodule = row["jobmodule"].ToString(),
                            JobAction = row["jobAction"].ToString(),
                            Jobparam = row["jobparam"].ToString(),
                            Jobronexpression = row["jobronexpression"].ToString(),
                            Jobstatus = Convert.ToInt32(row["jobstatus"].ToString())
                        };

                        result.Add(config);


                    }
                }
            }
            return result.FirstOrDefault();
        }
        /// <summary>
        /// 添加config
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        //public bool AddConfig(Jobconfig config)
        //{

        //}
    }
}