using System;
using System.Diagnostics;
using System.Security.Principal;

namespace ZilLion.Core.Unities.UnitiesMethods.Admin
{
    public class AdminRun
    {
        /// <summary>
        ///     以管理员方式运行程序
        /// </summary>
        public static void Run()
        {
            /**
             * 当前用户是管理员的时候，直接启动应用程序
             * 如果不是管理员，则使用启动对象启动程序，以确保使用管理员身份运行
             */
            //获得当前登录的Windows用户标示
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            //判断当前登录用户是否为管理员
            //如果不是管理员，则以管理员方式运行
            if (principal.IsInRole(WindowsBuiltInRole.Administrator)) return;
            //创建启动对象
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                Verb = "runas"
            };
            //设置启动动作,确保以管理员身份运行
            Process.Start(startInfo);
            Environment.Exit(0);
        }
    }
}