using System;
using System.Configuration;
using System.IO;
using System.Text;
using Exceptionless;
using Exceptionless.Logging;
using ZilLion.Core.Unities.UnitiesMethods;


namespace ZilLion.Core.Log
{
    public static class ZilLionLoger
    {
        private static readonly bool LogTraceEnable;
        private static readonly bool LogWarnEnable;
        private static readonly bool LogInfoEnable;

        private static readonly IZilLionLog<Exception> Exceptionlogger;
        private static readonly IZilLionLog<LogEntity> Infologger;
        private static readonly IZilLionLog<LogEntity> Warnlogger;
        private static readonly IZilLionLog<LogEntity> Tracelogger;
        private static readonly IZilLionLog<LogEntity> Filelogger;

        private static readonly ExceptionlessClient ExceptionlessClient;
        private static readonly object WriteErrorObject = new object();

        static ZilLionLoger()
        {
            Exceptionlogger = new ZilLionAsyncLogger<Exception>
            {
                HandleAction = s => { WriteExceptionToExceptionless(s); }
            };
            Infologger = new ZilLionAsyncLogger<LogEntity>
            {
                HandleAction = s => { WriteMessageToExceptionless(s, LogLevel.Info); }
            };
            Warnlogger = new ZilLionAsyncLogger<LogEntity>
            {
                HandleAction = s => { WriteMessageToExceptionless(s, LogLevel.Warn); }
            };
            Filelogger = new ZilLionAsyncLogger<LogEntity> {HandleAction = s => { WriteLogTofile(s); }};
            Tracelogger = new ZilLionAsyncLogger<LogEntity>
            {
                HandleAction = s => { WriteMessageToExceptionless(s, LogLevel.Trace); }
            };

            try
            {
                LogTraceEnable = ConfigurationManager.AppSettings["LogTraceEnable"].ToLower() == "true";
                LogWarnEnable = ConfigurationManager.AppSettings["LogWarnEnable"].ToLower() == "true";
                LogInfoEnable = ConfigurationManager.AppSettings["LogInfoEnable"].ToLower() == "true";
                ExceptionlessClient = new ExceptionlessClient(c =>
                {
                    c.ApiKey = ConfigurationManager.AppSettings["ExceptionUrlApiKey"];
                    c.ServerUrl = ConfigurationManager.AppSettings["ExceptionUrl"];
                });

                ExceptionlessClient.Configuration.UseInMemoryStorage();
            }
            catch (Exception)
            {
                ExceptionlessClient = null;
            }
        }


        private static void WriteExceptionToExceptionless(Exception exception)
        {
            exception.ToExceptionless(null, ExceptionlessClient).AddTags("错误")
                // 标记为关键异常
                .MarkAsCritical()
                .SetReferenceId(Guid.NewGuid().ToString("N"))
                .SetProperty("IpAddress", Computer.IpAddress)
                .SetProperty("MacAddress", Computer.MacAddress)
                .Submit();
        }

        private static void WriteMessageToExceptionless(LogEntity logEntity, LogLevel logLevel)
        {
            var tag = "信息";
            if (logLevel == LogLevel.Warn)
                tag = "警告";
            if (logLevel == LogLevel.Trace)
                tag = "跟踪";
            ExceptionlessClient.CreateLog(logEntity.ToString(), logLevel)
                .AddTags(tag)
                .SetReferenceId(Guid.NewGuid().ToString("N"))
                .SetProperty("IpAddress", Computer.IpAddress)
                .SetProperty("MacAddress", Computer.MacAddress)
                .Submit();
        }


        public static void WriteInfoLog(string info)
        {
            if (!LogInfoEnable) return;
            var logEntity = new LogEntity
            {
                LogType = "信息",
                LogDetail = string.Empty,
                LogMessage = info,
                LogTime = DateTime.Now
            };


            if (ExceptionlessClient != null)
                Infologger.LogInfo(logEntity);
            else
                Filelogger.LogInfo(logEntity);
        }

        public static void WriteWarnLog(string info)
        {
            if (!LogWarnEnable) return;
            var logEntity = new LogEntity
            {
                LogType = "警告",
                LogDetail = string.Empty,
                LogMessage = info,
                LogTime = DateTime.Now
            };
            if (ExceptionlessClient != null)
                Warnlogger.LogInfo(logEntity);
            else
                Filelogger.LogInfo(logEntity);
        }

        public static void WriteTraceLog(string info)
        {
            if (!LogTraceEnable) return;
            var logEntity = new LogEntity
            {
                LogType = "跟踪",
                LogDetail = string.Empty,
                LogMessage = info,
                LogTime = DateTime.Now
            };
            if (ExceptionlessClient != null)
                Tracelogger.LogInfo(logEntity);
            else
                Filelogger.LogInfo(logEntity);
        }

        public static void WriteErrLog(Exception ex)
        {
            if (ExceptionlessClient != null)
            {
                Exceptionlogger.LogInfo(ex);
            }
            else
            {
                var logEntity = new LogEntity
                {
                    LogType = ex.GetType().FullName,
                    LogDetail = CreateErrDetail(ex),
                    LogMessage = ex.Message,
                    LogTime = DateTime.Now
                };

                Filelogger.LogInfo(logEntity);
            }
        }

        #region 文件日志

        /// <summary>
        ///     写文件
        /// </summary>
        /// <param name="info"></param>
        private static void WriteLogTofile(LogEntity info)
        {
            lock (WriteErrorObject)
            {
                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\ZilionLog\";
#if DEBUG
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
#endif
                    if (!Directory.Exists(path)) return;
                    var logName = $@"ZiLion_ErrorLog_{DateTime.Now:yyyyMMddHH}.TXT";
                    var stream = new StreamWriter(path + logName, true, Encoding.Default);
                    stream.WriteLine(info.ToString());
                    stream.Flush();
                    stream.Close();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        ///     创建异常string
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="isinnerex"></param>
        /// <returns></returns>
        private static string CreateErrDetail(Exception ex, bool isinnerex = false)
        {
            var errBuilder = new StringBuilder();

            errBuilder.AppendLine(!isinnerex
                ? string.Empty
                : "InnerExceptionstart:");

            errBuilder.AppendLine($"LogTime:{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            errBuilder.AppendLine($"ErrMessage:{ex.Message}");
            errBuilder.AppendLine($"LogType:{ex.GetType().FullName}");
            errBuilder.AppendLine($"ErrSource:{ex.Source}");
            errBuilder.AppendLine($"ErrStackTrace:{ex.StackTrace}");

            var innerex = ex.InnerException;
            if (innerex != null)
                errBuilder.AppendLine(CreateErrDetail(innerex, true));

            errBuilder.AppendLine(!isinnerex
                ? string.Empty
                : "InnerExceptionend:");
            return errBuilder.ToString();
        }

        #endregion
    }


    public class LogEntity
    {
        public DateTime LogTime { get; set; }
        public string LogType { get; set; }
        public string LogMessage { get; set; }
        public string LogDetail { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"new log=>LogTime:{LogTime:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"LogType:{LogType}");
            builder.AppendLine($"LogMessage:{LogMessage}");
            builder.AppendLine($"LogDetail:{LogDetail}");
            return builder.ToString();
        }
    }
}