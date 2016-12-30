using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace ZilLion.Core.DatabaseWrapper.Dapper
{
    public interface IValidationErrorInfo : IDataErrorInfo
    {
        bool IsEnableRuleValidate { get; set; }
        string GetErrors(bool customerValidateable=true);
        IDictionary<PropertyInfo,string> GetErrorsDic(bool customerValidateable = true);
    }
}