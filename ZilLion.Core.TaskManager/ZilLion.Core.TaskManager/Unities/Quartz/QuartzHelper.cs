using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using ZilLion.Core.TaskManager.AppDomainTypeLoder;
using ZilLion.Core.TaskManager.Config;
using ZilLion.Core.TaskManager.Unities.File;

namespace ZilLion.Core.TaskManager.Unities.Quartz
{
    /// <summary>
    ///     任务处理帮助类
    /// </summary>
    public class QuartzHelper
    {
        private static readonly object Obj = new object();

        /// <summary>
        ///     任务程序集根目录
        /// </summary>
        private static readonly string TaskRootPath = FileHelper.GetRootPath() + "ZilLionTask";

        /// <summary>
        ///     缓存任务所在程序集信息
        /// </summary>
        private static readonly Dictionary<string, Assembly> AssemblyDic = new Dictionary<string, Assembly>();

        private static IScheduler _scheduler;

        private QuartzHelper()
        {
        }

        /// <summary>
        ///     初始化任务调度对象
        /// </summary>
        public static void InitScheduler()
        {
            try
            {
                lock (Obj)
                {
                    if (_scheduler != null) return;
                    var properties = new NameValueCollection
                    {
                        ["quartz.scheduler.instanceName"] = "ExampleDefaultQuartzScheduler",
                        ["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz",
                        ["quartz.threadPool.threadCount"] = "10",
                        ["quartz.threadPool.threadPriority"] = "Normal",
                        ["quartz.jobStore.misfireThreshold"] = "60000",
                        ["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz"
                    };
                    ISchedulerFactory factory = new StdSchedulerFactory(properties);

                    _scheduler = factory.GetScheduler();
                    _scheduler.Clear();
                    //LogHelper.WriteLog("任务调度初始化成功！");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog("任务调度初始化失败！", ex);
            }
        }

        /// <summary>
        ///     启用任务调度
        ///     启动调度时会把任务表中状态为“执行中”的任务加入到任务调度队列中
        /// </summary>
        public static void StartScheduler()
        {
            try
            {
                if (!_scheduler.IsStarted)
                {
                    //添加全局监听
                    _scheduler.ListenerManager.AddTriggerListener(new CustomTriggerListener(),
                        GroupMatcher<TriggerKey>.AnyGroup());
                    _scheduler.Start();

                    /////获取所有执行中的任务
                    var joblist = TaskHelper.ReadConfig();
                    if ((joblist == null) || !joblist.Any()) return;
                    foreach (var job in joblist)
                        try
                        {
                            ScheduleJob(job);
                        }
                        catch (Exception ex)
                        {
                            //LogHelper.WriteLog(string.Format("任务“{0}”启动失败！", taskUtil.TaskName), e);
                        }
                    //LogHelper.WriteLog("任务调度启动成功！");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog("任务调度启动失败！", ex);
            }
        }

        /// <summary>
        ///     删除现有任务
        /// </summary>
        /// <param name="JobKey"></param>
        public static void DeleteJob(string JobKey)
        {
            var jk = new JobKey(JobKey);
            if (_scheduler.CheckExists(jk))
                _scheduler.DeleteJob(jk);
        }

        /// <summary>
        ///     开始job
        /// </summary>
        /// <param name="jobconfig"></param>
        /// <param name="isDeleteOldTask"></param>
        public static void ScheduleJob(Jobconfig jobconfig, bool isDeleteOldTask = false)
        {
            if (isDeleteOldTask)
                DeleteJob(jobconfig.Jobid);
            //验证是否正确的Cron表达式
            if (ValidExpression(jobconfig.Jobronexpression))
            {
                string taskFileName = $@"ZilLion.Task.{jobconfig.Jobmodule}.dll";
                var taskClassName = jobconfig.Jobname;
                if (!System.IO.File.Exists($@"{TaskRootPath}\{taskFileName}")) return; //当文件不存在

                IJobDetail job = new JobDetailImpl(jobconfig.Jobid, GetClassInfo(taskFileName, taskClassName));
                var trigger = new CronTriggerImpl
                {
                    CronExpressionString = jobconfig.Jobronexpression,
                    Name = jobconfig.Jobid,
                    Description = jobconfig.Jobname
                };
                _scheduler.ScheduleJob(job, trigger);
                if (jobconfig.Jobstatus == 1)
                {
                    var jk = new JobKey(jobconfig.Jobid);
                    _scheduler.PauseJob(jk);
                }
                else
                {
                    //LogHelper.WriteLog(string.Format("任务“{0}”启动成功,未来5次运行时间如下:", taskUtil.TaskName));
                    var list = GetTaskeFireTime(jobconfig.Jobronexpression, 5);
                    foreach (var time in list)
                    {
                        //LogHelper.WriteLog(time.ToString());
                    }
                }
            }
            else
            {
                throw new Exception(jobconfig.Jobronexpression + "不是正确的Cron表达式,无法启动该任务!");
            }
        }

        /// <summary>
        ///     暂停任务
        /// </summary>
        /// <param name="jobKey"></param>
        public static void PauseJob(string jobKey)
        {
            var jk = new JobKey(jobKey);
            if (_scheduler.CheckExists(jk))
            {
                //任务已经存在则暂停任务
                _scheduler.PauseJob(jk);
                var jobDetail = _scheduler.GetJobDetail(jk);
                if (jobDetail.JobType.GetInterface("IInterruptableJob") != null)
                    _scheduler.Interrupt(jk);
                //LogHelper.WriteLog(string.Format("任务“{0}”已经暂停", JobKey));
            }
        }

        /// <summary>
        ///     恢复运行暂停的任务
        /// </summary>
        /// <param name="jobKey">任务key</param>
        public static void ResumeJob(string jobKey)
        {
            var jk = new JobKey(jobKey);
            if (_scheduler.CheckExists(jk))
                _scheduler.ResumeJob(jk);
        }

        /// <summary>
        ///     立即运行一次任务
        /// </summary>
        /// <param name="JobKey">任务key</param>
        public static void RunOnceTask(string JobKey)
        {
            var jk = new JobKey(JobKey);
            if (_scheduler.CheckExists(jk))
            {
                var jobDetail = _scheduler.GetJobDetail(jk);
                var type = jobDetail.JobType;
                //var instance = type.FastNew();
                var method = type.GetMethod("Execute");
                //method.Invoke(instance, new object[] { null });
                //LogHelper.WriteLog(string.Format("任务“{0}”立即运行", JobKey));
            }
        }

        /// <summary>
        ///     获取job类
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        private static Type GetClassInfo(string assemblyName, string className)
        {
            try
            {
                Assembly assembly = null;
                if (AssemblyDic.ContainsKey(assemblyName))
                {
                    assembly = AssemblyDic[assemblyName];
                }
                else
                {
                    var loader = new TypeLoader(assemblyName);
                    assembly = loader.RemoteTypeLoader.LoadedAssembly;
                }
                if (assembly == null) return null;
                var type = assembly.GetType(className, true, true);
                return type.GetInterfaces().Any(x => x.Name.ToUpper().Contains("IJOB")) ? type : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     停止任务调度
        /// </summary>
        public static void StopSchedule()
        {
            try
            {
                //判断调度是否已经关闭
                if (!_scheduler.IsShutdown)
                    _scheduler.Shutdown(true);
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog("任务调度停止失败！", ex);
            }
        }

        /// <summary>
        ///     校验字符串是否为正确的Cron表达式
        /// </summary>
        /// <param name="cronExpression">带校验表达式</param>
        /// <returns></returns>
        public static bool ValidExpression(string cronExpression)
        {
            return CronExpression.IsValidExpression(cronExpression);
        }

        /// <summary>
        ///     获取任务在未来周期内哪些时间会运行
        /// </summary>
        /// <param name="cronExpressionString">Cron表达式</param>
        /// <param name="numTimes">运行次数</param>
        /// <returns>运行时间段</returns>
        public static List<DateTime> GetTaskeFireTime(string cronExpressionString, int numTimes)
        {
            if (numTimes < 0)
                throw new Exception("参数numTimes值大于等于0");
            //时间表达式
            var trigger = TriggerBuilder.Create().WithCronSchedule(cronExpressionString).Build();
            var dates = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, null, numTimes);
            var list = new List<DateTime>();
            foreach (var dtf in dates)
                list.Add(TimeZoneInfo.ConvertTimeFromUtc(dtf.DateTime, TimeZoneInfo.Local));
            return list;
        }
    }
}