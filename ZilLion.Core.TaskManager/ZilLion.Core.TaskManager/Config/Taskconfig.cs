using DapperExtensions.Mapper;

namespace ZilLion.Core.TaskManager.Config
{
    public class TaskConfig
    {
        /// <summary>
        /// 作业ID
        /// </summary>
        public string Taskid { get; set; }

        /// <summary>
        /// 作业名称
        /// </summary>
        public string Taskname { get; set; }

        /// <summary>
        /// 作业说明
        /// </summary>
        public string TaskRemark { get; set; }
      
        /// <summary>
        /// 作业模块名（决定使用哪个程序集）
        /// </summary>
        public string TaskModule { get; set; }

        /// <summary>
        /// 作业Action名（决定使用哪个策略类）
        /// </summary>
        public string TaskAction { get; set; }

        /// <summary>
        /// 作业参数
        /// </summary>
        public string TaskParam { get; set; }

        /// <summary>
        /// 作业执行时间表达式
        /// </summary>
        public string TaskExpression { get; set; }

        /// <summary>
        /// 作业执行状态 0-运行中，1-停止
        /// </summary>
        public int TaskStatus { get; set; }


        public short IsDeleted { get; set; }

    }

    public sealed class TaskconfigMapper : ClassMapper<TaskConfig>
    {
        public TaskconfigMapper()
        {
            Table("task_config");
            Map(f => f.Taskid).Column("taskid");
            Map(f => f.Taskname).Column("taskname");
            Map(f => f.TaskRemark).Column("taskremark");
            Map(f => f.TaskModule).Column("taskmodule");
            Map(f => f.TaskAction).Column("taskAction");
            Map(f => f.TaskParam).Column("taskparam");
            Map(f => f.TaskExpression).Column("taskronexpression");
            Map(f => f.TaskStatus).Column("taskstatus");
            Map(f => f.IsDeleted).Column("isdeleted");
        }
    }


    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// 运行状态
        /// </summary>
        Run = 0,

        /// <summary>
        /// 停止状态
        /// </summary>
        Stop = 1
    }
}