using System.Collections.Generic;

namespace ZilLion.Core.DatabaseWrapper.Dapper.CustomerException
{
    public class DbLocalErrException : System.Exception
    {
         public DbLocalErrException(string message) : base(message)
        {
        }

         public DbLocalErrException(string message, System.Exception innerException)
             : base(message, innerException)
        {
        }
         public DbLocalErrException(string message, System.Exception innerException, IList<LocalErrorModel>  localErrorModels)
             : base(message, innerException)
         {
             LocalErrorModels = localErrorModels;
         }
        public IList<LocalErrorModel> LocalErrorModels { get; set; }
    }
}