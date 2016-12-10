using DapperExtensions.Mapper;

namespace ZilLion.Core.TaskManager.Config
{
    public class Taskconfig
    {
        /// <summary>
        /// 作业ID
        /// </summary>
        public string Jobid { get; set; }

        /// <summary>
        /// 作业名称
        /// </summary>
        public string Jobname { get; set; }

        /// <summary>
        /// 作业说明
        /// </summary>
        public string Jobremark { get; set; }
      
        /// <summary>
        /// 作业模块名（决定使用哪个程序集）
        /// </summary>
        public string Jobmodule { get; set; }

        /// <summary>
        /// 作业Action名（决定使用哪个策略类）
        /// </summary>
        public string JobAction { get; set; }

        /// <summary>
        /// 作业参数
        /// </summary>
        public string Jobparam { get; set; }

        /// <summary>
        /// 作业执行时间表达式
        /// </summary>
        public string Jobronexpression { get; set; }

        /// <summary>
        /// 作业执行状态 0-运行中，1-停止
        /// </summary>
        public int Jobstatus { get; set; }

    }

    public sealed class JobconfigMapper : ClassMapper<Taskconfig>
    {
        public JobconfigMapper()
        {
            Table("task_job_config");
            Map(f => f.Jobid).Column("jobid");
            Map(f => f.Jobname).Column("jobname");
            Map(f => f.Jobremark).Column("jobremark");
            Map(f => f.Jobmodule).Column("jobmodule");
            Map(f => f.JobAction).Column("jobAction");
            Map(f => f.Jobparam).Column("jobparam");
            Map(f => f.Jobronexpression).Column("jobronexpression");
            Map(f => f.Jobstatus).Column("jobstatus");

        }
    }
}