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
    public class HotspotInfo
    {
        public long HotspotId { get; set; }

        public long AccountId { get; set; }

        public List<long> SharesKeyList { get; set; }

        public int AddCount { get; set; }
    }

    public class Cal_SharesRiseLimitInfo
    {
        public long SharesKey { get; set; }

        public DateTime Date { get; set; }

        public int RiseLimitCount { get; set; }

        public int RiseLimitDays { get; set; }
    }

    public class HotSpotHighMarkHelper
    {
        public static Dictionary<long, Dictionary<long, Cal_SharesRiseLimitInfo>> TodaySharesHighMark = new Dictionary<long, Dictionary<long, Cal_SharesRiseLimitInfo>>();

        /// <summary>
        /// 前一个交易日缓存
        /// </summary>
        private static List<t_shares_quotes_riselimit> last_riselimit = new List<t_shares_quotes_riselimit>();
        private static Dictionary<long, HotspotInfo> last_hotspot_shares_session = new Dictionary<long, HotspotInfo>();
        private static DateTime last_date = DateTime.Now.Date;

        public static void Cal_HotSpotHighMark(DateTime date,long accountId=0) 
        {
            var lastTradeDate = DbHelper.GetLastTradeDate2(0, 0, 0, 0, DateTime.Now.Date);
            if (date == lastTradeDate)
            {
                return;
            }

            var sharesRiselimitList = GetSharesRiselimitDic(date, Singleton.Instance.HighMarkLimitUpCount,true);

            //1.连板数前X,最低X板
            //2.每个题材第一
            var result=GetHighmarkShares(sharesRiselimitList, accountId);

            //导入数据库
            importToDatabase(result, date, accountId);
        }

        private static List<t_shares_quotes_riselimit> GetSharesRiselimitDic(DateTime date,int count=-1,bool IsLimitUpToday=true)
        {
            using (var db = new meal_ticketEntities())
            {
                var riselimit = (from item in db.t_shares_quotes_riselimit
                                 where item.DataDate == date && (item.IsLimitUpToday == true || !IsLimitUpToday) && item.RiseLimitCount >= count
                                 orderby item.RiseLimitCount descending,item.LastRiseLimitDate descending, item.LastRiseLimitTime
                                 select item).ToList();
                return riselimit;
            }
        }

        /// <summary>
        /// 连板数前X,最低X板
        /// 每个题材第一
        /// </summary>
        /// <returns></returns>
        private static Dictionary<long,Dictionary<long, Cal_SharesRiseLimitInfo>> GetHighmarkShares(List<t_shares_quotes_riselimit> list,long accountId=0)
        {
            var shares_limit_session = Singleton.Instance.sessionHandler.GetShares_Limit_Session(false);
            Dictionary<long, Dictionary<long, Cal_SharesRiseLimitInfo>> result = new Dictionary<long, Dictionary<long, Cal_SharesRiseLimitInfo>>();
            var hotspot_shares_session = GetHotspotShares(shares_limit_session, accountId);

            int HighMarkLimitUpOrder = Singleton.Instance.HighMarkLimitUpOrder;
            int idx = 0;
            foreach (var item in list)
            {
                if (shares_limit_session.Contains(item.SharesKey.Value))
                {
                    continue;
                }
                idx++;
                foreach (var hotspot in hotspot_shares_session)
                {
                    if (!result.ContainsKey(hotspot.Value.AccountId))
                    {
                        result.Add(hotspot.Value.AccountId, new Dictionary<long, Cal_SharesRiseLimitInfo>());
                    }
                    var spotAccount = result[hotspot.Value.AccountId];
                    bool isAdd = false;
                    if (Singleton.Instance.HighMarkLimitUpIsHotSpot && hotspot.Value.AddCount<Singleton.Instance.HighMarkLimitUpHotSpotOrder && hotspot.Value.SharesKeyList.Contains(item.SharesKey.Value))
                    {
                        hotspot.Value.AddCount++;
                        isAdd = true;
                    }
                    else if (idx <= HighMarkLimitUpOrder)
                    {
                        isAdd = true;
                    }
                    if (isAdd && !spotAccount.ContainsKey(item.SharesKey.Value) && item.RiseLimitCount >= Singleton.Instance.HighMarkLimitUpCount) 
                    {
                        spotAccount.Add(item.SharesKey.Value,new Cal_SharesRiseLimitInfo
                        {
                            SharesKey=item.SharesKey.Value,
                            RiseLimitCount=item.RiseLimitCount,
                            RiseLimitDays=item.RiseLimitDays,
                            Date=item.DataDate
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取题材内股票列表
        /// </summary>
        /// <returns></returns>
        private static Dictionary<long, HotspotInfo> GetHotspotShares(List<long> shares_limit_session,long accountId=0)
        {
            var plate_shares_session = Singleton.Instance.sessionHandler.GetPlate_Real_Shares_Session(0, false);
            var hotspot_session = Singleton.Instance.sessionHandler.GetShares_Hotspot_Session(false);
            Dictionary<long, HotspotInfo> hotspot_shares_session = new Dictionary<long, HotspotInfo>();
            foreach (var hotspot in hotspot_session)
            {
                if (accountId > 0 && accountId != hotspot.Value.AccountId)
                {
                    continue;
                }
                List<long> sharesKeyList = new List<long>();
                foreach (long plateId in hotspot.Value.PlateIdList)
                {
                    if (plate_shares_session.ContainsKey(plateId))
                    {
                        sharesKeyList.AddRange(plate_shares_session[plateId]);
                    }
                }
                sharesKeyList = sharesKeyList.Distinct().ToList();
                sharesKeyList = (from sharesKey in sharesKeyList
                                 where !shares_limit_session.Contains(sharesKey)
                                 select sharesKey).ToList();
                hotspot_shares_session[hotspot.Key] = new HotspotInfo
                {
                    SharesKeyList = sharesKeyList,
                    AccountId = hotspot.Value.AccountId,
                    HotspotId = hotspot.Value.Id,
                    AddCount = 0
                };
            }
            return hotspot_shares_session;
        }

        /// <summary>
        /// 导入数据库
        /// </summary>
        private static void importToDatabase(Dictionary<long, Dictionary<long, Cal_SharesRiseLimitInfo>> data, DateTime date,long accountId=0) 
        {
            DataTable table = new DataTable();
            table.Columns.Add("AccountId", typeof(long));
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("SharesKey", typeof(long));
            table.Columns.Add("RiseLimitCount", typeof(int));
            table.Columns.Add("RiseLimitDays", typeof(Int32));

            foreach (var item in data)
            {
                foreach (var item2 in item.Value)
                {
                    DataRow row = table.NewRow();
                    row["AccountId"] = item.Key;
                    row["Date"] = date;
                    row["SharesKey"] = item2.Key;
                    row["RiseLimitCount"] = item2.Value.RiseLimitCount;
                    row["RiseLimitDays"] = item2.Value.RiseLimitDays;
                    table.Rows.Add(row);
                }
            }

            using (var db = new meal_ticketEntities())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        //关键是类型
                        SqlParameter parameter = new SqlParameter("@hotSpotHighMarkInfo", SqlDbType.Structured);
                        //必须指定表类型名
                        parameter.TypeName = "dbo.HotSpotHighMarkInfo";
                        //赋值
                        parameter.Value = table;

                        SqlParameter date_parameter = new SqlParameter("@date", SqlDbType.DateTime);
                        date_parameter.Value = date;

                        SqlParameter account_parameter = new SqlParameter("@accountId", SqlDbType.BigInt);
                        account_parameter.Value = accountId;

                        db.Database.ExecuteSqlCommand("exec P_Calculate_HotSpotHighMark @date,@accountId,@hotSpotHighMarkInfo", date_parameter, account_parameter, parameter);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("高标股票入库失败", ex);
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// 计算今日高标股票
        /// </summary>
        public static void Cal_HotSpotHighMark_Today()
        {
            var shares_limit_session = Singleton.Instance.sessionHandler.GetShares_Limit_Session(false);
            DateTime today_date = DbHelper.GetLastTradeDate2(0,0,0,0,DateTime.Now.AddHours(-9));
            DateTime pre_date = DbHelper.GetLastTradeDate2(0, 0, 0, -1, today_date);
            if (last_date != pre_date)
            {
                last_riselimit = GetSharesRiselimitDic(pre_date,-1,false);//昨日连板
                last_date = pre_date;
            }
            last_hotspot_shares_session = GetHotspotShares(shares_limit_session);//题材内股票
            foreach (var item in last_hotspot_shares_session)
            {
                item.Value.AddCount = 0;
            }

            var sharesRiselimitList = JsonConvert.DeserializeObject<List<t_shares_quotes_riselimit>>(JsonConvert.SerializeObject(last_riselimit));
            var shares_quotes_today_session = Singleton.Instance.sessionHandler.GetShares_Quotes_AppointDate_Session(today_date, false);//今日行情

            bool isTradeTime = DbHelper.CheckTradeTime7();
            foreach (var item in sharesRiselimitList)
            {
                if (shares_limit_session.Contains(item.SharesKey.Value))
                {
                    item.IsLimitUpToday = false;
                    continue;
                }
                if (shares_quotes_today_session.ContainsKey(item.SharesKey.Value))
                {
                    var shares_quotes_info = shares_quotes_today_session[item.SharesKey.Value];
                    if (shares_quotes_info.PriceType == 1)
                    {
                        item.RiseLimitCount++;
                        item.RiseLimitDays++;
                        item.LastRiseLimitDate = today_date;
                        item.LastRiseLimitTime = shares_quotes_info.LimitUpTime==null?DateTime.Parse(today_date.ToString("yyyy-MM-dd 14:57:00")) : shares_quotes_info.LimitUpTime.Value;
                        item.IsLimitUpYesday = item.IsLimitUpToday;
                        item.IsLimitUpToday = true;
                    }
                    else if (!isTradeTime)
                    {
                        item.IsLimitUpYesday = item.IsLimitUpToday;
                        item.IsLimitUpToday = false;
                    }
                }
            }
            sharesRiselimitList = sharesRiselimitList.Where(e=>e.IsLimitUpToday).OrderByDescending(e => e.RiseLimitCount).ThenByDescending(e => e.LastRiseLimitDate).ThenBy(e => e.LastRiseLimitTime).ToList();

            Dictionary<long, Dictionary<long, Cal_SharesRiseLimitInfo>> result = new Dictionary<long, Dictionary<long, Cal_SharesRiseLimitInfo>>();
            int HighMarkLimitUpOrder = Singleton.Instance.HighMarkLimitUpOrder;
            int idx = 0;
            foreach (var item in sharesRiselimitList)
            {
                idx++;
                foreach (var hotspot in last_hotspot_shares_session)
                {
                    if (!result.ContainsKey(hotspot.Value.AccountId))
                    {
                        result.Add(hotspot.Value.AccountId, new Dictionary<long, Cal_SharesRiseLimitInfo>());
                    }
                    var spotAccount = result[hotspot.Value.AccountId];
                    bool isAdd = false;
                    if (Singleton.Instance.HighMarkLimitUpIsHotSpot && hotspot.Value.AddCount<Singleton.Instance.HighMarkLimitUpHotSpotOrder && hotspot.Value.SharesKeyList.Contains(item.SharesKey.Value))
                    {
                        hotspot.Value.AddCount++;
                        isAdd = true;
                    }
                    else if (idx <= HighMarkLimitUpOrder)
                    {
                        isAdd = true;
                    }
                    if (isAdd && !spotAccount.ContainsKey(item.SharesKey.Value) && item.RiseLimitCount>=Singleton.Instance.HighMarkLimitUpCount)
                    {
                        spotAccount.Add(item.SharesKey.Value,new Cal_SharesRiseLimitInfo
                        {
                            SharesKey=item.SharesKey.Value,
                            RiseLimitCount=item.RiseLimitCount,
                            RiseLimitDays=item.RiseLimitDays,
                            Date = today_date
                        });
                    }
                }
            }
            TodaySharesHighMark = result;
        }
    }
}
