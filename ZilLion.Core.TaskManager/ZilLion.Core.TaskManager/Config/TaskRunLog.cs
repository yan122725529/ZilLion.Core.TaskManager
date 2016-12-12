using System;
using DapperExtensions.Mapper;

namespace ZilLion.Core.TaskManager.Config
{
    public class TaskRunLog
    {
        public string Taskserverid { get; set; }
        public string Pcip { get; set; }
        public string Pcmac { get; set; }
        public string Pcname { get; set; }
        public string Taskid { get; set; }
        public string Taskname { get; set; }
        public short Taskstatus { get; set; }
        public DateTime Tasknextruntime { get; set; }
        public DateTime Tasklastruntime { get; set; }
        public string Taskremark { get; set; }
    }
    public sealed class TaskRunLogMapper : ClassMapper<TaskRunLog>
    {
        public TaskRunLogMapper()
        {
            Table("task_run_log");
            Map(f => f.Taskserverid).Column("taskserverid");
            Map(f => f.Pcip).Column("pcip");
            Map(f => f.Pcmac).Column("pcmac");
            Map(f => f.Pcname).Column("pcname");
            Map(f => f.Taskid).Column("taskid");
            Map(f => f.Taskname).Column("taskname");
            Map(f => f.Taskstatus).Column("taskstatus");
            Map(f => f.Tasknextruntime).Column("tasknextruntime");
            Map(f => f.Tasklastruntime).Column("tasklastruntime");
            Map(f => f.Taskremark).Column("taskremark");
        }
    }
}