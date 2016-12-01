using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace ZilLion.Core.DatabaseWrapper.MongoDB
{
    public class MongoObjectDb<T>
    {
        private static readonly Dictionary<string, MongoObjectDb<T>> _mongoDbCache =
            new Dictionary<string, MongoObjectDb<T>>();

        private readonly string _collectionname;
        private readonly string _dbname;
        private readonly MongoServer _msever;

      
        private static object _lockobject = new object();

        private MongoObjectDb(string collectionname, string dbname)
        {
            _collectionname = collectionname;
            _dbname = dbname;

            try
            {
                var connectionString = string.Format("mongodb://{0}/?safe=true", MongoDbSetting.ConnectionString);
                _msever = new MongoClient(connectionString).GetServer();
            }
            catch (Exception)
            {
                _msever = null;
            }
        }

        public static MongoObjectDb<T> GetMongoObjectDb(string dbname, string collectionname)
        {
            lock (_lockobject)
            {
                if (!_mongoDbCache.ContainsKey(dbname + collectionname))
                    _mongoDbCache.Add(dbname + collectionname, new MongoObjectDb<T>(dbname, collectionname));
                return _mongoDbCache[dbname + collectionname];
            }
        }

        /// <summary>
        ///     保存对象
        /// </summary>
        /// <returns></returns>
        public void SaveObjectToMongoDb(T item)
        {
            if (_msever == null) return;
            var db = _msever.GetDatabase(_dbname);
            var collection = db.GetCollection(_collectionname);
            collection.Insert(item);
        }
    }
}