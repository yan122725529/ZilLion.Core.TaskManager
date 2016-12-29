using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Quartz;
using ZilLion.Core.Log;

namespace TaskPlugin
{
    public class TestLogJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            WriteLogTofile();
        }

        private static void WriteLogTofile()
        {
            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\TaskLog\";
#if DEBUG
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
#endif
                if (!Directory.Exists(path)) return;
                var logName = $@"ZiLion_Log_{DateTime.Now:yyyyMMddHH}.TXT";
                var stream = new StreamWriter(path + logName, true, Encoding.Default);
                stream.WriteLine(AppDomain.CurrentDomain.FriendlyName + "*********" +
                                 DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                stream.Flush();
                stream.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ZilLionLoger.WriteErrLog(ex);
            }
        }
    }
}