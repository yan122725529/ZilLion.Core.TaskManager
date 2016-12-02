using ZilLion.Core.DatabaseWrapper.Dapper;
using ZilLion.Core.TaskManager.Unities.Quartz;

namespace ZilLion.Core.TaskManager.Respository
{
    public class TaskRunLogRespository : Respository<TaskRunLog>
    {
        public TaskRunLogRespository()
        {
            Context = new SourceContext();
        }
    }
}