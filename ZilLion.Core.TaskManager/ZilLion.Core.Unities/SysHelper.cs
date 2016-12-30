using System;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Win32;

namespace ZilLion.Core.TaskManager.Unities
{
    /// <summary>
    ///     Computer Information
    /// </summary>
    public class Computer
    {
        static Computer()
        {
            CpuName = GetCpuName();
            IpAddress = GetIp();
            MacAddress = GetMacAddress();
            TotalPhysicalMemory = GetTotalPhysicalMemory();
            DisplayCard = GetDisplayCard();
            CsdVersion = GetCsdVersion();
            OsBit = GetOsBit();
            Resolution = GetVideoModeDescription();
            SystemAccountLevel = GetSystemAccountLevel();
            LanguageName = GetLanguage();
            ComputerName = SysHelper.ComputerName;
            OsProductName = $"{SysHelper.OsProductName} {CsdVersion}";

            #region 检测防火墙

            //在xp中无法检测
            if (!OsProductName.ToLower().Contains("xp"))
            {
                NetFwDomain = "0";
                NetFwPublic = "0";
                NetFwPrivate = "0";
            }

            #endregion

            #region IsAdmin

            //var identity = WindowsIdentity.GetCurrent();
            //var principal = new WindowsPrincipal(identity);
            //AdminFlag = principal.IsInRole(WindowsBuiltInRole.Administrator) ? "1" : "0";
            AdminFlag = GetAdminFlag();

            #endregion
        }

        //CPU名称
        public static string CpuName { get; private set; }
        //MAC地址

        public static string MacAddress { get; private set; }
        //IP
        public static string IpAddress { get; private set; }
        //物理内存
        public static string TotalPhysicalMemory { get; private set; } //单位：M
        //显卡
        public static string DisplayCard { get; private set; }
        //sp3,sp2
        public static string CsdVersion { get; }
        //32bit,64bit
        public static string OsBit { get; private set; }

        //客户端唯一Id
        public static string ClientId { get; private set; }

        //电脑用户名称
        public static string ComputerName { get; private set; }
        //操作系统名称（windows xp sp3）
        public static string OsProductName { get; }
        //客户端版本号
        public static string ClientVersionNumber { get; private set; }
        //客户端渠道来源
        public static string SourceMark { get; private set; }
        //是否已装Framework4.0
        public static string FkFlag { get; private set; }
        //域配置防火墙
        public static string NetFwDomain { get; private set; }
        //公共配置防火墙
        public static string NetFwPublic { get; private set; }
        //私有配置防火墙
        public static string NetFwPrivate { get; private set; }
        //是否管理员
        public static string AdminFlag { get; private set; }
        // 用户账户控制级别，由高到低
        public static string SystemAccountLevel { get; private set; }
        //分辨率
        public static string Resolution { get; private set; }
        //操作系统语言
        public static string LanguageName { get; private set; }
        //OEM信息（雨林木风、番茄花园）
        public static string OemInfo { get; private set; }

        private static string GetIp()
        {
            var name = Dns.GetHostName();
            var ipadrlist = Dns.GetHostAddresses(name);
            var builder = new StringBuilder();
            foreach (var ip in ipadrlist)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    builder.Append($"{ip};");


            return builder.ToString();
        }

        private static string GetAdminFlag()
        {
            return RunCmd("net localgroup administrators |find \"%username%\"")
                       .IndexOf(Environment.UserName, StringComparison.Ordinal) >= 0
                ? "1"
                : "0";
        }

        private static string RunCmd(string cmd)
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = "/c " + cmd,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            p.Start();
            return p.StandardOutput.ReadToEnd();
        }


