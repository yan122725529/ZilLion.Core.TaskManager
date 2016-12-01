using DapperExtensions.Mapper;

namespace ZilLion.Core.DatabaseWrapper.Dapper.Configs
{
    public class DbModelCmdRs 
    {
        /// <summary>
        ///     ModelKey
        /// </summary>
        public string DbMk { get; set; }

        /// <summary>
        ///     ModelCommand
        /// </summary>
        public string DbMc { get; set; }

        /// <summary>
        /// RsKey
        /// </summary>
        public string DbRsKey { get; set; }

        /// <summary>
        /// 记录集是否有序：0-不需要提供顺序列RowSn；1-需要 默认0
        /// </summary>
        public short DbRowSn { get; set; }

        /// <summary>
        /// Byref:0-不作为参数输出；1-输出 默认0
        /// </summary>
        public short DbByRef { get; set; }

        /// <summary>
        /// 格式：满足RecordSet.Filter属性。空表示全部行。默认空。
        /// </summary>
        public string DbRowFilter { get; set; }

        /// <summary>
        /// 格式：字段名1,字段名2,…。空表示全部列 默认空。
        /// </summary>
        public string DbFieldFilter { get; set; }
    }

    public class DbModelCmdRsMapper : ClassMapper<DbModelCmdRs>
    {
        public DbModelCmdRsMapper()
        {
            Table("#dbmodelcmdrs");
            Map(f => f.DbMk).Column("dbmk");
            Map(f => f.DbMc).Column("dbmc");
            Map(f => f.DbRsKey).Column("dbrskey");
            Map(f => f.DbRowSn).Column("dbrowsn");
            Map(f => f.DbByRef).Column("dbbyref");
            Map(f => f.DbRowFilter).Column("dbrowfilter");
            Map(f => f.DbFieldFilter).Column("dbfieldfilter");
        }
    }
}