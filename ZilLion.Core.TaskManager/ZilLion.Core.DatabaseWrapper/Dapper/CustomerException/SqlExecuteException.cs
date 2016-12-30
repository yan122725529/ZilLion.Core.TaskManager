using System;
using System.Collections.Generic;

namespace ZilLion.Core.DatabaseWrapper.Dapper.CustomerException
{
    /// <summary>
    ///     存储过程功能异常
    /// </summary>
    [Serializable]
    public class SqlExecuteException : Exception
    {
        public bool HasCommit { get; set; }

        public SqlExecuteException():base()
        {
        }

        public SqlExecuteException(bool hasCommit)
        {
            HasCommit = hasCommit;
        }
    }
}