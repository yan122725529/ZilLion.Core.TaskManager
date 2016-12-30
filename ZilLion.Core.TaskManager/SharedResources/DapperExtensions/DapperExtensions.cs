using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;

namespace DapperExtensions
{
    /// <summary>
    ///     数据库类型 为了实现动态切数据库，add by yzy
    /// </summary>
    public enum DataBaseType
    {
        SqlServer,
        Sqlite,
        Oracle
    }

    public static class DapperExtensions
    {
        private static IDictionary<DataBaseType, IDapperExtensionsConfiguration> _dapperConfigDic =
            new Dictionary<DataBaseType, IDapperExtensionsConfiguration>();

        private static IDictionary<DataBaseType, IDapperImplementor> _dapperImplementorDic =
            new Dictionary<DataBaseType, IDapperImplementor>();

        private static readonly object _lock = new object();

        private static Func<IDapperExtensionsConfiguration, IDapperImplementor> _instanceFactory;

        static DapperExtensions()
        {
            InitConfig();
        }


        /// <summary>
        ///     Get or sets the Dapper Extensions Implementation Factory.
        /// </summary>
        public static Func<IDapperExtensionsConfiguration, IDapperImplementor> InstanceFactory
        {
            get
            {
                return _instanceFactory ??
                       (_instanceFactory = config => new DapperImplementor(new SqlGeneratorImpl(config)));
            }
            set { _instanceFactory = value; }
        }


