﻿using System;
using Quartz;

namespace ZilLion.Core.TaskManager.Unities.Quartz
{
    /// <summary>
    ///     自定义触发器监听
    /// </summary>
    public class CustomTriggerListener : ITriggerListener
    {
        public string Name => "All_TriggerListener";

        /// <summary>
        ///     Job执行时调用
        /// </summary>
        /// <param name="trigger">触发器</param>
        /// <param name="context">上下文</param>
        public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            //todo 记下日志
        }


        /// <summary>
        ///     //Trigger触发后，job执行时调用本方法。true即否决，job后面不执行。
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            if (context.NextFireTimeUtc != null)
            {
                var lastRunTime = TimeZoneInfo.ConvertTimeFromUtc(context.NextFireTimeUtc.Value.DateTime, TimeZoneInfo.Local);
            }
            var taskid = trigger.JobKey.Name;

            return false;
        }

        /// <summary>
        ///     Job完成时调用
        /// </summary>
        /// <param name="trigger">触发器</param>
        /// <param name="context">上下文</param>
        /// <param name="triggerInstructionCode"></param>
        public void TriggerComplete(ITrigger trigger, IJobExecutionContext context,
            SchedulerInstruction triggerInstructionCode)
        {

            if (context.NextFireTimeUtc != null)
            {
                var nextRunTime = TimeZoneInfo.ConvertTimeFromUtc(context.NextFireTimeUtc.Value.DateTime, TimeZoneInfo.Local);
            }
            var taskid = trigger.JobKey.Name;
            //todo  更新到数据库

            //TaskHelper.UpdateLastRunTime(trigger.JobKey.Name, );
        }

        /// <summary>
        ///     错过触发时调用
        /// </summary>
        /// <param name="trigger">触发器</param>
        public void TriggerMisfired(ITrigger trigger)
        {
            //TODO  记下日志
        }
    }
}