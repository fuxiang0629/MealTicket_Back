using FXCommon.Common;
using MealTicket_DBCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Runner
{
    public class ShareslimitupStatisticInfo
    {
        public long PlateId { get; set; }

        public DateTime Date { get; set; }

        public int ChangeIndex { get; set; }

        public int LimitUpCount { get; set; }

        public List<long> LimitUpSharesKeyList { get; set; }

        public int LinkageChangeIndex { get; set; }

        public int LinkageLimitUpCount { get; set; }

        public List<long> LinkageLimitUpShares { get; set; }
    }

    public class PlateLimitUpStatisticHelper
    {
        public static DateTime lastDate = DateTime.Parse("1991-01-01 00:00:00");//最近计算日期
        public static int lastPlateLimitUpStatisticStatisticDays = Singleton.Instance.PlateLimitUpStatisticStatisticDays;//最近计算的天数
        public static Dictionary<long, List<long>> hisPlateLimitUpDic = new Dictionary<long, List<long>>();//历史板块最少涨停数量
        public static Dictionary<long, List<long>> hisLinkagePlateLimitUpDic = new Dictionary<long, List<long>>();//历史联动板块最少涨停数量

        public static Dictionary<long, ShareslimitupStatisticInfo> TodayPlateSharesLimitupStatisticDic = new Dictionary<long, ShareslimitupStatisticInfo>();//今日结果缓存

        public static void Cal_PlateLimitUpStatistic(DateTime date) 
        {
            var shares_limit_session = Singleton.Instance.sessionHandler.GetShares_Limit_Session(false);
            var plate_shares_session = GetBasePlateSharesList();
            var plate_linkage = Singleton.Instance.sessionHandler.GetSetting_Plate_Linkage_Session(false);

            hisPlateLimitUpDic = CalHisPlateLimitUpDic(date, plate_shares_session, shares_limit_session);
            hisLinkagePlateLimitUpDic= CalHisLinkagePlateLimitUpDic(date, plate_shares_session, shares_limit_session, plate_linkage);
            lastDate = date;
            lastPlateLimitUpStatisticStatisticDays = Singleton.Instance.PlateLimitUpStatisticStatisticDays;


            var today_shares_quotes = Singleton.Instance.sessionHandler.GetShares_Quotes_AppointDate_Session(date, false);
            Dictionary<long, ShareslimitupStatisticInfo> tempTodayPlateSharesLimitupStatisticDic = new Dictionary<long, ShareslimitupStatisticInfo>();
            foreach (var item in plate_shares_session)
            {
                List<long> LimitUpSharesKeyList = new List<long>();
                List<long> HisLimitUpSharesKeyList = new List<long>();
                GetPlateShareslimitupStatisticInfo(item.Value, today_shares_quotes,shares_limit_session,  ref LimitUpSharesKeyList);
                if (hisPlateLimitUpDic.ContainsKey(item.Key))
                {
                    HisLimitUpSharesKeyList = hisPlateLimitUpDic[item.Key];
                }
                int LimitUpCount = LimitUpSharesKeyList.Count();
                int HisLimitUpCount = HisLimitUpSharesKeyList.Count();
                int ChangeIndex = LimitUpCount < HisLimitUpCount ? 0 : (int)Math.Round(LimitUpCount * 1.0 / (LimitUpCount + HisLimitUpCount) * 10000, 0);

                List<long> LinkageLimitUpShares = new List<long>();
                List<long> HisLinkageLimitUpShares = new List<long>();
                if (plate_linkage.ContainsKey(item.Key))
                {
                    var plateLinkageInfo = plate_linkage[item.Key];

                    foreach (var linkagePlate in plateLinkageInfo.SessionList)
                    {
                        if (!plate_shares_session.ContainsKey(linkagePlate.LinkagePlateId))
                        {
                            continue;
                        }
                        var linkagePlateSharesKeyList = plate_shares_session[linkagePlate.LinkagePlateId];
                        List<long> tempLimitUpSharesKeyList = new List<long>();
                        GetPlateShareslimitupStatisticInfo(linkagePlateSharesKeyList, today_shares_quotes, shares_limit_session, ref tempLimitUpSharesKeyList);
                        LinkageLimitUpShares.AddRange(tempLimitUpSharesKeyList);
                    }
                    LinkageLimitUpShares = LinkageLimitUpShares.Distinct().ToList();
                }
                if (hisLinkagePlateLimitUpDic.ContainsKey(item.Key))
                {
                    HisLinkageLimitUpShares = hisLinkagePlateLimitUpDic[item.Key];
                }
                int LinkageLimitUpCount = LinkageLimitUpShares.Count();
                int LinkageHisLimitUpCount = HisLinkageLimitUpShares.Count();
                int LinkageChangeIndex = LinkageLimitUpCount < LinkageHisLimitUpCount ? 0 : (int)Math.Round(LinkageLimitUpCount * 1.0 / (LinkageLimitUpCount + LinkageHisLimitUpCount) * 10000, 0);

                tempTodayPlateSharesLimitupStatisticDic.Add(item.Key,new ShareslimitupStatisticInfo 
                {
                    PlateId= item.Key,
                    Date= date,
                    ChangeIndex = ChangeIndex,
                    LimitUpCount= LimitUpCount,
                    LimitUpSharesKeyList= LimitUpSharesKeyList,
                    LinkageLimitUpShares= LinkageLimitUpShares,
                    LinkageChangeIndex= LinkageChangeIndex,
                    LinkageLimitUpCount= LinkageLimitUpCount
                });
            }
            TodayPlateSharesLimitupStatisticDic = tempTodayPlateSharesLimitupStatisticDic;

            WriteToDataBase();
        }

        private static Dictionary<long, List<long>> GetBasePlateSharesList()
        {
            var plate_base_info = Singleton.Instance.sessionHandler.GetPlate_Base_Session(false);
            var plate_shares_rel = Singleton.Instance.sessionHandler.GetPlate_Real_Shares_Session(0, false);
            Dictionary<long, List<long>> result = new Dictionary<long, List<long>>();
            foreach (var item in plate_shares_rel)
            {
                if (!plate_base_info.ContainsKey(item.Key))
                {
                    continue;
                }
                var plateBase = plate_base_info[item.Key];
                if (plateBase.BaseStatus != 1)
                {
                    continue;
                }
                result.Add(item.Key,item.Value);
            }
            return result;
        }

        private static Dictionary<long, List<long>> CalHisPlateLimitUpDic(DateTime date, Dictionary<long, List<long>> plate_shares_session,List<long> shares_limit_session) 
        {
            int PlateLimitUpStatisticStatisticDays = Singleton.Instance.PlateLimitUpStatisticStatisticDays;
            if (lastDate == date && lastPlateLimitUpStatisticStatisticDays == PlateLimitUpStatisticStatisticDays)
            {
                return hisPlateLimitUpDic;
            }
            List<Dictionary<long, Shares_Quotes_Session_Info>> his_shares_quotes_dic = new List<Dictionary<long, Shares_Quotes_Session_Info>>();
            for (int idx = 1; idx < PlateLimitUpStatisticStatisticDays; idx++)
            {
                DateTime tempDate = DbHelper.GetLastTradeDate2(0, 0, 0, -idx, date);
                var temp_shares_quotes = Singleton.Instance.sessionHandler.GetShares_Quotes_AppointDate_Session(tempDate, false);
                his_shares_quotes_dic.Add(temp_shares_quotes);
            }

            Dictionary<long, List<long>> result = new Dictionary<long, List<long>>();
            foreach (var item in plate_shares_session)
            {
                List<long> limitUpSharesList = new List<long>();
                int limitUpCount = -1;
                foreach (var his_shares_quotes in his_shares_quotes_dic)
                {
                    List<long> templimitUpSharesList = new List<long>();
                    int tempLimitUpCount = 0;
                    foreach (long sharesKey in item.Value)
                    {
                        if (shares_limit_session.Contains(sharesKey))
                        {
                            continue;
                        }
                        if (!his_shares_quotes.ContainsKey(sharesKey))
                        {
                            continue;
                        }
                        if (his_shares_quotes[sharesKey].PriceType != 1)
                        {
                            continue;
                        }
                        tempLimitUpCount++;
                        templimitUpSharesList.Add(sharesKey);
                    }
                    if (limitUpCount == -1 || limitUpCount > tempLimitUpCount)
                    {
                        limitUpCount = tempLimitUpCount;
                        limitUpSharesList = templimitUpSharesList;
                    }
                }
                result.Add(item.Key, limitUpSharesList);
            }
            return result;
        }

        private static Dictionary<long, List<long>> CalHisLinkagePlateLimitUpDic(DateTime date, Dictionary<long, List<long>> plate_shares_session, List<long> shares_limit_session, Dictionary<long, Setting_Plate_Linkage_Session_Info_Group> plate_linkage)
        {
            int PlateLimitUpStatisticStatisticDays = Singleton.Instance.PlateLimitUpStatisticStatisticDays;
            if (lastDate == date && lastPlateLimitUpStatisticStatisticDays == PlateLimitUpStatisticStatisticDays)
            {
                return hisLinkagePlateLimitUpDic;
            }
            List<Dictionary<long, Shares_Quotes_Session_Info>> his_shares_quotes_dic = new List<Dictionary<long, Shares_Quotes_Session_Info>>();
            for (int idx = 1; idx < PlateLimitUpStatisticStatisticDays; idx++)
            {
                DateTime tempDate = DbHelper.GetLastTradeDate2(0, 0, 0, -idx, date);
                var temp_shares_quotes = Singleton.Instance.sessionHandler.GetShares_Quotes_AppointDate_Session(tempDate, false);
                his_shares_quotes_dic.Add(temp_shares_quotes);
            }

            Dictionary<long, List<long>> result = new Dictionary<long, List<long>>();
            foreach (var item in plate_shares_session)
            {
                if (!plate_linkage.ContainsKey(item.Key))
                {
                    continue;
                }
                var plate_linkage_list = plate_linkage[item.Key].SessionList;

                List<long> limitUpSharesList = null;
                foreach (var his_shares_quotes in his_shares_quotes_dic)
                {
                    List<long> templimitUpSharesList = new List<long>();
                    foreach (var linkageplate in plate_linkage_list)
                    {
                        if (!plate_shares_session.ContainsKey(linkageplate.LinkagePlateId))
                        {
                            continue;
                        }
                        var tempSharesList = plate_shares_session[linkageplate.LinkagePlateId];
                        foreach (long sharesKey in tempSharesList)
                        {
                            if (shares_limit_session.Contains(sharesKey))
                            {
                                continue;
                            }
                            if (!his_shares_quotes.ContainsKey(sharesKey))
                            {
                                continue;
                            }
                            if (his_shares_quotes[sharesKey].PriceType != 1)
                            {
                                continue;
                            }
                            templimitUpSharesList.Add(sharesKey);
                        }
                    }
                    templimitUpSharesList = templimitUpSharesList.Distinct().ToList();
                    if (limitUpSharesList == null || limitUpSharesList.Count() > templimitUpSharesList.Count())
                    {
                        limitUpSharesList = templimitUpSharesList;
                    }
                }
                if (limitUpSharesList == null)
                {
                    limitUpSharesList = new List<long>();
                }
                result.Add(item.Key, limitUpSharesList);
            }
            return result;
        }

        private static void GetPlateShareslimitupStatisticInfo(List<long> sharesKeyList, Dictionary<long, Shares_Quotes_Session_Info> shares_quotes_session, List<long> shares_limit_session,  ref List<long> LimitUpSharesKeyList) 
        {
            foreach (long sharesKey in sharesKeyList)
            {
                if (shares_limit_session.Contains(sharesKey))
                {
                    continue;
                }
                if (!shares_quotes_session.ContainsKey(sharesKey))
                {
                    continue;
                }
                var sharesQuotes = shares_quotes_session[sharesKey];
                if (sharesQuotes.PriceType != 1)
                {
                    continue;
                }
                LimitUpSharesKeyList.Add(sharesKey);
            }
        }

        private static void WriteToDataBase() 
        {
            DataTable table = new DataTable();
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("PlateId", typeof(long));
            table.Columns.Add("ChangeIndex", typeof(int));
            table.Columns.Add("LimitUpCount", typeof(int));
            table.Columns.Add("LimitUpShares", typeof(string));
            table.Columns.Add("LinkageChangeIndex", typeof(int));
            table.Columns.Add("LinkageLimitUpCount", typeof(int));
            table.Columns.Add("LinkageLimitUpShares", typeof(string));

            foreach (var item in TodayPlateSharesLimitupStatisticDic)
            {
                DataRow row = table.NewRow();
                row["Date"] = item.Value.Date;
                row["PlateId"] = item.Value.PlateId;
                row["ChangeIndex"] = item.Value.ChangeIndex;
                row["LimitUpCount"] = item.Value.LimitUpCount;
                row["LimitUpShares"] = JsonConvert.SerializeObject(item.Value.LimitUpSharesKeyList);
                row["LinkageChangeIndex"] = item.Value.LinkageChangeIndex;
                row["LinkageLimitUpCount"] = item.Value.LinkageLimitUpCount;
                row["LinkageLimitUpShares"] = JsonConvert.SerializeObject(item.Value.LinkageLimitUpShares);
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@plateSharesLimitupStatistic", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.PlateSharesLimitupStatistic";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_Calculate_PlateSharesLimitupStatistic @plateSharesLimitupStatistic", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("PlateLimitUpStatisticRunner入库失败", ex);
                    tran.Rollback();
                }
            }
        }
    }
}
