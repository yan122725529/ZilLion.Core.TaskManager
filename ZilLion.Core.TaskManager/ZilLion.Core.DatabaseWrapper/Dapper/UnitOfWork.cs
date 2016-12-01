using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DapperExtensions.Mapper;

namespace ZilLion.Core.DatabaseWrapper.Dapper
{
    /// <summary>
    ///     事务的封装
    /// </summary>
    public class UnitOfWork : IDisposable
    {
        private static readonly object lockObj = new object();
        //[ThreadStatic]
        private static ConcurrentStack<UnitOfWork> unitOfWorkStack;
        private readonly bool isShadow;
        private readonly List<OperatorInfo> list;
        private readonly List<Map> Maplist;

        public UnitOfWork() : this(UnitOfWorkOption.Required)
        {
        }

        public UnitOfWork(UnitOfWorkOption option)
        {
            list = new List<OperatorInfo>();
            Maplist = new List<Map>();
            isShadow = false;
            Option = option;
            if (Option == UnitOfWorkOption.Required)
            {
                var currentUnitOfWork = GetCurrentUnitOfWork();
                if (currentUnitOfWork == null)
                {
                    list = new List<OperatorInfo>();
                }
                else
                {
                    list = currentUnitOfWork.list;
                    isShadow = true;
                }
            }
            UnitOfWorkStack.Push(this);
        }

        public UnitOfWorkOption Option { get; private set; }

        /// <summary>
        ///     事务堆栈
        /// </summary>
        private static ConcurrentStack<UnitOfWork> UnitOfWorkStack
        {
            get
            {
                if (unitOfWorkStack == null)
                {
                    lock (lockObj)
                    {
                        if (unitOfWorkStack == null)
                        {
                            unitOfWorkStack = new ConcurrentStack<UnitOfWork>();
                        }
                    }
                }
                return unitOfWorkStack;
            }
        }

        public void Dispose()
        {
            if (!isShadow)
            {
                list.Clear();
            }
            var currentUnitOfWork = GetCurrentUnitOfWork();
            if (currentUnitOfWork == this)
            {
                UnitOfWorkStack.TryPop(out currentUnitOfWork);
            }
        }

        public void ChangeOperator(Func<IDbConnection, IDbTransaction, object, object> action, IDbContext context,
            dynamic entity, IPropertyMap map = null)
        {
            var item = new OperatorInfo
            {
                func = action,
                Context = context,
                entity = entity,
                Map = map
            };
            list.Add(item);
        }

        public void ChangeOperator(Func<IDbConnection, IDbTransaction, object, object> action, IDbContext context)
        {
            var item = new OperatorInfo
            {
                func = action,
                Context = context
            };
            list.Add(item);
        }

        private void Commit()
        {
            IDbConnection connection;
            switch ((from p in list select p.Context.ContextName).Distinct<string>().Count<string>())
            {
                case 0:
                    break;

                case 1:
                    using (connection = list.First().Context.GetConnection())
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                foreach (var info in list)
                                {
                                    var entity = MapForeignKey(info);
                                    var id = info.func(connection, transaction, entity);
                                    if (info.Map != null)
                                    {
                                        string[] strs = info.Map.ColumnName.Split('.');
                                        var map = new Map {id = id, MapTablename = strs[0], MapTableCol = strs[1]};
                                        Maplist.Add(map);
                                    }
                                }
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                            finally
                            {
                                connection.Close();
                                list.Clear();
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        ///     处理外键ID
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private object MapForeignKey(OperatorInfo info)
        {
            if (info.entity == null)
                return null;
            var map = Maplist.SingleOrDefault(p => p.MapTablename == info.entity.GetType().Name);
            if (map == null)
                return info.entity;
            try
            {
                var pi =
                    info.entity.GetType().GetProperties().SingleOrDefault(p => p.Name == map.MapTableCol);
                if (pi != null)
                {
                    pi.SetValue(info.entity, map.id, null);
                }
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("{0} mapping for property {1} failed.", map.MapTablename,
                    map.MapTableCol));
            }
            return info.entity;
        }

        /// <summary>
        ///     提交事务
        /// </summary>
        public void Complete()
        {
            var currentUnitOfWork = GetCurrentUnitOfWork();
            if (currentUnitOfWork != this)
            {
                throw new Exception("当前提交的事务内还有未完成的其他事务,请先完成内部的事务");
            }
            if ((Option == UnitOfWorkOption.RequiresNew) || ((Option == UnitOfWorkOption.Required) && !isShadow))
            {
                Commit();
            }
            UnitOfWorkStack.TryPop(out currentUnitOfWork);
            Dispose();
        }

        public static UnitOfWork GetCurrentUnitOfWork()
        {
            UnitOfWork result = null;
            UnitOfWorkStack.TryPeek(out result);
            return result;
        }

        public void SetCurrentUnitOfWork(UnitOfWork unitOfWork)
        {
            UnitOfWorkStack.Push(unitOfWork);
        }
    }
}