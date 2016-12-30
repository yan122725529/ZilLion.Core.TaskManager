using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using DapperExtensions;
using DapperExtensions.Mapper;
using ZilLion.Core.DatabaseWrapper.Dapper;
using ZilLion.Core.DatabaseWrapper.Dapper.CustomerException;

namespace ZilLion.Core.DatabaseWrapper.SqlServer
{
    public class SqlServerRespository<T> : Respository<T>, ISqlServerRespository where T : class
    {
        public SqlServerRespository()
        {
        }

        public SqlServerRespository(IDbContext source)
        {
            Context = source;
        }

        public IList<LocalErrorModel> ErrorModels
        {
            get { return _errorModels; }
            set { _errorModels = value; }
        }

        #region 存储过程管道封装

        private readonly object _lockobject = new object();
        private string _oldTableName = string.Empty;
        private IList<LocalErrorModel> _errorModels=new List<LocalErrorModel>();

        /// <summary>
        ///     设置临时表名称
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="tempdbname"></param>
        protected void SetTempdbname<TEntity>(string tempdbname) where TEntity : class
        {
            var mapper = DapperExtensions.DapperExtensions.GetMap<TEntity>() as ClassMapper<TEntity>;
            if (mapper == null) return;
            _oldTableName = mapper.TableName;
            mapper.TableName = tempdbname;
        }

        /// <summary>
        ///     重置临时表名称
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        protected void RestoreTempdbname<TEntity>() where TEntity : class
        {
            var mapper = DapperExtensions.DapperExtensions.GetMap<TEntity>() as ClassMapper<TEntity>;
            if (mapper != null) mapper.TableName = _oldTableName;
        }

        /// <summary>
        ///     根据entity执行存储过程
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="insertfun"></param>
        /// <param name="procName"></param>
        /// <param name="tempdbname"></param>
        /// <param name="isInsertEntity"></param>
        /// <param name="parameters"></param>
        /// <param name="changedTableName"></param>
        /// <param name="dbCwclFs"></param>
        /// <param name="dbByRef"></param>
        /// <returns></returns>
        private IEnumerable<TEntity> InnerExecuteProc<TEntity>(Action<IDbConnection> insertfun, string procName,
            string tempdbname, bool isInsertEntity, object parameters, string changedTableName, short dbCwclFs = 0,
            short dbByRef = 1) where TEntity : class
        {
            IEnumerable<LocalErrorModel> listerrors = null;
            var hasLocalerror = false;
            var hascommint = false;
            Func<IDbConnection, IDbTransaction, object, object> action = null;
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork != null) && (currentUnitOfWork.Option != UnitOfWorkOption.Suppress)) return null;
            var writeConnection = Context.GetConnection();

            writeConnection.Open();
            insertfun(writeConnection);
            if (parameters == null) parameters = new object();
            using (var transaction = writeConnection.BeginTransaction())
            {
                try
                {
                    listerrors = SqlMapper.Query<LocalErrorModel>(writeConnection, procName, (dynamic)parameters,
                        transaction,
                        commandType: CommandType.StoredProcedure);

                    if ((listerrors != null) && listerrors.Any())
                    {
                        transaction.Rollback();
                        var first = listerrors.FirstOrDefault();
                        hasLocalerror = true;
                        throw new DbLocalErrException(first != null ? first.desc : string.Empty, null,
                            listerrors.ToList());
                    }

                    transaction.Commit();
                    hascommint = true;
                    if (changedTableName != null)
                        SetTempdbname<TEntity>(changedTableName);
                    if ((dbByRef == 1) && !string.IsNullOrEmpty(tempdbname))
                    {
                        var list = writeConnection.GetList<TEntity>(null, null, null, null, false);
                        return list;
                    }
                    RestoreTempdbname<TEntity>();
                }
                catch (Exception ex)
                {
                    if (ex is DbLocalErrException)
                    {
                        lock (_lockobject)
                        {
                            ErrorModels.Clear();
                        }
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (dbCwclFs)
                        {
                            case 0:
                                throw;
                            case 1:
                                lock (_lockobject)
                                {
                                    ErrorModels.Clear();

                                    if (listerrors != null)
                                        foreach (var err in listerrors)
                                            ErrorModels.Add(err);
                                }
                                break;
                        }
                    }
                    else
                    {
                        throw new SqlExecuteException(hascommint);
                    }
                }
                finally
                {
                    RestoreTempdbname<TEntity>();
                    writeConnection.Close();
                    if (!hasLocalerror)
                        ErrorModels.Clear();
                }
            }


