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
using ZilLion.Core.TaskManager.Config;
using ZilLion.Core.TaskManager.Unities.File;

namespace ZilLion.Core.TaskManager.Unities.Quartz
{
    /// <summary>
    /// 任务处理帮助类
    /// </summary>
    public class QuartzHelper
    {
        private QuartzHelper() { }

        private static readonly object Obj = new object();

        /// <summary>
        /// 任务程序集根目录
        /// </summary>
        private static readonly string TaskRootPath = FileHelper.GetRootPath() + "ZilLionTask";

        /// <summary>
        /// 缓存任务所在程序集信息
        /// </summary>
        private static readonly Dictionary<string, Assembly> AssemblyDict = new Dictionary<string, Assembly>();

        private static IScheduler _scheduler = null;

        /// <summary>
        /// 初始化任务调度对象
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
        /// 启用任务调度
        /// 启动调度时会把任务表中状态为“执行中”的任务加入到任务调度队列中
        /// </summary>
        public static void StartScheduler()
        {
            try
            {
                if (!_scheduler.IsStarted)
                {
                    //添加全局监听
                    _scheduler.ListenerManager.AddTriggerListener(new CustomTriggerListener(), GroupMatcher<TriggerKey>.AnyGroup());
                    _scheduler.Start();

                    /////获取所有执行中的任务
                    var joblist = TaskHelper.ReadConfig();
                    if (joblist != null && joblist.Any())
                    {
                        foreach (var job in joblist)
                        {
                            try
                            {
                                ScheduleJob(job);
                            }
                            catch (Exception ex)
                            {
                                //LogHelper.WriteLog(string.Format("任务“{0}”启动失败！", taskUtil.TaskName), e);
                            }
                        }
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
        /// 删除现有任务
        /// </summary>
        /// <param name="JobKey"></param>
        public static void DeleteJob(string JobKey)
        {
            JobKey jk = new JobKey(JobKey);
            if (_scheduler.CheckExists(jk))
            {
                //任务已经存在则删除
                _scheduler.DeleteJob(jk);
                //LogHelper.WriteLog(string.Format("任务“{0}”已经删除", JobKey));
            }
        }

        /// <summary>
        /// 启用任务
        /// <param name="taskUtil">任务信息</param>
        /// <param name="isDeleteOldTask">是否删除原有任务</param>
        /// <returns>返回任务trigger</returns>
        /// </summary>
        public static void ScheduleJob(Jobconfig jobconfig, bool isDeleteOldTask = false)
        {
            if (isDeleteOldTask)
            {
                //先删除现有已存在任务
                DeleteJob(jobconfig.Jobid.ToString());
            }
            //验证是否正确的Cron表达式
            if (ValidExpression(jobconfig.Jobronexpression))
            {
                IJobDetail job = new JobDetailImpl(jobconfig.Jobid, GetClassInfo(taskUtil.Assembly, taskUtil.Class));
                var trigger = new CronTriggerImpl
                {
                    CronExpressionString = jobconfig.Jobronexpression,
                    Name = jobconfig.Jobid.ToString(),
                    Description = jobconfig.Jobname
                };
                _scheduler.ScheduleJob(job, trigger);
                if (jobconfig.Jobstatus == 1)
                {
                    JobKey jk = new JobKey(jobconfig.Jobid);
                    _scheduler.PauseJob(jk);
                }
                else
                {
                    //LogHelper.WriteLog(string.Format("任务“{0}”启动成功,未来5次运行时间如下:", taskUtil.TaskName));
                    List<DateTime> list = GetTaskeFireTime(jobconfig.Jobronexpression, 5);
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
        /// 暂停任务
        /// </summary>
        /// <param name="jobKey"></param>
        public static void PauseJob(string jobKey)
        {
            JobKey jk = new JobKey(jobKey);
            if (_scheduler.CheckExists(jk))
            {
                //任务已经存在则暂停任务
                _scheduler.PauseJob(jk);
                var jobDetail = _scheduler.GetJobDetail(jk);
                if (jobDetail.JobType.GetInterface("IInterruptableJob") != null)
                {
                    _scheduler.Interrupt(jk);
                }
                //LogHelper.WriteLog(string.Format("任务“{0}”已经暂停", JobKey));
            }
        }

        /// <summary>
        /// 恢复运行暂停的任务
        /// </summary>
        /// <param name="jobKey">任务key</param>
        public static void ResumeJob(string jobKey)
        {
            JobKey jk = new JobKey(jobKey);
            if (_scheduler.CheckExists(jk))
            {
                //任务已经存在则暂停任务
                _scheduler.ResumeJob(jk);
                //LogHelper.WriteLog(string.Format("任务“{0}”恢复运行", JobKey));
            }
        }

        /// <summary>
        ///立即运行一次任务
        /// </summary>
        /// <param name="JobKey">任务key</param>
        public static void RunOnceTask(string JobKey)
        {
            JobKey jk = new JobKey(JobKey);
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
        /// 获取类的属性、方法
        /// </summary>
        /// <param name="assemblyName">程序集</param>
        /// <param name="className">类名</param>
        //todo 使用APPDOMAIN  加载 并对APPDomain监控

        private static Type GetClassInfo(string assemblyName, string className)
        {
            try
            {
                string assemblyPath = string.Format("{0}\\{1}.dll", TaskRootPath, assemblyName);
                string HashCode = FileHelper.GetFileHash(assemblyPath);
                Assembly assembly = null;
                if (!AssemblyDict.TryGetValue(HashCode, out assembly))
                {
                    //修改程序集Assembly.LoadForm 导致程序集被占用问题
                    assembly = Assembly.Load(System.IO.File.ReadAllBytes(assemblyPath));
                    AssemblyDict[HashCode] = assembly;
                }
                Type type = assembly.GetType(className, true, true);
                return type;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 停止任务调度
        /// </summary>
        public static void StopSchedule()
        {
            try
            {
                //判断调度是否已经关闭
                if (!_scheduler.IsShutdown)
                {
                    //等待任务运行完成
                    _scheduler.Shutdown(true);
                    //LogHelper.WriteLog("任务调度停止！");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog("任务调度停止失败！", ex);
            }
        }

        /// <summary>
        /// 校验字符串是否为正确的Cron表达式
        /// </summary>
        /// <param name="cronExpression">带校验表达式</param>
        /// <returns></returns>
        public static bool ValidExpression(string cronExpression)
        {
            return CronExpression.IsValidExpression(cronExpression);
        }

        /// <summary>
        /// 获取任务在未来周期内哪些时间会运行
        /// </summary>
        /// <param name="cronExpressionString">Cron表达式</param>
        /// <param name="numTimes">运行次数</param>
        /// <returns>运行时间段</returns>
        public static List<DateTime> GetTaskeFireTime(string cronExpressionString, int numTimes)
        {
            if (numTimes < 0)
            {
                throw new Exception("参数numTimes值大于等于0");
            }
            //时间表达式
            ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(cronExpressionString).Build();
            IList<DateTimeOffset> dates = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, null, numTimes);
            List<DateTime> list = new List<DateTime>();
            foreach (DateTimeOffset dtf in dates)
            {
                list.Add(TimeZoneInfo.ConvertTimeFromUtc(dtf.DateTime, TimeZoneInfo.Local));
            }
            return list;
        }
    }
}