        public static void InitConfig(IDictionary<DataBaseType, IDapperExtensionsConfiguration> config = null)
        {
            if (config == null)
                _dapperConfigDic = new Dictionary<DataBaseType, IDapperExtensionsConfiguration>
                {
                    {
                        DataBaseType.SqlServer,
                        new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(),
                            new SqlServerDialect())
                    },
                    {
                        DataBaseType.Oracle,
                        new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(),
                            new OracleDialect())
                    },
                    {
                        DataBaseType.Sqlite,
                        new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(),
                            new SqliteDialect())
                    }
                };
            else
                _dapperConfigDic = config;


            _dapperImplementorDic = new Dictionary<DataBaseType, IDapperImplementor>();
        }


        /// <summary>
        ///     根据数据库类型获取IDapperImplementor add by yzy 20161221
        /// </summary>
        /// <param name="dataBaseType"></param>
        /// <returns></returns>
        private static IDapperImplementor GetImplementor(DataBaseType dataBaseType = DataBaseType.SqlServer)
        {
            lock (_lock)
            {
                if (_dapperImplementorDic.ContainsKey(dataBaseType))
                    return _dapperImplementorDic[dataBaseType];
                var config = _dapperConfigDic[dataBaseType];
                var instance = InstanceFactory(config);
                _dapperImplementorDic.Add(dataBaseType, instance);
                return _dapperImplementorDic[dataBaseType];
            }
        }


        /// <summary>
        ///     Executes a query for the specified id, returning the data typed as per T
        /// </summary>
        public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null,
            int? commandTimeout = null, DataBaseType dataBaseType = DataBaseType.SqlServer) where T : class
        {
            var instance = GetImplementor(dataBaseType);
            var result = instance.Get<T>(connection, id, transaction, commandTimeout);
            return (T) result;
        }

        /// <summary>
        ///     Executes an insert query for the specified entity.
        /// </summary>
        public static void Insert<T>(this IDbConnection connection, IEnumerable<T> entities,
            IDbTransaction transaction = null, int? commandTimeout = null,
            DataBaseType dataBaseType = DataBaseType.SqlServer)
            where T : class
        {
            var instance = GetImplementor(dataBaseType);
            instance.Insert(connection, entities, transaction, commandTimeout);
        }

        /// <summary>
        ///     Executes an insert query for the specified entity, returning the primary key.
        ///     If the entity has a single key, just the value is returned.
        ///     If the entity has a composite key, an IDictionary&lt;string, object&gt; is returned with the key values.
        ///     The key value for the entity will also be updated if the KeyType is a Guid or Identity.
        /// </summary>
        public static dynamic Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null,
            int? commandTimeout = null, DataBaseType dataBaseType = DataBaseType.SqlServer) where T : class
        {
            var instance = GetImplementor(dataBaseType);


            return instance.Insert(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        ///     Executes an update query for the specified entity.
        /// </summary>
        public static bool Update<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null,
            int? commandTimeout = null, DataBaseType dataBaseType = DataBaseType.SqlServer) where T : class
        {
            var instance = GetImplementor(dataBaseType);
            return instance.Update(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        ///     Executes a delete query for the specified entity.
        /// </summary>
        public static bool Delete<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null,
            int? commandTimeout = null, DataBaseType dataBaseType = DataBaseType.SqlServer) where T : class
        {
            var instance = GetImplementor(dataBaseType);
            return instance.Delete(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        ///     Executes a delete query using the specified predicate.
        /// </summary>
        public static bool Delete<T>(this IDbConnection connection, object predicate, IDbTransaction transaction = null,
            int? commandTimeout = null, DataBaseType dataBaseType = DataBaseType.SqlServer) where T : class
        {
            var instance = GetImplementor(dataBaseType);
            return instance.Delete<T>(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        ///     Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// </summary>
        public static IEnumerable<T> GetList<T>(this IDbConnection connection, object predicate = null,
            IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null,
            bool buffered = false, DataBaseType dataBaseType = DataBaseType.SqlServer) where T : class
        {
            var instance = GetImplementor(dataBaseType);
            return instance.GetList<T>(connection, predicate, sort, transaction, commandTimeout, buffered);
        }

        /// <summary>
        ///     Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        ///     Data returned is dependent upon the specified page and resultsPerPage.
        /// </summary>
        public static IEnumerable<T> GetPage<T>(this IDbConnection connection, object predicate, IList<ISort> sort,
            int page, int resultsPerPage, IDbTransaction transaction = null, int? commandTimeout = null,
            bool buffered = false, DataBaseType dataBaseType = DataBaseType.SqlServer) where T : class
        {
            var instance = GetImplementor(dataBaseType);
            return instance.GetPage<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout,
                buffered);
        }

        /// <summary>
        ///     Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        ///     Data returned is dependent upon the specified firstResult and maxResults.
        /// </summary>
        public static IEnumerable<T> GetSet<T>(this IDbConnection connection, object predicate, IList<ISort> sort,
            int firstResult, int maxResults, IDbTransaction transaction = null, int? commandTimeout = null,
            bool buffered = false, DataBaseType dataBaseType = DataBaseType.SqlServer) where T : class
        {
            var instance = GetImplementor(dataBaseType);
            return instance.GetSet<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout,
                buffered);
        }

        /// <summary>
        ///     Executes a query using the specified predicate, returning an integer that represents the number of rows that match
        ///     the query.
        /// </summary>
        public static int Count<T>(this IDbConnection connection, object predicate, IDbTransaction transaction = null,
            int? commandTimeout = null, DataBaseType dataBaseType = DataBaseType.SqlServer) where T : class
        {
            var instance = GetImplementor(dataBaseType);
            return instance.Count<T>(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        ///     Executes a select query for multiple objects, returning IMultipleResultReader for each predicate.
        /// </summary>
        public static IMultipleResultReader GetMultiple(this IDbConnection connection, GetMultiplePredicate predicate,
            IDbTransaction transaction = null, int? commandTimeout = null,
            DataBaseType dataBaseType = DataBaseType.SqlServer)
        {
            var instance = GetImplementor(dataBaseType);
            return instance.GetMultiple(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        ///     Gets the appropriate mapper for the specified type T.
        ///     If the mapper for the type is not yet created, a new mapper is generated from the mapper type specifed by
        ///     DefaultMapper.
        /// </summary>
        public static IClassMapper GetMap<T>(DataBaseType dataBaseType = DataBaseType.SqlServer) where T : class
        {
            var instance = GetImplementor(dataBaseType);
            return instance.SqlGenerator.Configuration.GetMap<T>();
        }

        /// <summary>
        ///     Clears the ClassMappers for each type.
        /// </summary>
        public static void ClearCache(DataBaseType dataBaseType = DataBaseType.SqlServer)
        {
            var instance = GetImplementor(dataBaseType);
            instance.SqlGenerator.Configuration.ClearCache();
        }

        /// <summary>
        ///     Generates a COMB Guid which solves the fragmented index issue.
        ///     See: http://davybrion.com/blog/2009/05/using-the-guidcomb-identifier-strategy
        /// </summary>
        public static Guid GetNextGuid(DataBaseType dataBaseType = DataBaseType.SqlServer)
        {
            var instance = GetImplementor(dataBaseType);
            return instance.SqlGenerator.Configuration.GetNextGuid();
        }


        /// <summary>
        ///     createTable table
        /// </summary>
        public static bool CreateTempTable<T>(this IDbConnection connection, T entity, string tempdbname,
            short dbRowSn = 0, DataBaseType dataBaseType = DataBaseType.SqlServer) where T : class
        {
            var instance = GetImplementor(dataBaseType);
            return instance.CreateTempTable(connection, entity, tempdbname, dbRowSn);
        }
    }
}