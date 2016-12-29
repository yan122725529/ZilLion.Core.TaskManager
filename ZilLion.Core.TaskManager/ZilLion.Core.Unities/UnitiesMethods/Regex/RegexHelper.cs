using System.Linq;
using System.Text.RegularExpressions;

namespace ZilLion.Core.Unities.UnitiesMethods.Regex
{
    public class RegexHelper
    {
        /// <summary>
        /// 是否手机号码
        /// </summary>
        /// <param name="input">要判断的手机号码</param>
        /// <returns></returns>
        public static bool IsMobile(string input)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(input, @"^[1][1-9]\d{9}$", RegexOptions.IgnoreCase))
                return false;
            return input.Length == 11 && (input.StartsWith("13") || input.StartsWith("14") || input.StartsWith("15") || input.StartsWith("18"));
        }

        /// <summary>
        /// 是否e-mail
        /// </summary>
        /// <param name="input">要判断的字符串</param>
        /// <returns></returns>
        public static bool IsEmail(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            const string emailReg = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";

            return System.Text.RegularExpressions.Regex.IsMatch(input, emailReg, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否中文汉字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsChinese(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var rx = new System.Text.RegularExpressions.Regex("^[\u4e00-\u9fa5]$");
            return input.All(t => rx.IsMatch(t.ToString()));
        }

        /// <summary>
        /// 是否IP地址
        /// </summary>
        /// <returns></returns>
        public static bool IsIpAddress(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length < 7 || input.Length > 15)
                return false;

            var regex = new System.Text.RegularExpressions.Regex(@"^([1-9]?\d|1\d\d|2[0-4]\d|25[0-5])\.([1-9]?\d|1\d\d|2[0-4]\d|25[0-5])\.([1-9]?\d|1\d\d|2[0-4]\d|25[0-5])\.([1-9]?\d|1\d\d|2[0-4]\d|25[0-5])$", RegexOptions.IgnoreCase);
            return regex.IsMatch(input);
        }
    }
}
