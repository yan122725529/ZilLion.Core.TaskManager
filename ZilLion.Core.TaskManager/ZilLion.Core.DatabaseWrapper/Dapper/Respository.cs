using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using DapperExtensions;
using DapperExtensions.Mapper;

namespace ZilLion.Core.DatabaseWrapper.Dapper
{
    /// <summary>
    ///     Dapper基本操作的封装，仓库
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class Respository<TEntity> : IRespository where TEntity : class
    {
        protected IDbContext Context;

        public Respository()
        {

        }

        public Respository(IDbContext source)
        {
            Context = source;
        }


        public ClassMapper<TEntity> GetMap()
        {
            return DapperExtensions.DapperExtensions.GetMap<TEntity>() as ClassMapper<TEntity>;
        }


        /// <summary>
        ///     添加实体
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected dynamic Add(TEntity item)
        {
            Func<IDbConnection, IDbTransaction, object, object> action = null;
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
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    writeConnection.Close();
                }
                return obj2;
            }
            {
                action =
                    (conn, tran, oitem) => conn.Insert((TEntity) oitem, tran, null);
                var mapper = DapperExtensions.DapperExtensions.GetMap<TEntity>();
                var properties = mapper.Properties;
                var propertyMap =
                    properties.Where(p => p.Name == "Id").SingleOrDefault(p => p.ColumnName.IndexOf('.') != -1);
                currentUnitOfWork.ChangeOperator(action, Context, item, propertyMap);
                return null;
            }
            return null;
        }

        /// <summary>
        ///     添加实体
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected dynamic AddModel<T>(T item) where T : class, new()
        {
            Func<IDbConnection, IDbTransaction, object, object> action = null;
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
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    writeConnection.Close();
                }
                return obj2;
            }
            {
                action =
                    (conn, tran, oitem) => conn.Insert((T) oitem, tran, null);
                var mapper = DapperExtensions.DapperExtensions.GetMap<T>();
                var properties = mapper.Properties;
                var propertyMap =
                    properties.Where(p => p.Name == "Id").SingleOrDefault(p => p.ColumnName.IndexOf('.') != -1);
                currentUnitOfWork.ChangeOperator(action, Context, item, propertyMap);
                return null;
            }
            return null;
        }

        /// <summary>
        ///     执行SQL,映射实体
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        protected void Execute(string sql, dynamic parameters = null)
        {
            Func<IDbConnection, IDbTransaction, object, object> action = null;
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork == null) ||
                (currentUnitOfWork.Option == UnitOfWorkOption.Suppress))
            {
                var writeConnection = Context.GetConnection();
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
            currentUnitOfWork.ChangeOperator(action, Context);
        }

        /// <summary>
        ///     获取所有实体
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<TEntity> GetAll()
        {
            IEnumerable<TEntity> list = null;
            NoLockInvoke(
                delegate(IDbConnection conn, IDbTransaction tran)
                {
                    list = conn.GetList<TEntity>(null, null, tran, null, false);
                });
            return list;
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
        ///     获取单个标量值
        /// </summary>
        /// <typeparam name="TU"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected TU? GetDataByScalar<TU>(string sql, dynamic parameters = null) where TU : struct
        {
            if (IsChangeSqlString(sql))
                throw new Exception("sql语句错误,执行的是查询方法,但SQL语句包含更新代码,SQL语句:" + sql);
            TU? value = null;
            var type = typeof(TU);
            NoLockInvoke(delegate(IDbConnection conn, IDbTransaction tran)
            {
                IEnumerable<object> values = SqlMapper.Query(conn,  sql, parameters, tran);
                value = (TU?) values.SingleOrDefault();
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
                IEnumerable<object> values = SqlMapper.Query(conn,  sql, parameters, tran);
                var singleOrDefault = values.SingleOrDefault();
                if (singleOrDefault != null) value = singleOrDefault.ToString();
            });
            return value;
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
                delegate(IDbConnection conn, IDbTransaction tran)
                {
                    entity = (TEntity) DapperExtensions.DapperExtensions.Get<TEntity>(conn, id, tran);
                });
            return entity;
        }

        /// <summary>
        ///     根据SQL获取实体列表
        /// </summary>
        /// <typeparam name="TU"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TU> GetList<TU>(string sql, dynamic parameters = null)
        {
            if (IsChangeSqlString(sql))
                throw new Exception("sql语句错误,执行的是查询方法,但SQL语句包含更新代码,SQL语句:" + sql);
            IEnumerable<TU> list = null;
            NoLockInvoke(
                delegate(IDbConnection conn, IDbTransaction tran)
                {
                    list = (IEnumerable<TU>) SqlMapper.Query<TU>(conn, sql, parameters, tran);
                });
            return list;
        }

        protected IEnumerable<TEntity> GetList(string sql, dynamic parameters = null)
        {
            if (IsChangeSqlString(sql))
                throw new Exception("sql语句错误,执行的是查询方法,但SQL语句包含更新代码,SQL语句:" + sql);
            IEnumerable<TEntity> list = null;
            NoLockInvoke(
                delegate(IDbConnection conn, IDbTransaction tran)
                {
                    list = (IEnumerable<TEntity>) SqlMapper.Query<TEntity>(conn, sql, parameters, tran);
                });
            return list;
        }

        /// <summary>
        ///     判断是否是改变数据库的SQL dcl
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected bool IsChangeSqlString(string sql)
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
            Func<IDbConnection, IDbTransaction, object, object> action = null;
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork == null) ||
                ((currentUnitOfWork != null) && (currentUnitOfWork.Option == UnitOfWorkOption.Suppress)))
            {
                var writeConnection = Context.GetConnection();
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
            else if (currentUnitOfWork != null)
            {
                if (action == null)
                    action = (conn, tran, entity) => conn.Update(item, tran, null);
                currentUnitOfWork.ChangeOperator(action, Context);
            }
        }

        /// <summary>
        ///     更新实体
        /// </summary>
        /// <param name="item"></param>
        protected void ModifyModel<T>(T item) where T : class, new()
        {
            Func<IDbConnection, IDbTransaction, object, object> action = null;
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork == null) ||
                ((currentUnitOfWork != null) && (currentUnitOfWork.Option == UnitOfWorkOption.Suppress)))
            {
                var writeConnection = Context.GetConnection();
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
            else if (currentUnitOfWork != null)
            {
                if (action == null)
                    action = (conn, tran, entity) => conn.Update(item, tran, null);
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
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        ///     删除实体
        /// </summary>
        /// <param name="item"></param>
        protected void Remove(TEntity item)
        {
            Func<IDbConnection, IDbTransaction, object, object> action = null;
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork == null) ||
                ((currentUnitOfWork != null) && (currentUnitOfWork.Option == UnitOfWorkOption.Suppress)))
            {
                var writeConnection = Context.GetConnection();
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
            else if (currentUnitOfWork != null)
            {
                if (action == null)
                    action = (conn, tran, entity) => conn.Delete(item, tran, null);
                currentUnitOfWork.ChangeOperator(action, Context);
            }
        }

        protected void RemoveModel<T>(T item) where T : class, new()
        {
            Func<IDbConnection, IDbTransaction, object, object> action = null;
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork == null) ||
                ((currentUnitOfWork != null) && (currentUnitOfWork.Option == UnitOfWorkOption.Suppress)))
            {
                var writeConnection = Context.GetConnection();
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
            else if (currentUnitOfWork != null)
            {
                if (action == null)
                    action = (conn, tran, entity) => conn.Delete(item, tran, null);
                currentUnitOfWork.ChangeOperator(action, Context);
            }
        }
    }
}