using System;

namespace ZilLion.Core.TaskManager.Unities.Quartz
{
    public class TaskRunLog
    {
        /// <summary>
        ///     任务ID
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        ///     任务名称
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        ///     运行频率设置
        /// </summary>
        public string CronExpressionString { get; set; }

        /// <summary>
        ///     任务运频率中文说明
        /// </summary>
        public string CronRemark { get; set; }

        /// <summary>
        ///     任务所在DLL对应的程序集名称
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        ///     任务所在类
        /// </summary>
        public string Class { get; set; }

        public TaskStatus Status { get; set; }

        /// <summary>
        ///     任务状态中文说明
        /// </summary>
        public string StatusCn => Status == TaskStatus.Stop ? "停止" : "运行";

        /// <summary>
        ///     任务创建时间
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        ///     任务修改时间
        /// </summary>
        public DateTime? ModifyOn { get; set; }

        /// <summary>
        ///     任务最近运行时间
        /// </summary>
        public DateTime? RecentRunTime { get; set; }

        /// <summary>
        ///     任务下次运行时间
        /// </summary>
        public DateTime? LastRunTime { get; set; }

        /// <summary>
        ///     任务备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        ///     对应配置是否已经删除
        /// </summary>
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    ///     任务状态枚举
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        ///     运行状态
        /// </summary>
        Run = 0,

        /// <summary>
        ///     停止状态
        /// </summary>
        Stop = 1
    }
}