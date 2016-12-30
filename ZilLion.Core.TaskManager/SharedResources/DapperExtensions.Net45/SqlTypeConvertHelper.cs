using System;
using System.Collections.Generic;

namespace DapperExtensions
{
    public class SqlTypeConvertHelper
    {
        private static readonly Dictionary<Type, string> TsqlTypesMap;
        private static readonly Dictionary<Type, string> TsqlTypesMapForDefault;
        private static readonly bool UserDefaultMap = true;
        static SqlTypeConvertHelper()
        {
            TsqlTypesMap = new Dictionary<Type, string>
            {
                {typeof (string), "NVARCHAR(2000)"},
                {typeof (int), "INT"},
                {typeof (int?), "INT"},
                {typeof (DateTime), "datetime"},
                {typeof (DateTime?), "datetime"},
                {typeof (long), "bigint"},
                {typeof (long?), "bigint"},
                {typeof (short), "tinyint"},
                {typeof (short?), "tinyint"},
                {typeof (decimal), "decimal(18,6)"},
                {typeof (decimal?), "decimal(18,6)"},
                {typeof (float), "float(53)"},
                {typeof (float?), "float(53)"},
                {typeof (double), "float(53)"},
                {typeof (double?), "float(53)"},
                {typeof (bool), "bit"},
                {typeof (bool?), "bit"}
            };

            TsqlTypesMapForDefault = new Dictionary<Type, string>
            {
                {typeof (string), "NVARCHAR(2000) default('')" },
                {typeof (int), "INT"},
                {typeof (int?), "INT default(0)"},
                {typeof (DateTime), "datetime"},
                {typeof (DateTime?), "datetime default('1900-01-01')"},
                {typeof (long), "bigint"},
                {typeof (long?), "bigint default(0)"},
                {typeof (short), "tinyint"},
                {typeof (short?), "tinyint default(0)"},
                {typeof (decimal), "decimal(18,6)"},
                {typeof (decimal?), "decimal(18,6) default(0)"},
                {typeof (float), "float(53)"},
                {typeof (float?), "float(53) default(0)"},
                {typeof (double), "float(53)"},
                {typeof (double?), "float(53) default(0)"},
                {typeof (bool), "bit"},
                {typeof (bool?), "bit default(0)"}
            };
        }



        public static string GetSqlType(System.Type theType)
        {
            if (UserDefaultMap)
            {
                return TsqlTypesMapForDefault.ContainsKey(theType) ? TsqlTypesMapForDefault[theType] : string.Empty;
            }
            return TsqlTypesMap.ContainsKey(theType) ? TsqlTypesMap[theType] : string.Empty;
        }

        //       
    }
}