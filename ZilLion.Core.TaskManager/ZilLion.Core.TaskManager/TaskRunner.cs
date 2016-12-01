using System;
using System.Collections.Generic;
using ZilLion.Core.TaskManager.Config;

namespace ZilLion.Core.TaskManager
{
    public class TaskRunner
    {
        #region 作业配置

        /// <summary>
        /// 可用作业配置
        /// </summary>
        public IList<Jobconfig> UseableJobconfigs { get; set; }

        private void GetUseableJobconfigs()
        {
            if (UseableJobconfigs==null)
                UseableJobconfigs=new List<Jobconfig>();
            UseableJobconfigs.Clear();
            //todo 从数据库获取
            UseableJobconfigs=new List<Jobconfig>() {new Jobconfig() {} };
        }


        #endregion



        #region 单例

        private static TaskRunner _instance;

        private TaskRunner()
        {
        }

        public static void InitTastRunner(string jobConfigDbConString)
        {
            _instance = new TaskRunner();
            TaskManagerConfig.JobConfigDbConString = jobConfigDbConString;
        }

        public static TaskRunner Getinstance()
        {
            if (_instance == null)
                throw new Exception("TaskRunner尚未初始化，请调用InitTastRunner方法初始化。");

            return _instance;
        }

        #endregion

        #region 刷新配置
        public void RefreshJob(List<string> joblist = null)
        {
            var todolist = new List<Jobconfig>();
            if (joblist == null)
            {
                //刷新全部配置
                GetUseableJobconfigs();

            }
            else
            {

            }


        }
        /// <summary>
        /// 重启job
        /// </summary>
        /// <param name="jobid"></param>
        private void Restartjob(string jobid)
        {

        }


        #endregion




    }
}