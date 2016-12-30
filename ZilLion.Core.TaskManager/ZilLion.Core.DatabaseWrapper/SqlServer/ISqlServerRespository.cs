using System.Collections.Generic;
using ZilLion.Core.DatabaseWrapper.Dapper;
using ZilLion.Core.DatabaseWrapper.Dapper.CustomerException;

namespace ZilLion.Core.DatabaseWrapper.SqlServer
{
    public interface ISqlServerRespository:IRespository
    {
        IList<LocalErrorModel> ErrorModels { get; set; }
    }
}