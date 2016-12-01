using System.ComponentModel;

namespace ZilLion.Core.DatabaseWrapper.Dapper
{
    public interface IValidationErrorInfo : IDataErrorInfo
    {
        bool IsEnableRuleValidate { get; set; }
        string GetErrors(bool customerValidateable=true);

       
    }
}