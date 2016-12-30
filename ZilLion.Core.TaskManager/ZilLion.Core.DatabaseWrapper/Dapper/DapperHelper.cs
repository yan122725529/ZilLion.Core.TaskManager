using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using DapperExtensions;
using Oracle.ManagedDataAccess.Client;

namespace ZilLion.Core.DatabaseWrapper.Dapper
{
    public class DapperHelper
    {
        private DataBaseType GetDataBaseType(IDbConnection connection)
        {
            if (connection == null) return DataBaseType.SqlServer; //默认
            if (connection is SqlConnection)
                return DataBaseType.SqlServer;
            if (connection is SQLiteConnection)
                return DataBaseType.Sqlite;
            if (connection is OracleConnection)
                return DataBaseType.Oracle;
            return DataBaseType.SqlServer; //默认
        }

        /// <summary>
        ///     执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="context"></param>
        protected void NoLockInvoke(Action<IDbConnection, IDbTransaction> action, IDbContext context = null)
        {
            if (context == null) throw new Exception("dapper操作的context不可以为空");
            using (var connection = context.GetConnection())
            {
                try
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        action(connection, transaction);
                        transaction.Commit();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        #region 增删改差

        /// <summary>
        ///     执行SQL,映射实体
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="context"></param>
        public static void Execute(string sql, IDbContext context = null, dynamic parameters = null)
        {
            if (context == null) throw new Exception("dapper操作的context不可以为空");

            Func<IDbConnection, IDbTransaction, object, object> action = null;
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork == null) ||
                (currentUnitOfWork.Option == UnitOfWorkOption.Suppress))
            {
                var writeConnection = context.GetConnection();
                try
                {
                    writeConnection.Open();
                    SqlMapper.Execute(writeConnection, sql, parameters);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    writeConnection.Close();
                }
            }
            if (currentUnitOfWork == null) return;
            action = (conn, tran, entity) => SqlMapper.Execute(conn, sql, parameters, tran);
            currentUnitOfWork.ChangeOperator(action, context);
        }

        /// <summary>
        ///     获取所有实体
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<T> GetAllModel<T>(object predicate = null) where T : class, new()
        {
            IEnumerable<T> list = null;
            NoLockInvoke(
                delegate(IDbConnection conn, IDbTransaction tran)
                {
                    list = conn.GetList<T>(predicate, null, tran, null, false);
                });
            return list;
        }

        /// <summary>
        ///     判断是否是改变数据库的SQL dcl
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static bool IsChangeSqlString(string sql)
        {
            sql = sql.Trim().ToLower();
            return Regex.IsMatch(sql, "^(update|delete|insert|create|alter|drop|truncate)") ||
                   Regex.IsMatch(sql, @"^(select)\s+(into)\s");
        }

        /// <summary>
        ///     根据SQL获取实体列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<T> GetList<T>(string sql, dynamic parameters = null)
        {
            if (IsChangeSqlString(sql))
                throw new Exception("sql语句错误,执行的是查询方法,但SQL语句包含更新代码,SQL语句:" + sql);
            IEnumerable<T> list = null;
            NoLockInvoke(
                delegate(IDbConnection conn, IDbTransaction tran)
                {
                    list = (IEnumerable<T>) SqlMapper.Query<T>(conn, sql, parameters, tran);
                });
            return list;
        }

        /// <summary>
        ///     删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="context"></param>
        protected void RemoveModel<T>(T item, IDbContext context = null) where T : class, new()
        {
            if (context == null) throw new Exception("dapper操作的context不可以为空");
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork == null) ||
                (currentUnitOfWork.Option == UnitOfWorkOption.Suppress))
            {
                var writeConnection = context.GetConnection();
                try
                {
                    writeConnection.Open();
                    writeConnection.Delete(item, null, null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    writeConnection.Close();
                }
            }
            else
            {
                Func<IDbConnection, IDbTransaction, object, object> action =
                    (conn, tran, entity) => conn.Delete(item, tran, null);
                currentUnitOfWork.ChangeOperator(action, context);
            }
        }

        /// <summary>
        ///     更新实体
        /// </summary>
        /// <param name="item"></param>
        /// <param name="context"></param>
        protected void ModifyModel<T>(T item, IDbContext context = null) where T : class, new()
        {
            if (context == null) throw new Exception("dapper操作的context不可以为空");
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork == null) ||
                (currentUnitOfWork.Option == UnitOfWorkOption.Suppress))
            {
                var writeConnection = context.GetConnection();
                try
                {
                    writeConnection.Open();
                    writeConnection.Update(item, null, null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    writeConnection.Close();
                }
            }
            else
            {
                Func<IDbConnection, IDbTransaction, object, object> action =
                    (conn, tran, entity) => conn.Update(item, tran, null);
                currentUnitOfWork.ChangeOperator(action, context);
            }
        }

        /// <summary>
        ///     获取单个标量值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected T? GetDataByScalar<T>(string sql, dynamic parameters = null) where T : struct
        {
            if (IsChangeSqlString(sql))
                throw new Exception("sql语句错误,执行的是查询方法,但SQL语句包含更新代码,SQL语句:" + sql);
            T? value = null;
            var type = typeof(T);
            NoLockInvoke(delegate(IDbConnection conn, IDbTransaction tran)
            {
                IEnumerable<object> values = SqlMapper.Query(conn, type, sql, parameters, tran);
                value = (T?) values.SingleOrDefault();
            });
            return value;
        }

        protected string GetDataByScalarString(string sql, dynamic parameters = null)
        {
            if (IsChangeSqlString(sql))
                throw new Exception("sql语句错误,执行的是查询方法,但SQL语句包含更新代码,SQL语句:" + sql);
            string value = null;
            var type = typeof(string);
            NoLockInvoke(delegate(IDbConnection conn, IDbTransaction tran)
            {
                IEnumerable<object> values = SqlMapper.Query(conn, type, sql, parameters, tran);
                var singleOrDefault = values.SingleOrDefault();
                if (singleOrDefault != null) value = singleOrDefault.ToString();
            });
            return value;
        }

        /// <summary>
        ///     添加实体
        /// </summary>
        /// <param name="item"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected dynamic AddModel<T>(T item, IDbContext context = null) where T : class, new()
        {
            if (context == null) throw new Exception("dapper操作的context不可以为空");
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork == null) ||
                (currentUnitOfWork.Option == UnitOfWorkOption.Suppress))
            {
                var writeConnection = context.GetConnection();
                object obj2 = null;
                try
                {
                    writeConnection.Open();
                    obj2 = writeConnection.Insert(item, null, null);
                }
                finally
                {
                    writeConnection.Close();
                }
                return obj2;
            }
            {
                Func<IDbConnection, IDbTransaction, object, object> action =
                    (conn, tran, oitem) => conn.Insert((T) oitem, tran, null);
                var mapper = DapperExtensions.DapperExtensions.GetMap<T>(GetDataBaseType(context.GetConnection()));
                var properties = mapper.Properties;
                var propertyMap =
                    properties.Where(p => p.Name == "Id").SingleOrDefault(p => p.ColumnName.IndexOf('.') != -1);
                currentUnitOfWork.ChangeOperator(action, context, item, propertyMap);
                return null;
            }
        }

        #endregion
    }
}