using System.Data;

namespace ZilLion.Core.DatabaseWrapper.Dapper
{
    public interface IDbContext
    {
        string ContextName { get; }
        IDbConnection GetConnection();
    }
}