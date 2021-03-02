using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace FXCommon.Database
{
    /// <summary>
    /// Oralce数据库批量拷贝
    /// </summary>
    class OracleBulk : BulkBase
    {
        /// /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="connection">数据库连接字符窜</param>
        /// <param name="srcTable">要插入的数据</param>
        /// <param name="targetTable">数据库中的表名</param>
        /// <exception cref="BulkCopyException">BulkCopyException</exception>
        public override void BulkWriteToServer(string connection, DataTable srcTable, string targetTable)
        {
            try
            {
                using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection, OracleBulkCopyOptions.UseInternalTransaction))
                {
                    bulkCopy.BulkCopyTimeout = BulkCopyTimeout;
                    bulkCopy.DestinationTableName = targetTable;
                    bulkCopy.BatchSize = BatchSize;
                    if (ColumnMappings != null && ColumnMappings.Count > 0)
                    {
                        foreach (var item in ColumnMappings)
                        {
                            bulkCopy.ColumnMappings.Add(item.Key, item.Value);
                        }
                    }
                    bulkCopy.WriteToServer(srcTable);
                }
            }
            catch (Exception ex)
            {
                throw new BulkCopyException("批量拷贝失败。", ex);
            }
        }

        public override void BulkWriteToServer(DbConnection connection, DataTable srcTable, string targetTable, SqlTransaction tran)
        {
            try
            {
                var oracleconnection = connection as OracleConnection;
                using (OracleBulkCopy bulkCopy = new OracleBulkCopy(oracleconnection, OracleBulkCopyOptions.UseInternalTransaction))
                {
                    bulkCopy.BulkCopyTimeout = BulkCopyTimeout;
                    bulkCopy.DestinationTableName = targetTable;
                    bulkCopy.BatchSize = BatchSize;
                    if (ColumnMappings != null && ColumnMappings.Count > 0)
                    {
                        foreach (var item in ColumnMappings)
                        {
                            bulkCopy.ColumnMappings.Add(item.Key, item.Value);
                        }
                    }
                    bulkCopy.WriteToServer(srcTable);
                }
            }
            catch (Exception ex)
            {
                throw new BulkCopyException("批量拷贝失败。", ex);
            }
        }
    }
}
