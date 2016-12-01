using DapperExtensions.Mapper;

namespace ZilLion.Core.DatabaseWrapper.Dapper.Configs
{
    public class DbModelCmd 
    {
        /// <summary>
        ///     ModelKey
        /// </summary>
        public string DbMk { get; set; }

        /// <summary>
        ///     ModelCommand 命令
        /// </summary>
        public string DbMc { get; set; }

        /// <summary>
        ///     是否需要调用DTDS路径确认：0-否 1-是
        /// </summary>
        public short DbDtdsLuj { get; set; }

        /// <summary>
        ///     标量参数格式：参数名1|类型1|参数名2|类型2|…
        /// </summary>
        public string DbBlcs { get; set; }

        /// <summary>
        ///     对应命令的核心错误处理方式。0-raise报错；1-通过rserr返回。默认0
        /// </summary>
        public short DbCwclFs { get; set; }

        /// <summary>
        ///    对应的存储过程名
        /// </summary>
        public string Dbccgc
        {
            get { return string.Format("pr_{0}_{1}", DbMk, DbMc); } 
        }
    }

    public class DbModelCmdMapper : ClassMapper<DbModelCmd>
    {
        public DbModelCmdMapper()
        {
            Table("#dbmodelcmd");
            Map(f => f.DbMk).Column("dbmk");
            Map(f => f.DbMc).Column("dbmc");
            Map(f => f.DbDtdsLuj).Column("dbdtdsluj");
            Map(f => f.DbBlcs).Column("dbblcs");
            Map(f => f.DbCwclFs).Column("dbcwclfs");
        }
    }
}