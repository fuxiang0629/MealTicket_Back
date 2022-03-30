﻿using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FXCommon.Common.Session_New;

namespace MealTicket_Web_Handler.Runner
{
    public class SharesMonitorTriHelper
    {
        public static void Cal_SharesMonitorTri()
        {
            GET_DATA_CXT gdc = new GET_DATA_CXT(SessionHandler.CAL_SHARESMONITORTTR, null);
            Singleton.Instance.sessionHandler.GetDataWithLock("", gdc);
        }

        public static void _cal_SharesMonitorTri()
        {
            List<long> leaderShares = Singleton.Instance.sessionHandler.GetLeaderShares_ByAllShares(false);
            var shares_quotes_today = Singleton.Instance.sessionHandler.GetShares_Quotes_Today_Session(false);
            var limit_fundmultiple = Singleton.Instance.sessionHandler.GetShares_Limit_Fundmultiple_Session(false);

            List<long> sharesList = new List<long>();
            foreach (var share in leaderShares)
            {
                if (!limit_fundmultiple.ContainsKey(share))
                {
                    continue;
                }
                int minRiseRate = limit_fundmultiple[share].NearLimitRange;
                if (!shares_quotes_today.ContainsKey(share))
                {
                    continue;
                }
                var lastQuotes = shares_quotes_today[share];
                if (lastQuotes == null)
                {
                    continue;
                }
                if (lastQuotes.RiseRate < minRiseRate)
                {
                    continue;
                }

                sharesList.Add(share);
            }
            _insert_To_Database(sharesList);
        }

        public static void _insert_To_Database(List<long> sharesList)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));

            foreach (var share in sharesList)
            {
                int market = (int)(share % 10);
                string sharesCode = (share / 10).ToString().PadLeft(6, '0');
                DataRow row = table.NewRow();
                row["Market"] = market;
                row["SharesCode"] = sharesCode;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesList", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesBase";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_Shares_Monitor_Tri_Cal @sharesList", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("个股监控触发计算入库失败", ex);
                    tran.Rollback();
                }
            }
        }
    }
}
