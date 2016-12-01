namespace ZilLion.Core.TaskManager.Config
{
    public class Jobconfig
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
}