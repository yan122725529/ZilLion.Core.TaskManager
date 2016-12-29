using System;

namespace ZilLion.Core.Unities.UnitiesMethods
{
    public static class PingyinHelper
    {
        /// <summary>
        ///     计算字符串的拼音码
        /// </summary>
        /// <param name="strFrom">需要计算的字符串</param>
        /// <param name="len"></param>
        /// <returns>计算后的拼音码</returns>
        public static string GetPymFromStr(string strFrom,int len=6)
        {
            if (string.IsNullOrEmpty(strFrom)) return "";
            var r = string.Empty;
            foreach (var chr in strFrom)
            {
                try
                {
                    //var chineseChar = new ChineseChar(chr);
                    //因为汉字可能有多个读音，，这里是一个集合 
                    //var t = chineseChar.Pinyins.FirstOrDefault();
                    //if (t.IsNotNullOrEmpty())
                    //{
                    //    r += t.Substring(0, 1);
                    //}
                    //else
                    //{
                    //    r += chr;
                    //}
                }
                catch (Exception)
                {
                    r += chr;
                }
            }

            return r.Length > len ? r.Substring(0, len) : r;
        }
    }
}