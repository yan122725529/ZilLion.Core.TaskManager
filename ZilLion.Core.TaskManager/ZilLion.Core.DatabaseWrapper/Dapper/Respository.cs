using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using DapperExtensions;
using DapperExtensions.Mapper;
using Oracle.ManagedDataAccess.Client;

namespace ZilLion.Core.DatabaseWrapper.Dapper
{
    /// <summary>
    ///     Dapper基本操作的封装，仓库
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class Respository<TEntity> : IRespository where TEntity : class
    {
        protected IDbContext Context;


        protected DataBaseType CurrentDataBaseType;

        public Respository()
        {

        }

        public Respository(IDbContext source)
        {
            Context = source;
            GetDataBaseType();
        }

        private void GetDataBaseType()
        {
            CurrentDataBaseType = DataBaseType.SqlServer;//默认
            var sqlconnection = Context.GetConnection();
            if (sqlconnection == null) return;
            if (sqlconnection is SqlConnection)
                CurrentDataBaseType = DataBaseType.SqlServer;
            if (sqlconnection is SQLiteConnection)
                CurrentDataBaseType = DataBaseType.Sqlite;

            if (sqlconnection is OracleConnection)
                CurrentDataBaseType = DataBaseType.Oracle;
        }


        public ClassMapper<TEntity> GetMap()
        {

            return DapperExtensions.DapperExtensions.GetMap<TEntity>(CurrentDataBaseType) as ClassMapper<TEntity>;
        }


        /// <summary>
        ///     添加实体
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected dynamic Add(TEntity item)
        {
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork == null) ||
                (currentUnitOfWork.Option == UnitOfWorkOption.Suppress))
            {
                var writeConnection = Context.GetConnection();
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
                    (conn, tran, oitem) => conn.Insert((TEntity)oitem, tran, null);
                var mapper = DapperExtensions.DapperExtensions.GetMap<TEntity>(CurrentDataBaseType);
                var properties = mapper.Properties;
                var propertyMap =
                    properties.Where(p => p.Name == "Id").SingleOrDefault(p => p.ColumnName.IndexOf('.') != -1);
                currentUnitOfWork.ChangeOperator(action, Context, item, propertyMap);
                return null;
            }
        }


        /// <summary>
        ///     获取所有实体
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<TEntity> GetAll()
        {
            IEnumerable<TEntity> list = null;
            NoLockInvoke(
                delegate (IDbConnection conn, IDbTransaction tran)
                {
                    list = conn.GetList<TEntity>(null, null, tran, null, false);
                });
            return list;
        }


        /// <summary>
        ///     根据id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected TEntity GetEntityById(dynamic id)
        {
            var entity = default(TEntity);
            NoLockInvoke(
                delegate (IDbConnection conn, IDbTransaction tran)
                {
                    entity = (TEntity)DapperExtensions.DapperExtensions.Get<TEntity>(conn, id, tran, null, CurrentDataBaseType);
                });
            return entity;
        }


        protected IEnumerable<TEntity> GetList(string sql, dynamic parameters = null)
        {
            if (IsChangeSqlString(sql))
                throw new Exception("sql语句错误,执行的是查询方法,但SQL语句包含更新代码,SQL语句:" + sql);
            IEnumerable<TEntity> list = null;
            NoLockInvoke(
                delegate (IDbConnection conn, IDbTransaction tran)
                {
                    list = (IEnumerable<TEntity>)SqlMapper.Query<TEntity>(conn, sql, parameters, tran);
                });
            return list;
        }

        /// <summary>
        ///     判断是否是改变数据库的SQL dcl
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private bool IsChangeSqlString(string sql)
        {
            sql = sql.Trim().ToLower();
            return Regex.IsMatch(sql, "^(update|delete|insert|create|alter|drop|truncate)") ||
                   Regex.IsMatch(sql, @"^(select)\s+(into)\s");
        }

        /// <summary>
        ///     更新实体
        /// </summary>
        /// <param name="item"></param>
        protected void Modify(TEntity item)
        {
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork == null) ||
                (currentUnitOfWork.Option == UnitOfWorkOption.Suppress))
            {
                var writeConnection = Context.GetConnection();
                try
                {
                    writeConnection.Open();
                    writeConnection.Update(item, null, null);
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
                currentUnitOfWork.ChangeOperator(action, Context);
            }
        }

        /// <summary>
        ///     删除实体
        /// </summary>
        /// <param name="item"></param>
        protected void Remove(TEntity item)
        {
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork == null) ||
                (currentUnitOfWork.Option == UnitOfWorkOption.Suppress))
            {
                var writeConnection = Context.GetConnection();
                try
                {
                    writeConnection.Open();
                    writeConnection.Delete(item, null, null);
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
                currentUnitOfWork.ChangeOperator(action, Context);
            }
        }

        protected void NoLockInvoke(Action<IDbConnection, IDbTransaction> action)
        {
            using (var connection = Context.GetConnection())
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
    }
}