using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Runner
{
    public class SharesTagCalHelper
    {
        /// <summary>
        /// 计算股票标签
        /// </summary>
        public static void Calculate()
        {
            Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> Plate_Quotes_Date_Session = Singleton.Instance.sessionHandler.GetPlate_Quotes_Date_Session();
            Dictionary<long, Plate_Quotes_Session_Info> Plate_Quotes_Today_Session = Singleton.Instance.sessionHandler.GetPlate_Quotes_Today_Session();
            Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> Shares_Quotes_Date_Session = Singleton.Instance.sessionHandler.GetShares_Quotes_Date_Session();
            Dictionary<long, Shares_Quotes_Session_Info> Shares_Quotes_Today_Session = Singleton.Instance.sessionHandler.GetShares_Quotes_Today_Session();
            Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> Plate_Tag_FocusOn_Session = Singleton.Instance.sessionHandler.GetPlate_Tag_FocusOn_Session();
            Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> Plate_Tag_Force_Session = Singleton.Instance.sessionHandler.GetPlate_Tag_Force_Session();
            Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> Plate_Tag_TrendLike_Session = Singleton.Instance.sessionHandler.GetPlate_Tag_TrendLike_Session();
            Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> Plate_Shares_Rel_Tag_Setting_Session = Singleton.Instance.sessionHandler.GetPlate_Shares_Rel_Tag_Setting_Session();
            Dictionary<long, Shares_Base_Session_Info> Shares_Base_Session= Singleton.Instance.sessionHandler.GetShares_Base_Session();
            List<SharesPlateRelInfo_Session> PlateRel = new List<SharesPlateRelInfo_Session>();
            ToGetPlateRelSession(ref PlateRel);

            var calTypeArr = System.Enum.GetValues(typeof(Enum_SharesTag_CalType));
            int taskCount = calTypeArr.Length;
            TaskThread[] taskArr = new TaskThread[taskCount];
            int idx = 0;
            foreach (Enum_SharesTag_CalType calType in calTypeArr)
            {
                Enum_SharesTag_CalType _calType = calType;
                taskArr[idx] = new TaskThread();
                taskArr[idx].CreateTask(e =>
                {
                    _calculate(_calType, PlateRel, Plate_Quotes_Date_Session, Plate_Quotes_Today_Session, Shares_Quotes_Date_Session, Shares_Quotes_Today_Session, Plate_Tag_FocusOn_Session, Plate_Tag_Force_Session, Plate_Tag_TrendLike_Session, Plate_Shares_Rel_Tag_Setting_Session, Shares_Base_Session);
                });
                idx++;
            }
            TaskThread.WaitAll(ref taskArr);

        }

        private static void ToGetPlateRelSession(ref List<SharesPlateRelInfo_Session> PlateRel)
        {
            PlateRel = (from item in Singleton.Instance._SharesPlateSession.GetSessionData()
                        join item2 in Singleton.Instance._SharesPlateRelSession.GetSessionData() on item.PlateId equals item2.PlateId
                        where item.BaseStatus == 1 && item.PlateType == 3
                        select item2).ToList();
        }

        private static void _calculate(Enum_SharesTag_CalType calType, List<SharesPlateRelInfo_Session> plateRel, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> _plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> _plate_Quotes_Today_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> _shares_Quotes_Today_Session, Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> _plate_Tag_FocusOn_Session, Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> _plate_Tag_Force_Session, Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> _plate_Tag_TrendLike_Session,Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> _plate_Shares_Rel_Tag_Setting_Session, Dictionary<long, Shares_Base_Session_Info> _shares_Base_Session)
        {
            switch (calType)
            {
                case Enum_SharesTag_CalType.LEADER:
                    DoLeaderHandler(plateRel, _plate_Quotes_Date_Session, _plate_Quotes_Today_Session, _shares_Quotes_Date_Session, _shares_Quotes_Today_Session, _plate_Tag_FocusOn_Session, _plate_Tag_Force_Session, _plate_Tag_TrendLike_Session, _plate_Shares_Rel_Tag_Setting_Session);
                    break;
                case Enum_SharesTag_CalType.DAYLEADER:
                    DoDayLeaderHandler(plateRel, _plate_Quotes_Date_Session, _plate_Quotes_Today_Session, _shares_Quotes_Date_Session, _shares_Quotes_Today_Session, _plate_Tag_FocusOn_Session, _plate_Tag_Force_Session, _plate_Tag_TrendLike_Session, _plate_Shares_Rel_Tag_Setting_Session);
                    break;
                case Enum_SharesTag_CalType.MAINARMY:
                    DoMainArmyHandler(plateRel, _plate_Quotes_Date_Session, _plate_Quotes_Today_Session, _shares_Quotes_Date_Session, _shares_Quotes_Today_Session, _plate_Tag_FocusOn_Session, _plate_Tag_Force_Session, _plate_Tag_TrendLike_Session, _plate_Shares_Rel_Tag_Setting_Session, _shares_Base_Session);
                    break;
                default:
                    break;
            }
        }

        private static void DoLeaderHandler(List<SharesPlateRelInfo_Session> plateRel, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> _plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> _plate_Quotes_Today_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> _shares_Quotes_Today_Session, Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> _plate_Tag_FocusOn_Session, Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> _plate_Tag_Force_Session, Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> _plate_Tag_TrendLike_Session,Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> _plate_Shares_Rel_Tag_Setting_Session)
        {
            var dayTypeArr = System.Enum.GetValues(typeof(Enum_PlateTag_DayType));
            int dayTypeCount = dayTypeArr.Length;
            Task[] taskArr = new Task[dayTypeCount];
            int idx = 0;
            List<Shares_Tag_Leader_Session_Info> new_data = new List<Shares_Tag_Leader_Session_Info>();
            object dayTypeLock = new object();
            foreach (Enum_PlateTag_DayType item in dayTypeArr)
            {
                int dayType = (int)item;
                taskArr[idx] = Task.Factory.StartNew(() =>
                {
                    var _plateRel = new List<SharesPlateRelInfo_Session>();
                    CheckPlateTagExist(dayType, plateRel, _plate_Tag_FocusOn_Session, _plate_Tag_Force_Session, _plate_Tag_TrendLike_Session, ref _plateRel);
                    var plate_Quotes_Date_Session = new Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>>();
                    var shares_Quotes_Date_Session = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
                    int diffDay = _shares_Quotes_Today_Session.Count() == 0 ? 0 : 1;
                    BuidForcePar(diffDay,dayType, _plate_Quotes_Date_Session, _shares_Quotes_Date_Session, ref plate_Quotes_Date_Session, ref shares_Quotes_Date_Session);

                    Dictionary<long, List<SharesPlateRelInfo_Session>> plateRelDic = _plateRel.GroupBy(e => e.PlateId).ToDictionary(k => k.Key, v => v.ToList());

                    List<Shares_Tag_Leader_Session_Info> tempData = _doLeaderHandler(plateRelDic, dayType, plate_Quotes_Date_Session, _plate_Quotes_Today_Session, shares_Quotes_Date_Session, _shares_Quotes_Today_Session, _plate_Shares_Rel_Tag_Setting_Session);
                    lock (dayTypeLock)
                    {
                        new_data.AddRange(tempData);
                    }
                });
                idx++;
            }
            Task.WaitAll(taskArr);
            //设置缓存
            Singleton.Instance.sessionHandler.UpdateSessionPart((int)SessionHandler.Enum_Excute_Type.Shares_Tag_Leader_Session, new_data);
            //入库
            try
            {
                DoLeaderToDataBase(new_data);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("龙头计算入库失败", ex);
            }
        }

        private static List<Shares_Tag_Leader_Session_Info> _doLeaderHandler(Dictionary<long, List<SharesPlateRelInfo_Session>> plateRelDic,int dayType, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> _plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> _plate_Quotes_Today_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> _shares_Quotes_Today_Session, Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> _plate_Shares_Rel_Tag_Setting_Session)
        {
            List<Shares_Tag_Leader_Session_Info> new_data = new List<Shares_Tag_Leader_Session_Info>();
            foreach (var item in plateRelDic)
            {
                long plateId = item.Key;
                //1.查询板块最低点日期
                DateTime lowestDate = DateTime.Now.Date;
                DateTime lastDate = DateTime.Now.Date;
                bool isVaild = QueryPlateLowestDate(plateId, _plate_Quotes_Date_Session, _plate_Quotes_Today_Session, ref lowestDate, ref lastDate);
                if (!isVaild)
                {
                    continue;
                }
                //2.查询板块涨幅最高排名
                int disCount=GetDisCount((int)Enum_SharesTag_CalType.LEADER, item.Value.Count(), _plate_Shares_Rel_Tag_Setting_Session);
                if (disCount <= 0)
                {
                    continue;
                }
                var new_data_temp = _toCalLeader(disCount,dayType, item.Value, lowestDate, lastDate, _shares_Quotes_Date_Session, _shares_Quotes_Today_Session);
                new_data.AddRange(new_data_temp);
            }
            return new_data;
        }

        private static bool QueryPlateLowestDate(long plateId, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> _plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> _plate_Quotes_Today_Session, ref DateTime lowestDate, ref DateTime lastDate)
        {
            Dictionary<DateTime, Plate_Quotes_Session_Info> plate_list = new Dictionary<DateTime, Plate_Quotes_Session_Info>();
            if (!_plate_Quotes_Date_Session.TryGetValue(plateId, out plate_list))
            {
                return false;
            }
            lastDate = DateTime.Now.Date;
            Plate_Quotes_Session_Info plate_today = new Plate_Quotes_Session_Info();
            if (!_plate_Quotes_Today_Session.TryGetValue(plateId, out plate_today))
            {
                var temp = plate_list.OrderByDescending(e => e.Key).FirstOrDefault();
                plate_today = temp.Value;
                lastDate = temp.Key;
            }
            Plate_Quotes_Session_Info lowestInfo = null;
            foreach (var item in plate_list)
            {
                if (lowestInfo == null)
                {
                    lowestInfo = item.Value;
                    lowestDate = item.Key;
                    continue;
                }
                if (lowestInfo.ClosedPrice > item.Value.ClosedPrice)
                {
                    lowestInfo = item.Value;
                    lowestDate = item.Key;
                }
            }
            if (lowestInfo.ClosedPrice >= plate_today.ClosedPrice)
            {
                return false;
            }
            return true;
        }

        private static List<Shares_Tag_Leader_Session_Info> _toCalLeader(int disCount, int dayType, List<SharesPlateRelInfo_Session> sharesList, DateTime lowestDate, DateTime lastDate, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> _shares_Quotes_Today_Session)
        {
            foreach (var item in sharesList)
            {
                long key = long.Parse(item.SharesCode) * 10 + item.Market;
                Dictionary<DateTime, Shares_Quotes_Session_Info> shares_quotes_date_dic = new Dictionary<DateTime, Shares_Quotes_Session_Info>();
                if (!_shares_Quotes_Date_Session.TryGetValue(key, out shares_quotes_date_dic))
                {
                    continue;
                }
                Shares_Quotes_Session_Info shares_quotes_date = new Shares_Quotes_Session_Info();
                if (!shares_quotes_date_dic.TryGetValue(lowestDate, out shares_quotes_date))
                {
                    continue;
                }

                Shares_Quotes_Session_Info shares_quotes_today = new Shares_Quotes_Session_Info();
                if (lastDate == DateTime.Now.Date)
                {
                    if (!_shares_Quotes_Today_Session.TryGetValue(key, out shares_quotes_today))
                    {
                        continue;
                    }
                }
                else
                {
                    if (!shares_quotes_date_dic.TryGetValue(lastDate, out shares_quotes_today))
                    {
                        continue;
                    }
                }
                if (shares_quotes_date.ClosedPrice >= shares_quotes_today.ClosedPrice)
                {
                    continue;
                }
                item.RiseRate = (int)((shares_quotes_today.ClosedPrice - shares_quotes_date.ClosedPrice) * 1.0 / shares_quotes_date.ClosedPrice * 10000 + 0.5);
            }

            List<Shares_Tag_Leader_Session_Info> new_data = new List<Shares_Tag_Leader_Session_Info>();
            List<SharesPlateRelInfo_Session> tempList = sharesList.OrderByDescending(e => e.RiseRate).ToList();
            int idx = 1;
            foreach (var item in tempList)
            {
                int LeaderType = 0;
                if (idx > disCount)
                {
                    LeaderType = 0;
                }
                else if (item.RiseRate <= 0)
                {
                    LeaderType = 0;
                }
                else
                {
                    LeaderType = idx;
                    idx++;
                }

                new_data.Add(new Shares_Tag_Leader_Session_Info
                {
                    LeaderType = LeaderType,
                    Type = dayType,
                    PlateId = item.PlateId,
                    Market = item.Market,
                    SharesCode = item.SharesCode
                });
            }
            return new_data;
        }

        private static void DoLeaderToDataBase(List<Shares_Tag_Leader_Session_Info> new_data_list) 
        {
            DataTable table = new DataTable();
            table.Columns.Add("PlateId", typeof(long));
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            table.Columns.Add("Type", typeof(int));
            table.Columns.Add("LeaderType", typeof(int));

            foreach (var item in new_data_list)
            {
                DataRow row = table.NewRow();
                row["PlateId"] = item.PlateId;
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                row["Type"] = item.Type;
                row["LeaderType"] = item.LeaderType;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesTagLeaderSessionInfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesTagLeaderSessionInfo";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_Shares_Tag_Leader_Session_Update @sharesTagLeaderSessionInfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("龙头计算入库失败", ex);
                    tran.Rollback();
                }
            }
        }

        private static void DoDayLeaderHandler(List<SharesPlateRelInfo_Session> plateRel, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> _plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> _plate_Quotes_Today_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> _shares_Quotes_Today_Session, Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> _plate_Tag_FocusOn_Session, Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> _plate_Tag_Force_Session, Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> _plate_Tag_TrendLike_Session, Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> _plate_Shares_Rel_Tag_Setting_Session)
        {
            var dayTypeArr = System.Enum.GetValues(typeof(Enum_PlateTag_DayType));
            int dayTypeCount = dayTypeArr.Length;
            Task[] taskArr = new Task[dayTypeCount];
            int idx = 0;
            List<Shares_Tag_DayLeader_Session_Info> new_data = new List<Shares_Tag_DayLeader_Session_Info>();
            object dayTypeLock = new object();
            foreach (Enum_PlateTag_DayType item in dayTypeArr)
            {
                int dayType = (int)item;
                taskArr[idx] = Task.Factory.StartNew(() =>
                {
                    var _plateRel = new List<SharesPlateRelInfo_Session>();
                    CheckPlateTagExist(dayType, plateRel, _plate_Tag_FocusOn_Session, _plate_Tag_Force_Session, _plate_Tag_TrendLike_Session, ref _plateRel);
                    var plate_Quotes_Date_Session = new Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>>();
                    var shares_Quotes_Date_Session = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
                    int diffDay = _shares_Quotes_Today_Session.Count() == 0 ? 0 : 1;
                    BuidForcePar(diffDay, dayType, _plate_Quotes_Date_Session, _shares_Quotes_Date_Session, ref plate_Quotes_Date_Session, ref shares_Quotes_Date_Session);

                    Dictionary<long, List<SharesPlateRelInfo_Session>> plateRelDic = _plateRel.GroupBy(e => e.PlateId).ToDictionary(k => k.Key, v => v.ToList());

                    List<Shares_Tag_DayLeader_Session_Info> tempData = _doDayLeaderHandler(plateRelDic, dayType, plate_Quotes_Date_Session, _plate_Quotes_Today_Session, shares_Quotes_Date_Session, _shares_Quotes_Today_Session, _plate_Shares_Rel_Tag_Setting_Session);
                    lock (dayTypeLock)
                    {
                        new_data.AddRange(tempData);
                    }
                });
                idx++;
            }
            Task.WaitAll(taskArr);
            //设置缓存
            Singleton.Instance.sessionHandler.UpdateSessionPart((int)SessionHandler.Enum_Excute_Type.Shares_Tag_DayLeader_Session, new_data);
            //入库
            try
            {
                DoDayLeaderToDataBase(new_data);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("日内龙头计算入库失败", ex);
            }
        }

        private static List<Shares_Tag_DayLeader_Session_Info> _doDayLeaderHandler(Dictionary<long, List<SharesPlateRelInfo_Session>> plateRelDic, int dayType, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> _plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> _plate_Quotes_Today_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> _shares_Quotes_Today_Session, Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> _plate_Shares_Rel_Tag_Setting_Session)
        {
            List<Shares_Tag_DayLeader_Session_Info> new_data = new List<Shares_Tag_DayLeader_Session_Info>();
            foreach (var item in plateRelDic)
            {
                int disCount = GetDisCount((int)Enum_SharesTag_CalType.DAYLEADER, item.Value.Count(), _plate_Shares_Rel_Tag_Setting_Session);
                if (disCount <= 0)
                {
                    continue;
                }
                var new_data_temp = _toCalDayLeader(disCount, dayType, item.Value, _shares_Quotes_Date_Session, _shares_Quotes_Today_Session);
                new_data.AddRange(new_data_temp);
            }
            return new_data;
        }

        private static List<Shares_Tag_DayLeader_Session_Info> _toCalDayLeader(int disCount,int dayType,List<SharesPlateRelInfo_Session> sharesList, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> _shares_Quotes_Today_Session) 
        {
            foreach (var item in sharesList)
            {
                long key = long.Parse(item.SharesCode) * 10 + item.Market;
                Dictionary<DateTime, Shares_Quotes_Session_Info> shares_quotes_date_dic = new Dictionary<DateTime, Shares_Quotes_Session_Info>();
                if (!_shares_Quotes_Date_Session.TryGetValue(key, out shares_quotes_date_dic))
                {
                    continue;
                }
                Shares_Quotes_Session_Info today_info = new Shares_Quotes_Session_Info();
                if (!_shares_Quotes_Today_Session.ContainsKey(key))
                {
                    today_info = shares_quotes_date_dic.OrderByDescending(e => e.Key).FirstOrDefault().Value;
                }
                else
                {
                    today_info = _shares_Quotes_Today_Session[key];
                }
                if (!today_info.IsLimitUp)
                {
                    continue;
                }
                if (today_info.LimitUpTime == null)
                {
                    continue;
                }
                item.LimitUpTime = today_info.LimitUpTime.Value;
            }

            List<Shares_Tag_DayLeader_Session_Info> new_data = new List<Shares_Tag_DayLeader_Session_Info>();
            List<SharesPlateRelInfo_Session> tempList = sharesList.OrderBy(e => e.LimitUpTime).ToList();
            int idx = 1;
            foreach (var item in tempList)
            {
                int DayLeaderType = 0;
                if (idx > disCount)
                {
                    DayLeaderType = 0;
                }
                else if (item.LimitUpTime == null)
                {
                    DayLeaderType = 0;
                }
                else
                {
                    DayLeaderType = idx;
                    idx++;
                }

                new_data.Add(new Shares_Tag_DayLeader_Session_Info
                {
                    DayLeaderType = DayLeaderType,
                    Type = dayType,
                    PlateId = item.PlateId,
                    Market = item.Market,
                    SharesCode = item.SharesCode
                });
            }
            return new_data;
        }

        private static void DoDayLeaderToDataBase(List<Shares_Tag_DayLeader_Session_Info> new_data_list)
        {
            DataTable table = new DataTable();
            table.Columns.Add("PlateId", typeof(long));
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            table.Columns.Add("Type", typeof(int));
            table.Columns.Add("DayLeaderType", typeof(int));

            foreach (var item in new_data_list)
            {
                DataRow row = table.NewRow();
                row["PlateId"] = item.PlateId;
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                row["Type"] = item.Type;
                row["DayLeaderType"] = item.DayLeaderType;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesTagDayLeaderSessionInfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesTagDayLeaderSessionInfo";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_Shares_Tag_DayLeader_Session_Update @sharesTagDayLeaderSessionInfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("日内龙头计算入库失败", ex);
                    tran.Rollback();
                }
            }
        }

        private static void DoMainArmyHandler(List<SharesPlateRelInfo_Session> plateRel, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> _plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> _plate_Quotes_Today_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> _shares_Quotes_Today_Session, Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> _plate_Tag_FocusOn_Session, Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> _plate_Tag_Force_Session, Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> _plate_Tag_TrendLike_Session, Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> _plate_Shares_Rel_Tag_Setting_Session,Dictionary<long, Shares_Base_Session_Info> _shares_Base_Session)
        {
            var dayTypeArr = System.Enum.GetValues(typeof(Enum_PlateTag_DayType));
            int dayTypeCount = dayTypeArr.Length;
            Task[] taskArr = new Task[dayTypeCount];
            int idx = 0;
            List<Shares_Tag_MainArmy_Session_Info> new_data = new List<Shares_Tag_MainArmy_Session_Info>();
            object dayTypeLock = new object();
            foreach (Enum_PlateTag_DayType item in dayTypeArr)
            {
                int dayType = (int)item;
                taskArr[idx] = Task.Factory.StartNew(() =>
                {
                    var _plateRel = new List<SharesPlateRelInfo_Session>();
                    CheckPlateTagExist(dayType, plateRel, _plate_Tag_FocusOn_Session, _plate_Tag_Force_Session, _plate_Tag_TrendLike_Session, ref _plateRel);
                    var plate_Quotes_Date_Session = new Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>>();
                    var shares_Quotes_Date_Session = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
                    int diffDay = _shares_Quotes_Today_Session.Count() == 0 ? 0 : 1;
                    BuidForcePar(diffDay, dayType, _plate_Quotes_Date_Session, _shares_Quotes_Date_Session, ref plate_Quotes_Date_Session, ref shares_Quotes_Date_Session);

                    Dictionary<long, List<SharesPlateRelInfo_Session>> plateRelDic = _plateRel.GroupBy(e => e.PlateId).ToDictionary(k => k.Key, v => v.ToList());

                    List<Shares_Tag_MainArmy_Session_Info> tempData = _doMainArmyHandler(plateRelDic, dayType, plate_Quotes_Date_Session, _plate_Quotes_Today_Session, shares_Quotes_Date_Session, _shares_Quotes_Today_Session, _plate_Shares_Rel_Tag_Setting_Session, _plate_Tag_TrendLike_Session, _shares_Base_Session);
                    lock (dayTypeLock)
                    {
                        new_data.AddRange(tempData);
                    }
                });
                idx++;
            }
            Task.WaitAll(taskArr);
            //设置缓存
            Singleton.Instance.sessionHandler.UpdateSessionPart((int)SessionHandler.Enum_Excute_Type.Shares_Tag_MainArmy_Session, new_data);
            //入库
            try
            {
                DoMainArmyToDataBase(new_data);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("中军计算入库失败", ex);
            }
        }

        private static List<Shares_Tag_MainArmy_Session_Info> _doMainArmyHandler(Dictionary<long, List<SharesPlateRelInfo_Session>> plateRelDic, int dayType, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> _plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> _plate_Quotes_Today_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> _shares_Quotes_Today_Session, Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> _plate_Shares_Rel_Tag_Setting_Session,Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> _plate_Tag_TrendLike_Session,Dictionary<long, Shares_Base_Session_Info> _shares_Base_Session)
        {
            List<Shares_Tag_MainArmy_Session_Info> new_data = new List<Shares_Tag_MainArmy_Session_Info>();
            foreach (var item in plateRelDic)
            {
                int disCount = GetDisCount((int)Enum_SharesTag_CalType.MAINARMY, item.Value.Count(), _plate_Shares_Rel_Tag_Setting_Session);
                if (disCount <= 0)
                {
                    continue;
                }
                var new_data_temp = _toCalMainArmy(disCount, dayType, item.Value, _shares_Quotes_Date_Session, _shares_Quotes_Today_Session, _plate_Tag_TrendLike_Session, _shares_Base_Session);
                new_data.AddRange(new_data_temp);
            }
            return new_data;
        }

        private static List<Shares_Tag_MainArmy_Session_Info> _toCalMainArmy(int disCount, int dayType, List<SharesPlateRelInfo_Session> sharesList, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> _shares_Quotes_Today_Session, Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> _plate_Tag_TrendLike_Session, Dictionary<long, Shares_Base_Session_Info> _shares_Base_Session)
        {
            foreach (var item in sharesList)
            {
                long key = long.Parse(item.SharesCode) * 10 + item.Market;
                Dictionary<DateTime, Shares_Quotes_Session_Info> shares_quotes_date_dic = new Dictionary<DateTime, Shares_Quotes_Session_Info>();
                if (!_shares_Quotes_Date_Session.TryGetValue(key, out shares_quotes_date_dic))
                {
                    continue;
                }
                Shares_Quotes_Session_Info today_info = new Shares_Quotes_Session_Info();
                if (!_shares_Quotes_Today_Session.ContainsKey(key))
                {
                    today_info = shares_quotes_date_dic.OrderByDescending(e => e.Key).FirstOrDefault().Value;
                }
                else
                {
                    today_info = _shares_Quotes_Today_Session[key];
                }

                if (!_shares_Base_Session.ContainsKey(key))
                {
                    continue;
                }
                if (!_plate_Tag_TrendLike_Session.ContainsKey(key))
                {
                    continue;
                }
                if (!_plate_Tag_TrendLike_Session[key].ContainsKey(item.PlateId))
                {
                    continue;
                }
                item.Score = _plate_Tag_TrendLike_Session[key][item.PlateId].Score;
                item.MarketValue = today_info.ClosedPrice * _shares_Base_Session[key].TotalCapital;
            }

            int mainArmyRankRate = Singleton.Instance.MainArmyRankRate;
            int takeCount = (int)(sharesList.Count() * (mainArmyRankRate * 1.0 / 10000));
            var rankList = sharesList.OrderByDescending(e => e.MarketValue).Take(takeCount).ToList();
            rankList=rankList.Where(e => e.Score >= 0).OrderBy(e => e.Score).ToList();

            Dictionary<long, int> sharesDic = new Dictionary<long, int>();
            int idx = 1;
            foreach (var item in rankList)
            {
                if (idx > disCount)
                {
                    continue;
                }
                long key = long.Parse(item.SharesCode) * 10 + item.Market;
                sharesDic.Add(key, idx);
                idx++;
            }

            List<Shares_Tag_MainArmy_Session_Info> new_data = new List<Shares_Tag_MainArmy_Session_Info>();
            foreach (var item in sharesList)
            {
                int MainArmyType = 0;
                long key = long.Parse(item.SharesCode) * 10 + item.Market;
                if (sharesDic.ContainsKey(key))
                {
                    MainArmyType = sharesDic[key];
                }
                new_data.Add(new Shares_Tag_MainArmy_Session_Info
                {
                    MainArmyType = MainArmyType,
                    Type = dayType,
                    PlateId = item.PlateId,
                    Market = item.Market,
                    SharesCode = item.SharesCode
                });
            }
            return new_data;
        }

        private static void DoMainArmyToDataBase(List<Shares_Tag_MainArmy_Session_Info> new_data_list)
        {
            DataTable table = new DataTable();
            table.Columns.Add("PlateId", typeof(long));
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            table.Columns.Add("Type", typeof(int));
            table.Columns.Add("MainArmyType", typeof(int));

            foreach (var item in new_data_list)
            {
                DataRow row = table.NewRow();
                row["PlateId"] = item.PlateId;
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                row["Type"] = item.Type;
                row["MainArmyType"] = item.MainArmyType;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesTagMainArmySessionInfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesTagMainArmySessionInfo";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_Shares_Tag_MainArmy_Session_Update @sharesTagMainArmySessionInfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("中军计算入库失败", ex);
                    tran.Rollback();
                }
            }
        }

        private static void CheckPlateTagExist(int dayType, List<SharesPlateRelInfo_Session> PlateRel, Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> Plate_Tag_FocusOn_Session, Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> Plate_Tag_Force_Session, Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> Plate_Tag_TrendLike_Session,ref List<SharesPlateRelInfo_Session> _plateRel)
        {
            _plateRel = new List<SharesPlateRelInfo_Session>();
            foreach (var item in PlateRel)
            {
                bool isVaild = false;
                long key1 = long.Parse(item.SharesCode) * 10 + item.Market;
                long key2 = item.PlateId;
                if (Plate_Tag_FocusOn_Session.ContainsKey(key1))
                {
                    if (Plate_Tag_FocusOn_Session[key1].ContainsKey(key2))
                    {
                        if (Plate_Tag_FocusOn_Session[key1][key2].IsFocusOn)
                        {
                            isVaild = true;
                        }
                    }
                }
                if (Plate_Tag_TrendLike_Session.ContainsKey(key1))
                {
                    if (Plate_Tag_TrendLike_Session[key1].ContainsKey(key2))
                    {
                        if (Plate_Tag_TrendLike_Session[key1][key2].IsTrendLike)
                        {
                            isVaild = true;
                        }
                    }
                }
                key1 = long.Parse(item.SharesCode) * 1000 + item.Market * 100 + dayType;
                if (Plate_Tag_Force_Session.ContainsKey(key1))
                {
                    if (Plate_Tag_Force_Session[key1].ContainsKey(key2))
                    {
                        if (Plate_Tag_Force_Session[key1][key2].IsForce1 || Plate_Tag_Force_Session[key1][key2].IsForce2)
                        {
                            isVaild = true;
                        }
                    }
                }

                if (isVaild)
                {
                    _plateRel.Add(item);
                }
            }
        }

        private static void BuidForcePar(int diffDay,int dayType, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> Plate_Quotes_Date_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> Shares_Quotes_Date_Session, ref Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> _plate_Quotes_Date_Session, ref Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session)
        {
            int takeCount = 0;
            switch (dayType)
            {
                case (int)Enum_PlateTag_DayType.FORCE_3DAYS:
                    takeCount = 3 - diffDay;
                    break;
                case (int)Enum_PlateTag_DayType.FORCE_5DAYS:
                    takeCount = 5 - diffDay;
                    break;
                case (int)Enum_PlateTag_DayType.FORCE_10DAYS:
                    takeCount = 10 - diffDay;
                    break;
                case (int)Enum_PlateTag_DayType.FORCE_15DAYS:
                    takeCount = 15 - diffDay;
                    break;
                default:
                    break;
            }
            foreach (var item in Plate_Quotes_Date_Session)
            {
                var temp = item.Value.OrderByDescending(e => e.Key).Take(takeCount).ToDictionary(k => k.Key, v => v.Value);
                _plate_Quotes_Date_Session.Add(item.Key, temp);
            }
            foreach (var item in Shares_Quotes_Date_Session)
            {
                var temp = item.Value.OrderByDescending(e => e.Key).Take(takeCount).ToDictionary(k => k.Key, v => v.Value);
                _shares_Quotes_Date_Session.Add(item.Key, temp);
            }
        }

        private static int GetDisCount(int calType, int sharesCount, Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> Plate_Shares_Rel_Tag_Setting_Session) 
        {
            if (!Plate_Shares_Rel_Tag_Setting_Session.ContainsKey(calType))
            {
                return 0;
            }
            var list = Plate_Shares_Rel_Tag_Setting_Session[calType];
            foreach (var item in list)
            {
                if (sharesCount <= item.BaseCount)
                {
                    return item.DisCount;
                }
            }
            return 0;
        }
    }
}