            return null;
        }

        #region 手工调用存储过程

        /// <summary>
        ///     根据配置执行存储过程（参数为单条数据）
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="item">对象</param>
        /// <param name="procName">存储过程名称</param>
        /// <param name="tempdbname">临时表名称</param>
        /// <param name="isInsertEntity">是否插入实体</param>
        /// <param name="parameters">传入参数</param>
        /// <returns>返回执行结果</returns>
        protected IEnumerable<TEntity> ExecuteProcWithTemptable<TEntity>(TEntity item, string procName,
            string tempdbname, bool isInsertEntity = false, object parameters = null, short dbCwclFs = 0)
            where TEntity : class
        {
            if (item == null)
                return null;
            Action<IDbConnection> insertfun = writeConnection =>
            {
                if (string.IsNullOrEmpty(tempdbname)) return;
                writeConnection.CreateTempTable(item, tempdbname);
                if (tempdbname != null)
                    SetTempdbname<TEntity>(tempdbname);
                if (isInsertEntity)
                    writeConnection.Insert(item, null, null);
                RestoreTempdbname<TEntity>();
            };
            return InnerExecuteProc<TEntity>(insertfun, procName, tempdbname, isInsertEntity, parameters,
                tempdbname, dbCwclFs);
        }

        /// <summary>
        ///     根据配置执行存储过程(参数为多条数据)
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="procName">存储过程</param>
        /// <param name="tempdbname">临时表名称</param>
        /// <param name="isInsertEntity">是否插入实体</param>
        /// <param name="parameters">传入参数</param>
        /// <param name="items"></param>
        /// <param name="dbCwclFs"></param>
        /// <returns>返回执行结果</returns>
        protected IEnumerable<TEntity> ExecuteProcWithTemptableList<TEntity>(IList<TEntity> items, string procName,
            string tempdbname, bool isInsertEntity = false, object parameters = null, short dbCwclFs = 0)
            where TEntity : class
        {
            if (items == null)
                return null;
            Action<IDbConnection> insertfun = writeConnection =>
            {
                if (string.IsNullOrEmpty(tempdbname)) return;
                writeConnection.CreateTempTable(items.FirstOrDefault(), tempdbname);
                if (isInsertEntity)
                {
                    SetTempdbname<TEntity>(tempdbname);
                    foreach (var item in items)
                        writeConnection.Insert(item, null, null);
                    RestoreTempdbname<TEntity>();
                }
            };
            return InnerExecuteProc<TEntity>(insertfun, procName, tempdbname, isInsertEntity, parameters,
                tempdbname, dbCwclFs);
        }


        /// <summary>
        ///     准备存储过程输入临时表数据（多条数据）
        /// </summary>
        /// <typeparam name="TEntity[]"></typeparam>
        /// <returns></returns>
        public Action<IDbConnection> ExecuteProcAddParams<TEntity>(IEnumerable<TEntity> items, string tempdbname,
            bool isInsertEntity = false) where TEntity : class
        {
            if (items == null)
                return null;
            Action<IDbConnection> insertfun = writeConnection =>
            {
                if (string.IsNullOrEmpty(tempdbname)) return;
                SetTempdbname<TEntity>(tempdbname);
                var enumerable = items as TEntity[] ?? items.ToArray();
                writeConnection.CreateTempTable(enumerable.FirstOrDefault(), tempdbname);
                if (isInsertEntity)
                    foreach (var item in enumerable)
                        writeConnection.Insert(item, null, null);
                RestoreTempdbname<TEntity>();
            };
            return insertfun;
        }

        /// <summary>
        ///     准备存储过程输入临时表数据（单条数据）
        /// </summary>
        /// <typeparam name="TEntity[]"></typeparam>
        /// <returns></returns>
        public Action<IDbConnection> ExecuteProcAddParam<TEntity>(TEntity item, string tempdbname,
            bool isInsertEntity = false) where TEntity : class
        {
            if (item == null)
                return null;
            Action<IDbConnection> insertfun = writeConnection =>
            {
                if (string.IsNullOrEmpty(tempdbname)) return;
                SetTempdbname<TEntity>(tempdbname);
                writeConnection.CreateTempTable(item, tempdbname);
                if (isInsertEntity)
                    writeConnection.Insert(item, null, null);
                RestoreTempdbname<TEntity>();
            };
            return insertfun;
        }


        /// <summary>
        ///     执行存储过程，多临时表输入，多临时表输出
        /// </summary>
        /// <param name="insertfuns"></param>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <param name="returnlist"></param>
        /// <param name="dbCwclFs">0-raise报错；1-通过rserr返回。默认0</param>
        public void ExecuteProc(List<Action<IDbConnection>> insertfuns, string procName, object parameters = null,
            Action<IDbConnection> returnlist = null, short dbCwclFs = 0)
        {
            IList<LocalErrorModel> listerrors = null;
            var hasLocalerror = false;
            var hascommint = false;
            var currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if ((currentUnitOfWork != null) && (currentUnitOfWork.Option != UnitOfWorkOption.Suppress)) return;
            var writeConnection = Context.GetConnection();
            writeConnection.Open();
            if (insertfuns != null)
                foreach (var insertfun in insertfuns)
                    insertfun(writeConnection);

            if (parameters == null) parameters = new object();


            using (var transaction = writeConnection.BeginTransaction())
            {
                try
                {
                    listerrors = SqlMapper.Query<LocalErrorModel>(writeConnection, procName, (dynamic)parameters,
                        transaction,
                        commandType: CommandType.StoredProcedure);

                    if ((listerrors != null) && listerrors.Any())
                    {
                        transaction.Rollback();
                        var first = listerrors.FirstOrDefault();
                        hasLocalerror = true;
                        throw new DbLocalErrException(first != null ? first.desc : string.Empty, null,
                            listerrors.ToList());
                    }

                    transaction.Commit();
                    hascommint = true;
                    returnlist?.Invoke(writeConnection);
                }
                catch (Exception ex)
                {
                    if (ex is DbLocalErrException)
                    {
                        lock (_lockobject)
                        {
                            ErrorModels.Clear();
                        }
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (dbCwclFs)
                        {
                            case 0:
                                throw;
                            case 1:
                                lock (_lockobject)
                                {
                                    ErrorModels.Clear();

                                    if (listerrors != null)
                                        foreach (var err in listerrors)
                                            ErrorModels.Add(err);
                                }
                                break;
                        }
                    }
                    else
                    {
                        throw new SqlExecuteException(hascommint);
                    }
                }
                finally
                {
                    
                 
                    if (!hasLocalerror)
                        ErrorModels.Clear();
                    writeConnection.Close();
                }
            }
    
        }

        #endregion

        #endregion
    }
}