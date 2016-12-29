using System;
using System.Management;
using Microsoft.Win32;

namespace ZilLion.Core.Unities.UnitiesMethods
{
    public static class SysHelper
    {
        static SysHelper()
        {
            OsProductName = GetOsName();
            ComputerMacAddress = GetMacAddress();
            ComputerName = Environment.MachineName.ToLower();
            HasFramework40 = CheckFramework();
        }

        //public static string VersionName { get; private set; }

        public static string ComputerName { get; private set; }


        public static string OsProductName { get; private set; }
        public static string ComputerMacAddress { get; private set; }


        public static bool HasFramework40 { get; private set; }

        private static string GetOsName()
        {
            try
            {
                var rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");
                if (rk != null)
                {
                    var oSname = rk.GetValue("ProductName").ToString();
                    rk.Close();
                    return oSname;
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return @"Can't find LocalMachine\Software\Microsoft\Windows NT\CurrentVersion\ProductName";
            }
        }

        private static bool CheckFramework()
        {
            return CheckInstallFramework(@"Software\Microsoft\NET Framework Setup\NDP\v4\Client") ||
                   CheckInstallFramework(@"Software\Microsoft\NET Framework Setup\NDP\v4\Full");
        }

        private static string GetMacAddress()
        {
            try
            {
                //获取网卡硬件地址
                var mac = "";
                var mc = new
                    ManagementClass("Win32_NetworkAdapterConfiguration");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    if (!(bool) mo["IPEnabled"]) continue;
                    mac = mo["MacAddress"].ToString();
                    break;
                }
                moc = null;
                mc = null;
                return mac;
            }
            catch
            {
                return null;
            }
            finally
            {
            }
        }

        private static bool CheckInstallFramework(string registryPath)
        {
            var framework = Registry.LocalMachine.OpenSubKey(registryPath);
            return framework != null && CheckInstallValue(framework);
        }

        private static bool CheckInstallValue(RegistryKey framework)
        {
            var fullValue = framework.GetValue("Install");
            return fullValue != null && fullValue.ToString() == "1";
        }

        #region Registry ZilLionClient

        public static string GetRegistryCurrentUserOfZilLionClientValue(string key)
        {
            return GetRegistryCurrentUserOfValue("SOFTWARE\\ZilLionClient", key);
        }

        public static void SetRegistryCurrentUserOfZilLionClientValue(string key, string value)
        {
            SetRegistryCurrentUserOfValue("SOFTWARE\\ZilLionClient", key, value);
        }

        public static void DeleteRegistryCurrentUserOfZilLionClientValue(string key)
        {
            DeleteRegistryCurrentUserOfValue("SOFTWARE\\ZilLionClient", key);
        }

        #endregion

        #region Registry Custom SubKey

        /// <summary>
        ///     找不到注册表记录，直接忽略
        /// </summary>
        /// <param name="subKey"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetRegistryCurrentUserOfValueIgnoerEx(string subKey, string key)
        {
            var versionKey = Registry.CurrentUser.OpenSubKey(subKey, true);
            var versionValue = versionKey?.GetValue(key);
            return string.IsNullOrEmpty(versionValue?.ToString().Trim()) ? string.Empty : versionValue.ToString();
        }

        /// <summary>
        ///     找不到注册表记录，报错
        /// </summary>
        /// <param name="subKey"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetRegistryCurrentUserOfValue(string subKey, string key)
        {
            var versionKey = Registry.CurrentUser.OpenSubKey(subKey, true);

            if (versionKey == null)
                throw new NullReferenceException($"Can't find Registry.CurrentUser.OpenSubKey of {subKey}");

            var versionValue = versionKey.GetValue(key);
            return versionValue?.ToString() ?? string.Empty;
        }

        public static void SetRegistryCurrentUserOfValue(string subKey, string key, string value)
        {
            var subValue = Registry.CurrentUser.CreateSubKey(subKey);
            subValue?.SetValue(key, value);
        }

        public static void DeleteRegistryCurrentUserOfValue(string subKey, string key)
        {
            var subValue = Registry.CurrentUser.CreateSubKey(subKey);
            subValue?.DeleteValue(key, false);
        }

        #endregion
    }
}