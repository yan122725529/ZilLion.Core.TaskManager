namespace ZilLion.Core.DatabaseWrapper.Dapper.Configs
{
    public class ServerAnswer
    {
        private string _cngVName;
        private string _cngVer;

        /// <summary>
        /// 服务器
        /// </summary>
        public string Server { get; set; }


        /// <summary>
        /// 用户名
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// 子程序系统编号
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        /// 会话组件的版本名称
        /// </summary>
        public string CngVName
        {
            get { return _cngVName; }
            set { _cngVName = value; }
        }

        /// <summary>
        /// 会话组件的版本号
        /// </summary>
        public string CngVer
        {
            get { return _cngVer; }
            set { _cngVer = value; }
        }
    }
}
