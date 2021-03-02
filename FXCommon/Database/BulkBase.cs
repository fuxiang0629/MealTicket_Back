using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace FXCommon.Database
{
    public abstract class BulkBase
    {
        /// <summary>
        /// 插入批次大小（每次提交一批次数据）。默认10000。
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// 超时时间,默认300
        /// </summary>
        public int BulkCopyTimeout { get; set; }

        /// <summary>
        /// 列映射
        /// </summary>
        public Dictionary<string, string> ColumnMappings { get; set; }

        public BulkBase()
        {
            BatchSize = 10000;
            BulkCopyTimeout = 300;
        }

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="connection">数据库连接字符窜</param>
        /// <param name="srcTable">要插入的数据</param>
        /// <param name="targetTable">数据库中的表名</param>
        /// <exception cref="BulkCopyException">BulkCopyException</exception>
        public abstract void BulkWriteToServer(string connection, DataTable srcTable, string targetTable);

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="connection">数据库连接字符窜</param>
        /// <param name="srcTable">要插入的数据</param>
        /// <param name="targetTable">数据库中的表名</param>
        /// <exception cref="BulkCopyException">BulkCopyException</exception>
        public abstract void BulkWriteToServer(DbConnection connection, DataTable srcTable, string targetTable, SqlTransaction tran);
    }
}