        private static string GetSystemAccountLevel()
        {
            string str = null;
            var policies =
                Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
            if (policies == null)
                str = null;
            if (policies == null) return null;
            var consentPromptBehaviorAdmin = policies.GetValue("ConsentPromptBehaviorAdmin");
            var enableLua = policies.GetValue("PromptOnSecureDesktop");
            var promptOnSecureDesktop = policies.GetValue("PromptOnSecureDesktop");
            if ((consentPromptBehaviorAdmin == null) || (enableLua == null) || (promptOnSecureDesktop == null))
                str = null;
            else
            {
                if ((consentPromptBehaviorAdmin.ToString() == "2") && (enableLua.ToString() == "1") &&
                    (promptOnSecureDesktop.ToString() == "1"))
                    str = "1";
                else if ((consentPromptBehaviorAdmin.ToString() == "5") && (enableLua.ToString() == "1") &&
                         (promptOnSecureDesktop.ToString() == "1"))
                    str = "2";
                else if ((consentPromptBehaviorAdmin.ToString() == "5") && (enableLua.ToString() == "1") &&
                         (promptOnSecureDesktop.ToString() == "0"))
                    str = "3";
                else if (enableLua.ToString() == "0")
                    str = "4";
                else
                    str = null;
            }
            return str;

            //ConsentPromptBehaviorAdmin  通知强度级别
            //EnableLUA		    是否开启UAC
            //PromptOnSecureDesktop	    桌面是否变黑
            //note UAC高
            //ConsentPromptBehaviorAdmin  2
            //EnableLUA		    1
            //PromptOnSecureDesktop	    1
            //note UAC中
            //ConsentPromptBehaviorAdmin  5
            //EnableLUA		    1
            //PromptOnSecureDesktop	    1
            //note UAC低
            //ConsentPromptBehaviorAdmin  5
            //EnableLUA		    1
            //PromptOnSecureDesktop	    0
            //note UAC关
            //ConsentPromptBehaviorAdmin  0
            //EnableLUA		    0（只需判断该值）
            //PromptOnSecureDesktop	    0
        }

        private static string GetCpuName()
        {
            try
            {
                //获取CPU序列号代码
                var cpuInfo = ""; //cpu序列号
                var mc = new ManagementClass("Win32_Processor");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    cpuInfo = mo.Properties["Name"].Value.ToString();
                }
                moc = null;
                mc = null;
                return cpuInfo;
            }
            catch
            {
                return null;
            }
            finally
            {
            }
        }

        private static string GetLanguage()
        {
            try
            {
                //获取语言
                var cpuInfo = ""; //cpu序列号
                var mc = new ManagementClass("Win32_OperatingSystem");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    cpuInfo = mo.Properties["CodeSet"].Value.ToString();
                }
                moc = null;
                mc = null;
                return cpuInfo;
            }
            catch
            {
                return null;
            }
            finally
            {
            }
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
                foreach (ManagementObject mo in moc)
                    if ((bool) mo["IPEnabled"])
                    {
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

        /// <summary>
        ///     物理内存
        /// </summary>
        /// <returns></returns>
        private static string GetTotalPhysicalMemory()
        {
            try
            {
                var st = "";
                var mc = new ManagementClass("Win32_ComputerSystem");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    st = mo["TotalPhysicalMemory"].ToString();
                }
                moc = null;
                mc = null;
                var m = 0L;
                return long.TryParse(st, out m) ? (m/1024/1024).ToString() : st;
            }
            catch
            {
                return null;
            }
            finally
            {
            }
        }

        private static string GetDisplayCard()
        {
            try
            {
                var st = "";
                var mc = new ManagementClass("Win32_VideoController");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    st = mo["VideoProcessor"].ToString();
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return null;
            }
            finally
            {
            }
        }

        /// <summary>
        ///     SP3等
        /// </summary>
        /// <returns></returns>
        private static string GetCsdVersion()
        {
            try
            {
                var st = "";
                var mc = new ManagementClass("Win32_OperatingSystem");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    st = mo["CSDVersion"].ToString();
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return null;
            }
            finally
            {
            }
        }

        private static string GetOsBit()
        {
            try
            {
                var st = "";
                var mc = new ManagementClass("Win32_OperatingSystem");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    st = mo["OSArchitecture"].ToString();
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return null;
            }
            finally
            {
            }
        }

        private static string GetVideoModeDescription()
        {
            try
            {
                var st = "";
                var mc = new ManagementClass("Win32_VideoController");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    st = mo["VideoModeDescription"].ToString();
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return null;
            }
            finally
            {
            }
        }
    }


    public static class SysHelper
    {
        static SysHelper()
        {
            OsProductName = GetOsName();

            ComputerName = Environment.MachineName.ToLower();
        }

        //public static string VersionName { get; private set; }

        public static string ComputerName { get; }


        public static string OsProductName { get; }

        private static string GetOsName()
        {
            try
            {
                var rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");
                if (rk == null) return string.Empty;
                var oSname = rk.GetValue("ProductName").ToString();
                rk.Close();
                return oSname;
            }
            catch (Exception)
            {
                return @"Can't find LocalMachine\Software\Microsoft\Windows NT\CurrentVersion\ProductName";
            }
        }
    }
}