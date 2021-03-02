using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FXCommon.Database
{
    public class BulkFactory
    {
        public static BulkBase CreateBulkCopy(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.SqlServer: return new SqlServerBulk();
                case DatabaseType.Oracle: return new OracleBulk();
                default: throw new BulkCopyException("未找到对应BulkCopy类型。");
            }
        }
    }
}
