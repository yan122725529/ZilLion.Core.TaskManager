using System;

namespace ZilLion.Core.QuartzWrapper.Config
{
    /// <summary>
    ///     组件关键配置
    /// </summary>
    [Serializable]
    public class TaskRunnerConfig
    {
        /// <summary>
        ///     作业配置数据库连接字符串
        /// </summary>
        public string TaskConfigDbConString { get; set; }


        public string TaskServerId { get; set; }
    }
}