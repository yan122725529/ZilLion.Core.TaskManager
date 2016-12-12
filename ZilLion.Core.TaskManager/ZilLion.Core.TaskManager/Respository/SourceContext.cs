using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using ZilLion.Core.DatabaseWrapper.Dapper;
using ZilLion.Core.TaskManager.Config;

namespace ZilLion.Core.TaskManager.Respository
{
    public class SourceContext : IDbContext
    {

        public static string WeixinContextConnectionString => TaskManagerConfig.TaskConfigDbConString;

        public IDbConnection GetConnection()
        {
            DbConnection connection = new SqlConnection(WeixinContextConnectionString);
            return connection;
        }

        public string ContextName => "WeixinO2OAuthContext";
    }
}