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
    enum Enum_CalType
    {
        FORCE,//强势
        TRENDLIKE,//走势最像
    }
    enum Enum_ForceType
    {
        FORCE_3DAYS=1,
        FORCE_5DAYS=2,
        FORCE_10DAYS=3,
        FORCE_15DAYS=4
    }

    public class PlateTagCalHelper
    {
        /// <summary>
        /// 计算板块标记
        /// </summary>
        public static void Calculate() 
        {
            var calTypeArr = System.Enum.GetValues(typeof(Enum_CalType));
            int taskCount = calTypeArr.Length;
            Task[] taskArr = new Task[taskCount];
            int idx = 0;
            foreach (Enum_CalType calType in calTypeArr)
            {
                Enum_CalType _calType = calType;
                taskArr[idx] = Task.Factory.StartNew(() =>
                {
                    _calculate(_calType);
                });
                idx++;
            }
            Task.WaitAll(taskArr);
        }

        private static void _calculate(Enum_CalType type)
        {
            var Plate_Quotes_Date_Session = Singleton.Instance.sessionHandler.GetPlate_Quotes_Date_Session();
            var Plate_Quotes_Today_Session = Singleton.Instance.sessionHandler.GetPlate_Quotes_Today_Session();
            var Shares_Quotes_Date_Session = Singleton.Instance.sessionHandler.GetShares_Quotes_Date_Session();
            var Shares_Quotes_Today_Session = Singleton.Instance.sessionHandler.GetShares_Quotes_Today_Session();
            var _plate_Quotes_Date_Session = Utils.DeepCopyWithBinarySerialize(Plate_Quotes_Date_Session);
            var _plate_Quotes_Today_Session = Utils.DeepCopyWithBinarySerialize(Plate_Quotes_Today_Session);
            var _shares_Quotes_Date_Session = Utils.DeepCopyWithBinarySerialize(Shares_Quotes_Date_Session);
            var _shares_Quotes_Today_Session = Utils.DeepCopyWithBinarySerialize(Shares_Quotes_Today_Session);


            var plateRel = (from item in Singleton.Instance._SharesPlateSession.GetSessionData()
                            join item2 in Singleton.Instance._SharesPlateRelSession.GetSessionData() on item.PlateId equals item2.PlateId
                            where item.BaseStatus == 1 && item.PlateType == 3
                            select item2).ToList();
            switch (type)
            {
                case Enum_CalType.FORCE:
                    DoForceHandler(plateRel,_plate_Quotes_Date_Session, _plate_Quotes_Today_Session, _shares_Quotes_Date_Session, _shares_Quotes_Today_Session);
                    break;
                case Enum_CalType.TRENDLIKE:
                    DoTrendLikeHandler(plateRel,_plate_Quotes_Date_Session, _plate_Quotes_Today_Session, _shares_Quotes_Date_Session, _shares_Quotes_Today_Session);
                    break;
                default:
                    break;
            }
        }

        private static void DoForceHandler(List<SharesPlateRelInfo_Session> plateRel, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> Plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> Plate_Quotes_Today_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> Shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> Shares_Quotes_Today_Session)
        {
            var plateRelDic = plateRel.GroupBy(e => new { e.Market, e.SharesCode }).ToDictionary(k => long.Parse(k.Key.SharesCode) * 10 + k.Key.Market, v => v.ToList());
            
            var forceTypeArr = System.Enum.GetValues(typeof(Enum_ForceType));
            int forceTypeCount = forceTypeArr.Length;
            Task[] taskArr = new Task[forceTypeCount];
            int idx = 0;
            List<Plate_Tag_Force_Session_Info> new_force_data = new List<Plate_Tag_Force_Session_Info>();
            object forceTypeLock = new object();
            foreach (Enum_ForceType item in forceTypeArr)
            {
                int forceType = (int)item;
                taskArr[idx] = Task.Factory.StartNew(() =>
                {
                    List<Plate_Tag_Force_Session_Info> tempData = _doForceHandler(plateRelDic, forceType, Plate_Quotes_Date_Session, Plate_Quotes_Today_Session, Shares_Quotes_Date_Session, Shares_Quotes_Today_Session);
                    lock (forceTypeLock)
                    {
                        new_force_data.AddRange(tempData);
                    }
                });
                idx++;
            }
            Task.WaitAll(taskArr);
            //设置缓存
            Singleton.Instance.sessionHandler.UpdateSessionPart((int)SessionHandler.Enum_Excute_Type.Plate_Tag_Force_Session, new_force_data);
            //入库
            try
            {
                DoForceToDataBase(new_force_data);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("强势板块计算入库失败",ex);
            }
        }

        private static List<Plate_Tag_Force_Session_Info> _doForceHandler(Dictionary<long,List<SharesPlateRelInfo_Session>> plateRelDic,int forceType, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> Plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> Plate_Quotes_Today_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> Shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> Shares_Quotes_Today_Session)
        {
            Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> _plate_Quotes_Date_Session = new Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>>();
            Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>(); ;
            _buidForcePar(forceType, Plate_Quotes_Date_Session, Shares_Quotes_Date_Session,ref _plate_Quotes_Date_Session,ref _shares_Quotes_Date_Session);

            List<Plate_Tag_Force_Session_Info> new_force_data = new List<Plate_Tag_Force_Session_Info>();
            foreach (var item in plateRelDic)
            {
                //1.查询股票最低点涨跌幅日期
                DateTime lowestDate = DateTime.Now.Date;
                bool isVaild = QuerySharesLowestDate(item.Key, _shares_Quotes_Date_Session, Shares_Quotes_Today_Session, ref lowestDate);
                if (!isVaild)
                {
                    continue;
                }
                //2.查询板块涨幅最高前两名
                var new_force_data_temp=QueryMaxRiseRatePlate(forceType,item.Value, lowestDate, _plate_Quotes_Date_Session, Plate_Quotes_Today_Session);
                new_force_data.AddRange(new_force_data_temp);
            }
            return new_force_data;
        }

        private static void _buidForcePar(int forceType,Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> Plate_Quotes_Date_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> Shares_Quotes_Date_Session,ref Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> _plate_Quotes_Date_Session, ref Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> _shares_Quotes_Date_Session) 
        {
            switch (forceType)
            {
                case (int)Enum_ForceType.FORCE_3DAYS:
                    _plate_Quotes_Date_Session = Plate_Quotes_Date_Session.OrderByDescending(e => e.Key).Take(3 - 1).ToDictionary(k => k.Key, v => v.Value);
                    _shares_Quotes_Date_Session = Shares_Quotes_Date_Session.OrderByDescending(e => e.Key).Take(3 - 1).ToDictionary(k => k.Key, v => v.Value);
                    break;
                case (int)Enum_ForceType.FORCE_5DAYS:
                    _plate_Quotes_Date_Session = Plate_Quotes_Date_Session.OrderByDescending(e => e.Key).Take(5 - 1).ToDictionary(k => k.Key, v => v.Value);
                    _shares_Quotes_Date_Session = Shares_Quotes_Date_Session.OrderByDescending(e => e.Key).Take(5 - 1).ToDictionary(k => k.Key, v => v.Value);
                    break;
                case (int)Enum_ForceType.FORCE_10DAYS:
                    _plate_Quotes_Date_Session = Plate_Quotes_Date_Session.OrderByDescending(e => e.Key).Take(10 - 1).ToDictionary(k => k.Key, v => v.Value);
                    _shares_Quotes_Date_Session = Shares_Quotes_Date_Session.OrderByDescending(e => e.Key).Take(10 - 1).ToDictionary(k => k.Key, v => v.Value);
                    break;
                case (int)Enum_ForceType.FORCE_15DAYS:
                    _plate_Quotes_Date_Session = Plate_Quotes_Date_Session.OrderByDescending(e => e.Key).Take(15 - 1).ToDictionary(k => k.Key, v => v.Value);
                    _shares_Quotes_Date_Session = Shares_Quotes_Date_Session.OrderByDescending(e => e.Key).Take(15 - 1).ToDictionary(k => k.Key, v => v.Value);
                    break;
                default:
                    break;
            }
        }

        private static bool QuerySharesLowestDate(long sharesKey, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> Shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> Shares_Quotes_Today_Session,ref DateTime lowestDate)
        {
            Dictionary<DateTime, Shares_Quotes_Session_Info> shares_list = new Dictionary<DateTime, Shares_Quotes_Session_Info>();
            if (!Shares_Quotes_Date_Session.TryGetValue(sharesKey, out shares_list))
            {
                return false;
            }
            Shares_Quotes_Session_Info shares_today = new Shares_Quotes_Session_Info();
            if (!Shares_Quotes_Today_Session.TryGetValue(sharesKey, out shares_today))
            {
                return false;
            }
            Shares_Quotes_Session_Info lowestInfo = null;
            foreach (var item in shares_list)
            {
                if (lowestInfo == null)
                {
                    lowestInfo = item.Value;
                    lowestDate = item.Key;
                    continue;
                }
                if (lowestInfo.RiseRate > item.Value.RiseRate)
                {
                    lowestInfo = item.Value;
                    lowestDate = item.Key;
                }
            }
            if (lowestInfo.RiseRate >= shares_today.RiseRate)
            {
                return false;
            }
            return true;
        }

        private static List<Plate_Tag_Force_Session_Info> QueryMaxRiseRatePlate(int forceType,List<SharesPlateRelInfo_Session> plateList, DateTime lowestDate, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> Plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> Plate_Quotes_Today_Session)
        {
            int riseRate1 = 0;
            long plateId1 = 0;
            int riseRate2 = 0;
            long plateId2 = 0;
            foreach (var item in plateList)
            {
                Dictionary<DateTime, Plate_Quotes_Session_Info> plate_quotes_date_dic = new Dictionary<DateTime, Plate_Quotes_Session_Info>();
                if (!Plate_Quotes_Date_Session.TryGetValue(item.PlateId, out plate_quotes_date_dic))
                {
                    continue;
                }
                Plate_Quotes_Session_Info plate_quotes_date = new Plate_Quotes_Session_Info();
                if (!plate_quotes_date_dic.TryGetValue(lowestDate, out plate_quotes_date))
                {
                    continue;
                }
                Plate_Quotes_Session_Info plate_quotes_today = new Plate_Quotes_Session_Info();
                if (!Plate_Quotes_Today_Session.TryGetValue(item.PlateId, out plate_quotes_today))
                {
                    continue;
                }
                if (plate_quotes_date.RiseRate >= plate_quotes_today.RiseRate)
                {
                    continue;
                }
                int riseDiff = plate_quotes_today.RiseRate - plate_quotes_date.RiseRate;
                if (riseRate1 < riseDiff)
                {
                    riseRate1 = riseDiff;
                    riseRate2 = riseRate1;

                    plateId2 = plateId1;
                    plateId1 = item.PlateId;
                }
                else if (riseRate2 < riseDiff)
                {
                    riseRate2 = riseDiff;
                    plateId2 = item.PlateId;
                }
            }

            List<Plate_Tag_Force_Session_Info> new_force_data = new List<Plate_Tag_Force_Session_Info>();
            foreach (var item in plateList)
            {
                bool isForce1 = false;
                bool isForce2 = false;
                if (item.PlateId == plateId1)
                {
                    isForce1 = true;
                }
                else if (item.PlateId == plateId2)
                {
                    isForce2 = true;
                }
                new_force_data.Add(new Plate_Tag_Force_Session_Info 
                {
                    IsForce1= isForce1,
                    IsForce2= isForce2,
                    Type= forceType,
                    PlateId= item.PlateId,
                    Market=item.Market,
                    SharesCode=item.SharesCode
                });
            }
            return new_force_data;
        }

        private static void DoForceToDataBase(List<Plate_Tag_Force_Session_Info> new_data_list) 
        {
            DataTable table = new DataTable();
            table.Columns.Add("PlateId", typeof(long));
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            table.Columns.Add("Type", typeof(int));
            table.Columns.Add("IsForce1", typeof(bool));
            table.Columns.Add("IsForce2", typeof(bool));
            foreach (var item in new_data_list)
            {
                DataRow row = table.NewRow();
                row["PlateId"] = item.PlateId;
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                row["Type"] = item.Type;
                row["IsForce1"] = item.IsForce1;
                row["IsForce2"] = item.IsForce2;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@plateTagForceSessionInfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.PlateTagForceSessionInfo";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_Plate_Tag_Force_Session_Update @plateTagForceSessionInfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("强势板块计算入库失败", ex);
                    tran.Rollback();
                }
            }
        }

        private static void DoTrendLikeHandler(List<SharesPlateRelInfo_Session> plateRel, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> Plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> Plate_Quotes_Today_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> Shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> Shares_Quotes_Today_Session)
        {
            var plateRelDic = plateRel.GroupBy(e => new { e.Market, e.SharesCode }).ToDictionary(k => long.Parse(k.Key.SharesCode) * 10 + k.Key.Market, v => v.ToList());
            List<Plate_Tag_TrendLike_Session_Info> new_trendlike_data = _doTrendLikeHandler(plateRelDic, Plate_Quotes_Date_Session, Plate_Quotes_Today_Session, Shares_Quotes_Date_Session, Shares_Quotes_Today_Session);
            //设置缓存
            Singleton.Instance.sessionHandler.UpdateSessionPart((int)SessionHandler.Enum_Excute_Type.Plate_Tag_TrendLike_Session, new_trendlike_data);
            //入库
            try
            {
                DoTrendLikeToDataBase(new_trendlike_data);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("走势最像板块计算入库失败", ex);
            }
        }

        private static List<Plate_Tag_TrendLike_Session_Info> _doTrendLikeHandler(Dictionary<long, List<SharesPlateRelInfo_Session>> plateRelDic,Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> Plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> Plate_Quotes_Today_Session, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> Shares_Quotes_Date_Session, Dictionary<long, Shares_Quotes_Session_Info> Shares_Quotes_Today_Session) 
        {
            List<Plate_Tag_TrendLike_Session_Info> new_data_list = new List<Plate_Tag_TrendLike_Session_Info>();
            foreach (var item in plateRelDic)
            {
                //1.计算股票每一天涨跌幅
                Dictionary<DateTime, Shares_Quotes_Session_Info> shares_date_dic = new Dictionary<DateTime, Shares_Quotes_Session_Info>();
                if (!Shares_Quotes_Date_Session.TryGetValue(item.Key, out shares_date_dic))
                {
                    continue;
                }
                Shares_Quotes_Session_Info shares_today = new Shares_Quotes_Session_Info();
                if (!Shares_Quotes_Today_Session.TryGetValue(item.Key, out shares_today))
                {
                    continue;
                }
                if (shares_date_dic.Count() == 0)
                {
                    continue;
                }
                var tempData=_calTrendLike(shares_date_dic, shares_today, item.Value, Plate_Quotes_Date_Session, Plate_Quotes_Today_Session);
                new_data_list.AddRange(tempData);
            }

            return new_data_list;
        }

        private static List<Plate_Tag_TrendLike_Session_Info> _calTrendLike(Dictionary<DateTime, Shares_Quotes_Session_Info> shares_date_dic, Shares_Quotes_Session_Info shares_today, List<SharesPlateRelInfo_Session> plateRelList, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> Plate_Quotes_Date_Session, Dictionary<long, Plate_Quotes_Session_Info> Plate_Quotes_Today_Session)
        {
            long plateId = 0;
            int minScore = int.MaxValue;
            foreach (var plateRel in plateRelList)
            {
                Dictionary<DateTime, Plate_Quotes_Session_Info> plate_date_dic = new Dictionary<DateTime, Plate_Quotes_Session_Info>();
                if (!Plate_Quotes_Date_Session.TryGetValue(plateRel.PlateId, out plate_date_dic))
                {
                    continue;
                }
                Plate_Quotes_Session_Info plate_today = new Plate_Quotes_Session_Info();
                if (!Plate_Quotes_Today_Session.TryGetValue(plateRel.PlateId, out plate_today))
                {
                    continue;
                }
                int totalScore = 0;
                foreach (DateTime date in shares_date_dic.Keys)
                {
                    int plateRiseRate = 0;
                    if (plate_date_dic.ContainsKey(date))
                    {
                        plateRiseRate = plate_date_dic[date].RiseRate;
                    }
                    int score=Math.Abs(shares_date_dic[date].RiseRate - plateRiseRate);
                    totalScore += score;
                }
                if (minScore > totalScore)
                {
                    minScore = totalScore;
                    plateId = plateRel.PlateId;
                }
            }
            List<Plate_Tag_TrendLike_Session_Info> new_data_list = new List<Plate_Tag_TrendLike_Session_Info>();
            foreach (var item in plateRelList)
            {
                bool isTrendLike = false;
                if (item.PlateId == plateId)
                {
                    isTrendLike = true;
                }
                new_data_list.Add(new Plate_Tag_TrendLike_Session_Info 
                {
                    SharesCode=item.SharesCode,
                    Market=item.Market,
                    PlateId=item.PlateId,
                    IsTrendLike=isTrendLike
                });
            }
            return new_data_list;
        }

        private static void DoTrendLikeToDataBase(List<Plate_Tag_TrendLike_Session_Info> new_data_list)
        {
            DataTable table = new DataTable();
            table.Columns.Add("PlateId", typeof(long));
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            table.Columns.Add("IsTrendLike", typeof(bool));
            foreach (var item in new_data_list)
            {
                DataRow row = table.NewRow();
                row["PlateId"] = item.PlateId;
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                row["IsTrendLike"] = item.IsTrendLike;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@plateTagTrendLikeSessionInfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.PlateTagTrendLikeSessionInfo";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_Plate_Tag_TrendLike_Session_Update @plateTagTrendLikeSessionInfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("走势最像板块计算入库失败", ex);
                    tran.Rollback();
                }
            }
        }
    }
}
