using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ZilLion.Core.Unities.UnitiesMethods.WebApi.Http
{
    public  class DigestHelper
    {
        public static string Md5Converter(string str)
        {
            var md5 = new MD5Cng();
            var data = md5.ComputeHash(Encoding.Default.GetBytes(str));
            var pwdmd5 = data.Aggregate(string.Empty, (current, t) => current + t.ToString("x2"));
            return pwdmd5;
        }
    }
}
