using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace ZilLion.Core.DatabaseWrapper.Dapper.Configs
{
    public class AccountContext : IDbContext
    {
        public static ServerAnswer Answer = new ServerAnswer();
        private static string _systemDataConnectionString;

        public static string SystemDataConnectionString
        {
            get
            {
                if (Answer.Server == null) return string.Empty;
                return _systemDataConnectionString = string.Format(
                    @"data source={0};initial catalog={1};user id={2};password={3}",
                    string.Format("{0};pooling=true;min pool size=1;max pool size=1;Connection Timeout=0",
                        Answer.Server), "master", Answer.User, Answer.Pwd);
            }
        }

        public IDbConnection GetConnection()
        {
            DbConnection connection = new SqlConnection(SystemDataConnectionString);
            return connection;
        }

        public string ContextName
        {
            get { return "AccountSource"; }
        }
    }
}