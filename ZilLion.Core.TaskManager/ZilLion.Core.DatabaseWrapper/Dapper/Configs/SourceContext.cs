using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace ZilLion.Core.DatabaseWrapper.Dapper.Configs
{
    public class SourceContext : IDbContext
    {

        /// <summary>
        /// DB数据模型命令表
        /// </summary>
        public static List<DbModelCmd> DbModelCmdList { get; set; }

        /// <summary>
        /// DB数据模型命令结构表
        /// </summary>
        public static List<DbModelCmdRs> DbModelCmdRsList { get; set; }

        public static string _systemDataConnectionString;

        public static string SystemDataConnectionString
        {
            get
            {
                if (AccountContext.Answer.Server == null) return string.Empty;
                return _systemDataConnectionString = string.Format(
                    @"data source={0};initial catalog={1};user id={2};password={3}",
                    string.Format("{0};pooling=true;min pool size=1;max pool size=1;Connection Timeout=0",
                        AccountContext.Answer.Server), ServerConfig.Accname, AccountContext.Answer.User,
                    AccountContext.Answer.Pwd);
            }
        }

        public IDbConnection GetConnection()
        {
            DbConnection connection = new SqlConnection(SystemDataConnectionString);
            //string strconn = string.Format("server={0};uid={1};pwd={2};database={3}", "devserver,2006", "sa", "", "z9报表");
            //DbConnection connection = new SqlConnection(strconn);
            return connection;
        }

        public string ContextName
        {
            get { return "DefaultSource"; }
        }
    }
}