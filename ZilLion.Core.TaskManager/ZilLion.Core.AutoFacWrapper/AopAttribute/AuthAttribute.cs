using System;

namespace ZilLion.Core.AutoFacWrapper.AopAttribute
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class AuthAttribute : Attribute
    {
        //需要验证的权限类型
        public AuthType AuthType { get; set; }
    }
    /// <summary>
    /// 权限枚举
    /// </summary>
    public enum AuthType
    {
        New = 1,
        Modify = 2,
        
    }
}