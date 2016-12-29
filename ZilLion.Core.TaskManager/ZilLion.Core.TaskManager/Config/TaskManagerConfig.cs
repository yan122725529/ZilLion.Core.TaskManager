using System;

namespace ZilLion.Core.TaskManager.Config
{
    /// <summary>
    /// 组件关键配置
    /// </summary>
    [Serializable]
    public static class TaskManagerConfig
    {
        /// <summary>
        /// 作业配置数据库连接字符串
        /// </summary>
        public static string TaskConfigDbConString { get; set; }


        public static string TaskServerId { get; set; }
    }
}