﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using ZilLion.Core.TaskManager.Config;
using ZilLion.Core.TaskManager.Respository;
using ZilLion.Core.TaskManager.Unities.File;
using ZilLion.Core.TaskManager.Unities.Reflection;

//using ZilLion.Core.TaskManager.AppDomainTypeLoder;

namespace ZilLion.Core.TaskManager.Unities.Quartz
{
    /// <summary>
    ///     任务处理帮助类
    /// </summary>
    public class QuartzHelper
    {
        private static readonly ITaskRunLogRespository TaskRunLogRespository = new TaskRunLogRespository();

        /// <summary>
        ///     任务程序集根目录
        /// </summary>
        private static readonly string TaskRootPath = FileHelper.GetRootPath() + "ZilLionTask";

        /// <summary>
        ///     缓存任务所在程序集信息
        /// </summary>
        public static readonly Dictionary<Assembly, AppDomain> AssemblyDic = new Dictionary<Assembly, AppDomain>();

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
            catch (Exception ex)
            {
                //LogHelper.WriteLog("任务调度初始化失败！", ex);
            }
        }

        #region 获取job实例

        /// <summary>
        ///     获取job类
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        private static Tuple<Type, AppDomain> GetClassInfo(string assemblyName, string className)
        {
            var first = AssemblyDic.Keys.FirstOrDefault(x => x.FullName.Contains(assemblyName));
            Assembly assembly = null;
            if (first != null)
            {
                assembly = first;
            }
            else
            {
                #region 创建新的appdomain

                var setup = new AppDomainSetup
                {
                    ApplicationName = "JobLoader",
                    ApplicationBase = FileHelper.GetRootPath(),
                    PrivateBinPath = "ZilLionTask",
                    CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JobCachePath"),
                    ShadowCopyFiles = "true"
                };
                setup.ShadowCopyDirectories = string.Concat(setup.ApplicationBase, ";", setup.PrivateBinPath);
                setup.ApplicationName = string.Concat("JobLoaderDomain_", Guid.NewGuid().ToString());
                var remoteDomain = AppDomain.CreateDomain(string.Concat("JobLoaderDomain_", Guid.NewGuid().ToString()),
                    null, setup);

                assembly = Assembly.Load(assemblyName); //需要从加载到domain

                AssemblyDic.Add(assembly, remoteDomain);

                #endregion
            }
            var type = assembly.GetTypes().FirstOrDefault(x => x.Name.Contains(className));

            if (type != null)
                if (type.GetInterfaces().Any(x => x.Name.ToUpper().Contains("IJOB")))
                    return new Tuple<Type, AppDomain>(type, AssemblyDic[assembly]);


            return null;
        }

        #endregion

        #region 任务执行相关

        /// <summary>
        ///     启用任务调度
        ///     启动调度时会把任务表中状态为“执行中”的任务加入到任务调度队列中
        /// </summary>
        public static void StartScheduler(IList<TaskConfig> configs)
        {
            if (configs == null)
                return;


            if (!configs.Any()) return;
            try
            {
                if (_scheduler.IsStarted) return;
                //添加全局监听
                _scheduler.ListenerManager.AddTriggerListener(new CustomTriggerListener(),
                    GroupMatcher<TriggerKey>.AnyGroup());
                _scheduler.Start();

                //获取所有执行中的任务
                foreach (var job in configs)
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
            catch (Exception ex)
            {
                //LogHelper.WriteLog("任务调度启动失败！", ex);
            }
        }

        /// <summary>
        ///     删除现有任务
        /// </summary>
        /// <param name="jobKey"></param>
        public static void DeleteJob(string jobKey)
        {
            var jk = new JobKey(jobKey);
            if (_scheduler.CheckExists(jk))
                _scheduler.DeleteJob(jk);
        }

        /// <summary>
        ///     开始job
        /// </summary>
        /// <param name="taskConfig"></param>
        /// <param name="isDeleteOldTask"></param>
        public static void ScheduleJob(TaskConfig taskConfig, bool isDeleteOldTask = false)
        {
            if (isDeleteOldTask)
                DeleteJob(taskConfig.Taskid);
            //验证是否正确的Cron表达式
            if (ValidExpression(taskConfig.TaskExpression))
            {
                //string taskFileName = $@"ZilLion.Task.{taskConfig.TaskModule}.dll";
                var taskClassName = taskConfig.Taskname;
                //if (!System.IO.File.Exists($@"{TaskRootPath}\{taskFileName}")) return; //当文件不存在

                #region IJobDetail 支持 appdomain  加载  add by yanzhengyu 20161229
                var classinfo = GetClassInfo(taskConfig.TaskModule, taskClassName);
                IJobDetail job = new JobDetailImpl(taskConfig.Taskid, classinfo.Item1);
                job.ExcuteAppDomain = classinfo.Item2;
                #endregion
                var trigger = new CronTriggerImpl
                {
                    CronExpressionString = taskConfig.TaskExpression,
                    Name = taskConfig.Taskid,
                    Description = taskConfig.Taskname
                };
                _scheduler.ScheduleJob(job, trigger);
                var runlog = TaskRunLogRespository.GetTaskRunLogById(taskConfig.Taskid);
                runlog.Tasknextruntime = GetTaskeFireTime(taskConfig.TaskExpression, 1).FirstOrDefault();
                TaskRunLogRespository.ModifyTaskRunLog(runlog);
            }
            else
            {
                throw new Exception(taskConfig.TaskExpression + "不是正确的Cron表达式,无法启动该任务!");
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
        /// <param name="jobKey">任务key</param>
        public static void RunOnceTask(string jobKey)
        {
            var jk = new JobKey(jobKey);


            if (_scheduler.CheckExists(jk))
            {
                var jobDetail = _scheduler.GetJobDetail(jk);
                var type = jobDetail.JobType;
                var instance = type.FastNew();
                var method = type.GetMethod("Execute");
                method.Invoke(instance, new object[] {null});
                //LogHelper.WriteLog(string.Format("任务“{0}”立即运行", JobKey));
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
        public static IList<DateTime> GetTaskeFireTime(string cronExpressionString, int numTimes)
        {
            if (numTimes < 0)
                throw new Exception("参数numTimes值大于等于0");
            //时间表达式
            var trigger = TriggerBuilder.Create().WithCronSchedule(cronExpressionString).Build();
            var dates = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, null, numTimes);
            return dates.Select(dtf => TimeZoneInfo.ConvertTimeFromUtc(dtf.DateTime, TimeZoneInfo.Local)).ToList();
        }

        #endregion
    }
}