using System;
using System.Data;
using System.Data.SqlClient;

namespace ZilLion.Core.TaskManager.Unities
{
    public static class SqlHelper
    {
        /// <summary>
        ///     适用于事务在方法外处理
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="strsql"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public static DataSet GetData(SqlConnection conn, string strsql)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            var dt = new DataSet();
            var cmd = new SqlCommand(strsql, conn);
            var ad = new SqlDataAdapter(cmd);
            ad.Fill(dt);
            return dt;
        }

       
        /// <summary>
        ///     事务在方法内自动完成
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="strsql"></param>
        /// <returns></returns>
        public static DataSet ExecWithAutoTrans(SqlConnection conn, string strsql)
        {
            using (conn)
            {
                var dt = new DataSet();
                SqlCommand cmd = null;
                SqlDataAdapter ad = null;
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    cmd = new SqlCommand(strsql, conn, tran);
                    ad = new SqlDataAdapter(cmd);
                    ad.Fill(dt);
                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    conn.Close();
                    throw;
                }
                return dt;
            }
        }
    }
}