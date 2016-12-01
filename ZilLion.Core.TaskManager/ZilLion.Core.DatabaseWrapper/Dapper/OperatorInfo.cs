using System;
using System.Data;
using DapperExtensions.Mapper;

namespace ZilLion.Core.DatabaseWrapper.Dapper
{
    internal class OperatorInfo
    {
        public Func<IDbConnection, IDbTransaction, object, object> func { get; set; }
        public IDbContext Context { get; set; }
        public object entity { get; set; }
        public IPropertyMap Map { get; set; }
    }

    internal class Map
    {
        public object id { get; set; }
        public string MapTablename { get; set; }
        public string MapTableCol { get; set; }
    }
}