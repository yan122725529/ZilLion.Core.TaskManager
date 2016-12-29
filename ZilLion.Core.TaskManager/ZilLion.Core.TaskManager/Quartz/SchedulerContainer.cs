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
using ZilLion.Core.Log;
using ZilLion.Core.TaskManager.Config;
using ZilLion.Core.TaskManager.Respository;
using ZilLion.Core.TaskManager.Unities.Reflection;

namespace ZilLion.Core.TaskManager.Quartz
{
    public class SchedulerContainer
    {

        #region 单例

        private static SchedulerContainer _instance;

        public static SchedulerContainer GetContainerInstance()
        {
            return _instance ?? (_instance = new SchedulerContainer());
        }


        private SchedulerContainer()
        {
        }

        #endregion

        /// <summary>
        ///     任务程序集根目录
        /// </summary>
        private readonly ITaskRunLogRespository _taskRunLogRespository = new TaskRunLogRespository();
       
        private IScheduler _scheduler;

        private string _schedulerKey;

        private IList<TaskConfig> _taskConfigs;


        private Assembly _assembly;
        private bool _hasinited;
        /// <summary>
        ///     初始化任务调度对象
        /// </summary>
        public void InitScheduler(string schedulerKey, IList<TaskConfig> taskConfigs
            )
        {
            
            try
            {
                if (_hasinited)
                    throw new Exception("SchedulerContainer 只能初始化一次");
                    var properties = new NameValueCollection
                {
                    ["quartz.scheduler.instanceName"] = schedulerKey,
                    ["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz",
                    ["quartz.threadPool.threadCount"] = "10",
                    ["quartz.threadPool.threadPriority"] = "Normal",
                    ["quartz.jobStore.misfireThreshold"] = "60000",
                    ["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz"
                };
                ISchedulerFactory factory = new StdSchedulerFactory(properties);
                _schedulerKey = schedulerKey;
                _taskConfigs = taskConfigs;
              
                _scheduler = factory.GetScheduler();
                _scheduler.Clear();
                _hasinited = true;
            }
            catch (Exception ex)
            {
                ZilLionLoger.WriteErrLog(ex);
            }
        }

        #region 获取job实例

        /// <summary>
        ///     获取job类
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        private Type GetClassInfo(string className)
        {
            if (_assembly == null)
            {
                _assembly = Assembly.Load(_schedulerKey);
            }

            return
                _assembly.GetTypes()
                    .Where(x => x.GetInterfaces().Any(y => y.Name.ToUpper().Contains("IJOB")))
                    .FirstOrDefault(x => x.Name.Contains(className));
        }

        #endregion

        #region 任务执行相关

        /// <summary>
        ///     启用任务调度
        ///     启动调度时会把任务表中状态为“执行中”的任务加入到任务调度队列中
        /// </summary>
        public void StartScheduler()
        {
            try
            {
                if (_scheduler.IsStarted) return;
                //添加全局监听
                _scheduler.ListenerManager.AddTriggerListener(new CustomTriggerListener(),
                    GroupMatcher<TriggerKey>.AnyGroup());
                _scheduler.Start();

                //获取所有执行中的任务
                foreach (var taskConfig in _taskConfigs)
                    try
                    {
                        ScheduleJob(taskConfig);
                    }
                    catch (Exception ex)
                    {
                        ZilLionLoger.WriteErrLog(ex);
                    }
                ZilLionLoger.WriteTraceLog($"scheduler:{_schedulerKey}成功启动！");
            }
            catch (Exception ex)
            {
                ZilLionLoger.WriteErrLog(ex);
            }
        }

        /// <summary>
        ///     删除现有任务
        /// </summary>
        /// <param name="taskid"></param>
        public void DeleteJob(string taskid)
        {
            var jk = new JobKey(taskid);
            if (_scheduler.CheckExists(jk))
                _scheduler.DeleteJob(jk);
        }

        /// <summary>
        ///     开始job
        /// </summary>
        /// <param name="taskConfig"></param>
        /// <param name="isDeleteOldTask"></param>
        public void ScheduleJob(TaskConfig taskConfig, bool isDeleteOldTask = false)
        {
            if (isDeleteOldTask)
                DeleteJob(taskConfig.Taskid);
            //验证是否正确的Cron表达式
            if (ValidExpression(taskConfig.TaskExpression))
            {
                var taskClassName = taskConfig.Taskname;
                var classinfo = GetClassInfo(taskClassName);
                IJobDetail job = new JobDetailImpl(taskConfig.Taskid, classinfo);


                var trigger = new CronTriggerImpl
                {
                    CronExpressionString = taskConfig.TaskExpression,
                    Name = taskConfig.Taskid,
                    Description = taskConfig.Taskname
                };
                _scheduler.ScheduleJob(job, trigger);
                var runlog = _taskRunLogRespository.GetTaskRunLogById(taskConfig.Taskid);
                runlog.Tasknextruntime = GetTaskeFireTime(taskConfig.TaskExpression, 1).FirstOrDefault();
                _taskRunLogRespository.ModifyTaskRunLog(runlog);
            }
            else
            {
                throw new Exception(taskConfig.TaskExpression + "不是正确的Cron表达式,无法启动该任务!");
            }
        }

        /// <summary>
        ///     暂停任务
        /// </summary>
    
        /// <param name="taskid"></param>
        public void PauseJob(string taskid)
        {
            var jk = new JobKey(taskid);
            if (!_scheduler.CheckExists(jk)) return;
            //任务已经存在则暂停任务
            _scheduler.PauseJob(jk);
            var jobDetail = _scheduler.GetJobDetail(jk);
            if (jobDetail.JobType.GetInterface("IInterruptableJob") != null)
                _scheduler.Interrupt(jk);

            ZilLionLoger.WriteWarnLog($"任务“{taskid}”已经暂停");
        }

        /// <summary>
        ///     恢复运行暂停的任务
        /// </summary>
        /// <param name="taskid">任务key</param>
        public void ResumeJob(string taskid)
        {
            var jk = new JobKey(taskid);
            if (_scheduler.CheckExists(jk))
                _scheduler.ResumeJob(jk);
        }

        /// <summary>
        ///     立即运行一次任务
        /// </summary>
        /// <param name="taskid"></param>
        public void RunOnceTask(string taskid)
        {
            var jk = new JobKey(taskid);


            if (!_scheduler.CheckExists(jk)) return;
            var jobDetail = _scheduler.GetJobDetail(jk);
            var type = jobDetail.JobType;
            var instance = type.FastNew();
            var method = type.GetMethod("Execute");
            method.Invoke(instance, new object[] {null});
            ZilLionLoger.WriteTraceLog($"任务“{taskid}”立即运行");
        }

        /// <summary>
        ///     停止任务调度
        /// </summary>
        public void StopSchedule()
        {
            try
            {
                //判断调度是否已经关闭
                if (!_scheduler.IsShutdown)
                    _scheduler.Shutdown(true);
            }
            catch (Exception ex)
            {
                ZilLionLoger.WriteErrLog(ex);
            }
        }

        /// <summary>
        ///     校验字符串是否为正确的Cron表达式
        /// </summary>
        /// <param name="cronExpression">带校验表达式</param>
        /// <returns></returns>
        public bool ValidExpression(string cronExpression)
        {
            return CronExpression.IsValidExpression(cronExpression);
        }

        /// <summary>
        ///     获取任务在未来周期内哪些时间会运行
        /// </summary>
        /// <param name="cronExpressionString">Cron表达式</param>
        /// <param name="numTimes">运行次数</param>
        /// <returns>运行时间段</returns>
        public IList<DateTime> GetTaskeFireTime(string cronExpressionString, int numTimes)
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