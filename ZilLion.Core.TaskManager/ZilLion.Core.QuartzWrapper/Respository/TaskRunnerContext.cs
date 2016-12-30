using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using ZilLion.Core.DatabaseWrapper.Dapper;

namespace ZilLion.Core.QuartzWrapper.Respository
{
    public class TaskRunnerContext : IDbContext
    {
        private readonly string _weixinContextConnectionString;


        public TaskRunnerContext(string weixinContextConnectionString)
        {
            _weixinContextConnectionString = weixinContextConnectionString;
        }

        public IDbConnection GetConnection()
        {
            DbConnection connection = new SqlConnection(_weixinContextConnectionString);
            return connection;
        }

        public string ContextName => "TaskRunnerContext";
    }
}