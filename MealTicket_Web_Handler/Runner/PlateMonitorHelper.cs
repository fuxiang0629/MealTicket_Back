using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Runner
{
    public struct PlateIndexInfo
    {
        /// <summary>
        /// 百分比
        /// </summary>
        public int IndexRate { get; set; }

        /// <summary>
        /// 得分
        /// </summary>
        public int IndexScore { get; set; }

        /// <summary>
        /// 总分系数
        /// </summary>
        public int Coefficient { get; set; }

        /// <summary>
        /// 股票列表
        /// </summary>
        public List<long> KeyList { get; set; }
    }

    public struct PlateVolumeInfo 
    {
        /// <summary>
        /// 当前量比
        /// </summary>
        public int CurrRate { get; set; }

        /// <summary>
        /// 预期量比
        /// </summary>
        public int ExceptRate { get; set; }
    }

    public struct PlateMonitor
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 动能指数
        /// </summary>
        public int EnergyIndex 
        {
            get 
            {
                int tempEnergyIndex = 0;
                var EnergyIndexTypeList = Singleton.Instance.EnergyIndexTypeList;
                if (EnergyIndexTypeList.Contains(1)) 
                {
                    tempEnergyIndex += LeaderIndex.IndexScore;
                }
                if (EnergyIndexTypeList.Contains(2))
                {
                    tempEnergyIndex += PlateLinkageIndex.IndexScore;
                }
                if (EnergyIndexTypeList.Contains(3))
                {
                    tempEnergyIndex += SharesLinkageIndex.IndexScore;
                }
                if (EnergyIndexTypeList.Contains(4))
                {
                    tempEnergyIndex += NewHighIndex.IndexScore;
                }
                return tempEnergyIndex;
            }
        }

        public int EnergyIndexRate
        {
            get 
            {
                int Coefficient = 0;
                var EnergyIndexTypeList = Singleton.Instance.EnergyIndexTypeList;
                if (EnergyIndexTypeList.Contains(1))
                {
                    Coefficient += LeaderIndex.Coefficient;
                }
                if (EnergyIndexTypeList.Contains(2))
                {
                    Coefficient += PlateLinkageIndex.Coefficient;
                }
                if (EnergyIndexTypeList.Contains(3))
                {
                    Coefficient += SharesLinkageIndex.Coefficient;
                }
                if (EnergyIndexTypeList.Contains(4))
                {
                    Coefficient += NewHighIndex.Coefficient;
                }
                return Coefficient == 0 ? 0 : (int)Math.Round(EnergyIndex * 1.0 / Coefficient * 10000, 0);
            }
        }

        /// <summary>
        /// 龙头指数
        /// </summary>
        public PlateIndexInfo LeaderIndex { get; set; }

        /// <summary>
        /// 板块联动指数
        /// </summary>
        public PlateIndexInfo PlateLinkageIndex { get; set; }

        /// <summary>
        /// 股票联动指数
        /// </summary>
        public PlateIndexInfo SharesLinkageIndex { get; set; }

        /// <summary>
        /// 新高指数
        /// </summary>
        public PlateIndexInfo NewHighIndex { get; set; }

        /// <summary>
        /// 量比
        /// </summary>
        public PlateVolumeInfo PlateVolume { get; set; }

        public int CalCount { get; set; }
    }

    public struct SessionContainer
    {
        /// <summary>
        /// 股票龙头缓存
        /// </summary>
        public Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>> LeaderSession { get; set; }

        /// <summary>
        /// 股票日内龙缓存
        /// </summary>
        public Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>> DayLeaderSession { get; set; }

        /// <summary>
        /// 股票中军缓存
        /// </summary>
        public Dictionary<long, Dictionary<long, Shares_Tag_MainArmy_Session_Info>> MainArmySession { get; set; }

        /// <summary>
        /// 板块指数系数配置缓存
        /// </summary>
        public Dictionary<int, Setting_Plate_Index_Session_Info> SettingPlateIndexSession { get; set; }

        /// <summary>
        /// 板块联动缓存
        /// </summary>
        public Dictionary<long, Setting_Plate_Linkage_Session_Info_Group> SettingPlateLinkageSession { get; set; }

        /// <summary>
        /// 股票联动缓存
        /// </summary>
        public Dictionary<long, Setting_Plate_Shares_Linkage_Session_Info_Group> SettingPlateSharesLinkageSession { get; set; }

        /// <summary>
        /// 股票最后行情缓存
        /// </summary>
        public Dictionary<long, Shares_Quotes_Session_Info_Last> SharesQuotesLastSession { get; set; }

        /// <summary>
        /// 板块最后行情数据
        /// </summary>
        public Dictionary<long, Plate_Quotes_Session_Info> PlateQuotesLastSession { get; set; }

        /// <summary>
        /// 板块内股票缓存
        /// </summary>
        public Dictionary<long, List<Plate_Shares_Rel_Session_Info>> Plate_Shares_Rel_Session { get; set; }

        /// <summary>
        /// 板块基础数据
        /// </summary>
        public Dictionary<long, Plate_Base_Session_Info> PlateBasePlate { get; set; }
    }

    public struct Plate_CalDays_Info
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 时间类型
        /// </summary>
        public int DayType { get; set; }
    }

    public class PlateMonitorHelper
    {
        Dictionary<int, Dictionary<long, PlateMonitor>> PlateMonitorSessionDic;

        ReaderWriterLock _sessionReadWriteLock = new ReaderWriterLock();

        private void SetSession(int dayType,List<PlateMonitor> plateMonitors)
        {
            _sessionReadWriteLock.AcquireWriterLock(Timeout.Infinite);
            if (!PlateMonitorSessionDic.ContainsKey(dayType))
            {
                PlateMonitorSessionDic.Add(dayType, new Dictionary<long, PlateMonitor>());
            }
            foreach (var item in plateMonitors)
            {
                if (!PlateMonitorSessionDic[dayType].ContainsKey(item.PlateId))
                {
                    PlateMonitorSessionDic[dayType].Add(item.PlateId,new PlateMonitor());
                }
                PlateMonitorSessionDic[dayType][item.PlateId] = item;
            }
            _sessionReadWriteLock.ReleaseWriterLock();
        }

        public Dictionary<long, PlateMonitor> GetSession(int dayType)
        {
            Dictionary<long, PlateMonitor> result;
            _sessionReadWriteLock.AcquireReaderLock(Timeout.Infinite);
            if (!PlateMonitorSessionDic.ContainsKey(dayType))
            {
                result = new Dictionary<long, PlateMonitor>();
            }
            else
            {
                result = new Dictionary<long, PlateMonitor>(PlateMonitorSessionDic[dayType]);
            }
            _sessionReadWriteLock.ReleaseReaderLock();
            return result;
        }

        public Dictionary<int, Dictionary<long, PlateMonitor>> GetSession()
        {
            Dictionary<int, Dictionary<long, PlateMonitor>> result=new Dictionary<int, Dictionary<long, PlateMonitor>>();
            _sessionReadWriteLock.AcquireReaderLock(Timeout.Infinite);
            foreach (var item in PlateMonitorSessionDic)
            {
                result.Add(item.Key, new Dictionary<long, PlateMonitor>(item.Value));
            }
            _sessionReadWriteLock.ReleaseReaderLock();
            return result;
        }

        Dictionary<long, Dictionary<DateTime, PlateMonitor>> PlateMonitorMinSessionDic;

        private void SetMinSession() 
        {
            DateTime timeNow = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:00"));
            _sessionReadWriteLock.AcquireWriterLock(Timeout.Infinite);
            Dictionary<long, PlateMonitor> plateListDic = new Dictionary<long, PlateMonitor>();
            foreach (var dayType in PlateMonitorSessionDic)
            {
                foreach (var plate in dayType.Value)
                {
                    if (!PlateMonitorMinSessionDic.ContainsKey(plate.Key))
                    {
                        PlateMonitorMinSessionDic.Add(plate.Key, new Dictionary<DateTime, PlateMonitor>());
                    }
                    if (!PlateMonitorMinSessionDic[plate.Key].ContainsKey(timeNow))
                    {
                        PlateMonitorMinSessionDic[plate.Key].Add(timeNow, new PlateMonitor());
                    }
                    if (!plateListDic.ContainsKey(plate.Key))
                    {
                        plateListDic.Add(plate.Key, new PlateMonitor 
                        {
                            PlateId= plate.Key,
                            SharesLinkageIndex =new PlateIndexInfo 
                            {
                                IndexRate=0,
                                IndexScore=0,
                                Coefficient=0
                            },
                            LeaderIndex = new PlateIndexInfo
                            {
                                IndexRate = 0,
                                IndexScore = 0,
                                Coefficient = 0
                            },
                            NewHighIndex = new PlateIndexInfo
                            {
                                IndexRate = 0,
                                IndexScore = 0,
                                Coefficient = 0
                            },
                            PlateLinkageIndex = new PlateIndexInfo
                            {
                                IndexRate = 0,
                                IndexScore = 0,
                                Coefficient = 0
                            },
                            PlateVolume = new PlateVolumeInfo
                            {
                                CurrRate = 0,
                                ExceptRate = 0
                            },
                            CalCount=0
                        });
                    }
                    var temp=plateListDic[plate.Key];
                    var tempLeaderIndex = temp.LeaderIndex;
                    tempLeaderIndex.IndexRate = tempLeaderIndex.IndexRate + plate.Value.LeaderIndex.IndexRate;
                    tempLeaderIndex.IndexScore = tempLeaderIndex.IndexScore + plate.Value.LeaderIndex.IndexScore;
                    tempLeaderIndex.Coefficient = tempLeaderIndex.Coefficient + plate.Value.LeaderIndex.Coefficient;
                    temp.LeaderIndex = tempLeaderIndex;
                    var tempNewHighIndex = temp.NewHighIndex;
                    tempNewHighIndex.IndexRate = tempNewHighIndex.IndexRate + plate.Value.NewHighIndex.IndexRate;
                    tempNewHighIndex.IndexScore = tempNewHighIndex.IndexScore + plate.Value.NewHighIndex.IndexScore;
                    tempNewHighIndex.Coefficient = tempNewHighIndex.Coefficient + plate.Value.NewHighIndex.Coefficient;
                    temp.NewHighIndex = tempNewHighIndex;
                    var tempPlateLinkageIndex = temp.PlateLinkageIndex;
                    tempPlateLinkageIndex.IndexRate = tempPlateLinkageIndex.IndexRate + plate.Value.PlateLinkageIndex.IndexRate;
                    tempPlateLinkageIndex.IndexScore = tempPlateLinkageIndex.IndexScore + plate.Value.PlateLinkageIndex.IndexScore;
                    tempPlateLinkageIndex.Coefficient = tempPlateLinkageIndex.Coefficient + plate.Value.PlateLinkageIndex.Coefficient;
                    temp.PlateLinkageIndex = tempPlateLinkageIndex;
                    var tempSharesLinkageIndex = temp.SharesLinkageIndex;
                    tempSharesLinkageIndex.IndexRate = tempSharesLinkageIndex.IndexRate + plate.Value.SharesLinkageIndex.IndexRate;
                    tempSharesLinkageIndex.IndexScore = tempSharesLinkageIndex.IndexScore + plate.Value.SharesLinkageIndex.IndexScore;
                    tempSharesLinkageIndex.Coefficient = tempSharesLinkageIndex.Coefficient + plate.Value.SharesLinkageIndex.Coefficient;
                    temp.SharesLinkageIndex = tempSharesLinkageIndex;
                    temp.CalCount = temp.CalCount + 1;
                    plateListDic[plate.Key] = temp;
                }
            }
            Dictionary<long, Dictionary<DateTime, PlateMonitor>> tempPlateMonitorMinSessionDic = new Dictionary<long, Dictionary<DateTime, PlateMonitor>>();
            foreach (var item in PlateMonitorMinSessionDic)
            {
                Dictionary<DateTime, PlateMonitor> tempPm = new Dictionary<DateTime, PlateMonitor>();
                if (plateListDic.ContainsKey(item.Key) && plateListDic[item.Key].CalCount > 0)
                {
                    int calCount = plateListDic[item.Key].CalCount;
                    item.Value[timeNow] = new PlateMonitor
                    {
                        PlateId = item.Key,
                        LeaderIndex = new PlateIndexInfo
                        {
                            IndexRate = plateListDic[item.Key].LeaderIndex.IndexRate / calCount,
                            IndexScore = plateListDic[item.Key].LeaderIndex.IndexScore / calCount,
                            Coefficient = plateListDic[item.Key].LeaderIndex.Coefficient / calCount,
                        },
                        SharesLinkageIndex = new PlateIndexInfo
                        {
                            IndexRate = plateListDic[item.Key].SharesLinkageIndex.IndexRate / calCount,
                            IndexScore = plateListDic[item.Key].SharesLinkageIndex.IndexScore / calCount,
                            Coefficient = plateListDic[item.Key].SharesLinkageIndex.Coefficient / calCount,
                        },
                        PlateLinkageIndex = new PlateIndexInfo
                        {
                            IndexRate = plateListDic[item.Key].PlateLinkageIndex.IndexRate / calCount,
                            IndexScore = plateListDic[item.Key].PlateLinkageIndex.IndexScore / calCount,
                            Coefficient = plateListDic[item.Key].PlateLinkageIndex.Coefficient / calCount,
                        },
                        NewHighIndex = new PlateIndexInfo
                        {
                            IndexRate = plateListDic[item.Key].NewHighIndex.IndexRate / calCount,
                            IndexScore = plateListDic[item.Key].NewHighIndex.IndexScore / calCount,
                            Coefficient = plateListDic[item.Key].NewHighIndex.Coefficient / calCount,
                        },
                    };

                    SortedDictionary<DateTime, PlateMonitor> temp = new SortedDictionary<DateTime, PlateMonitor>(item.Value);
                    int count = item.Value.Count();
                    count = count < 16 ? 0 : (count - 16);
                    temp = new SortedDictionary<DateTime, PlateMonitor>(temp.Skip(count).ToDictionary(k => k.Key, v => v.Value));
                    tempPm = temp.Reverse().ToDictionary(k=>k.Key,v=>v.Value);
                }

                tempPlateMonitorMinSessionDic.Add(item.Key, tempPm);
            }
            PlateMonitorMinSessionDic = tempPlateMonitorMinSessionDic;
            _sessionReadWriteLock.ReleaseWriterLock();
        }

        public Dictionary<DateTime, PlateMonitor> GetMinSession(long key)
        {
            Dictionary<DateTime, PlateMonitor> result;
            _sessionReadWriteLock.AcquireReaderLock(Timeout.Infinite);
            if (!PlateMonitorMinSessionDic.ContainsKey(key))
            {
                result = new Dictionary<DateTime, PlateMonitor>();
            }
            else
            {
                result = PlateMonitorMinSessionDic[key];
            }
            _sessionReadWriteLock.ReleaseReaderLock();
            return result;
        }

        SessionContainer sessionContainer;

        Timer calTimer = null;

        //1.3天 2.5天 3.10天 4.15天
        int[] DaysType;

        //计算间隔（默认3秒）
        int CalInterval;

        bool IsFirst;

        public PlateMonitorHelper() 
        {
            PlateMonitorSessionDic = new Dictionary<int, Dictionary<long, PlateMonitor>>();
            PlateMonitorMinSessionDic = new Dictionary<long, Dictionary<DateTime, PlateMonitor>>();
            DaysType = new int[] { 1, 2, 3, 4 };
            CalInterval = 3000;
            IsFirst = true;
        }

        public void DoCalTask() 
        {
            StartTimer();
        }

        private void UpdateSessionContainer()
        {
            ToWriteDebugLog("11:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            var DayLeaderSession = Singleton.Instance.sessionHandler.GetShares_Tag_DayLeader_Session();
            ToWriteDebugLog("12:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            var LeaderSession = Singleton.Instance.sessionHandler.GetShares_Tag_Leader_Session();
            ToWriteDebugLog("13:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            var MainArmySession = Singleton.Instance.sessionHandler.GetShares_Tag_MainArmy_Session();
            ToWriteDebugLog("14:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            var SettingPlateIndexSession = Singleton.Instance.sessionHandler.GetSetting_Plate_Index_Session();
            ToWriteDebugLog("15:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            var SettingPlateLinkageSession = Singleton.Instance.sessionHandler.GetSetting_Plate_Linkage_Session();
            ToWriteDebugLog("16:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            var SharesQuotesLastSession = Singleton.Instance.sessionHandler.GetShares_Quotes_Last_Session();
            ToWriteDebugLog("17:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            var PlateQuotesLastSession = Singleton.Instance.sessionHandler.GetPlate_Quotes_Last_Session();
            ToWriteDebugLog("18:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            var Plate_Shares_Rel_Session = Singleton.Instance.sessionHandler.GetPlate_Shares_Rel_Session();
            ToWriteDebugLog("19:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            var SettingPlateSharesLinkageSession = Singleton.Instance.sessionHandler.GetSetting_Plate_Shares_Linkage_Session();
            ToWriteDebugLog("110:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            var PlateBasePlate = Singleton.Instance.sessionHandler.GetPlate_Base_Session();
            sessionContainer = new SessionContainer
            {
                DayLeaderSession = DayLeaderSession,
                LeaderSession = LeaderSession,
                MainArmySession = MainArmySession,
                SettingPlateIndexSession = SettingPlateIndexSession,
                SettingPlateLinkageSession = SettingPlateLinkageSession,
                SharesQuotesLastSession = SharesQuotesLastSession,
                PlateQuotesLastSession = PlateQuotesLastSession,
                Plate_Shares_Rel_Session= Plate_Shares_Rel_Session,
                SettingPlateSharesLinkageSession= SettingPlateSharesLinkageSession,
                PlateBasePlate= PlateBasePlate
            };
        }

        private void ToWriteDebugLog(string message,Exception ex) 
        {
            return;
            Logger.WriteFileLog(message, ex);
        }

        public void _doCalTask(object obj)
        {
            try
            {
                ToWriteDebugLog("1:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                var time_limit = Singleton.Instance.sessionHandler.GetShares_Limit_Time_Session();

                IsFirst = false;
                UpdateSessionContainer();//获取公用缓存

                ToWriteDebugLog("2:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);

                int coefficient = 0;
                var newHighResult = CheckNewHeightIndex(ref coefficient);

                ToWriteDebugLog("3:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                WaitHandle[] taskArr = new WaitHandle[DaysType.Count()];
                for (int idx = 0; idx < DaysType.Count(); idx++)
                {
                    int dayType = DaysType[idx];
                    taskArr[idx] = TaskThread.CreateTask((e) =>
                    {
                        List<PlateMonitor> plateMonitors = new List<PlateMonitor>();
                        if (dayType == 4)
                        {
                            ToWriteDebugLog("31:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                        }
                        var plate_real_shares = Singleton.Instance.sessionHandler.GetPlate_Real_Shares_Session(dayType);
                        if (dayType == 4)
                        {
                            ToWriteDebugLog("33:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                        }
                        foreach (var item in sessionContainer.PlateBasePlate)
                        {
                            if (!plate_real_shares.ContainsKey(item.Key))
                            {
                                continue;
                            }
                            PlateIndexInfo LeaderIndex = new PlateIndexInfo();
                            CalLeaderIndex(dayType, item.Key, ref LeaderIndex);
                            PlateIndexInfo PlateLinkageIndex = new PlateIndexInfo();
                            CalPlateLinkageIndex(item.Key, dayType, ref PlateLinkageIndex, plate_real_shares);
                            PlateIndexInfo SharesLinkageIndex = new PlateIndexInfo();
                            CalSharesLinkageIndex(item.Key, plate_real_shares, ref SharesLinkageIndex);
                            PlateIndexInfo NewHighIndex = new PlateIndexInfo();
                            CalNewHeightIndex(item.Key, coefficient, newHighResult, plate_real_shares, ref NewHighIndex);
                            PlateVolumeInfo PlateVolume = new PlateVolumeInfo();
                            CalVolumeRate(item.Key, ref PlateVolume);
                            plateMonitors.Add(new PlateMonitor
                            {
                                PlateId = item.Key,
                                LeaderIndex = LeaderIndex,
                                PlateLinkageIndex = PlateLinkageIndex,
                                SharesLinkageIndex = SharesLinkageIndex,
                                NewHighIndex = NewHighIndex,
                                PlateVolume = PlateVolume
                            });
                        }
                        if (dayType == 4)
                        {
                            ToWriteDebugLog("34:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                        }
                        SetSession(dayType, plateMonitors);
                        if (dayType == 4)
                        {
                            ToWriteDebugLog("35:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                        }
                        if (DbHelper.CheckTradeTime7(null, time_limit))
                        {
                            InsertIntoDataBase(dayType, plateMonitors);
                        }
                        if (dayType == 4)
                        {
                            ToWriteDebugLog("36:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                        }
                    }, null);
                }
                TaskThread.WaitAll(taskArr, Timeout.Infinite);
                TaskThread.CloseAllTasks(taskArr);
                ToWriteDebugLog("4:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                if (DbHelper.CheckTradeTime7(null, time_limit))
                {
                    SetMinSession();
                }
                ToWriteDebugLog("5:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                ChangeTimer();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("计算板块动能失败", ex);
            }
        }

        /// <summary>
        /// 添加到数据库
        /// </summary>
        private void InsertIntoDataBase(int dayType, List<PlateMonitor> dataList) 
        {
            DataTable table = new DataTable();
            table.Columns.Add("PlateId", typeof(long));
            table.Columns.Add("DayType", typeof(int));
            table.Columns.Add("Time", typeof(DateTime));
            table.Columns.Add("EnergyIndex", typeof(int));
            table.Columns.Add("LeaderIndexRate", typeof(int));
            table.Columns.Add("LeaderIndexScore", typeof(int));
            table.Columns.Add("LeaderKeyList", typeof(string));
            table.Columns.Add("PlateLinkageIndexRate", typeof(int));
            table.Columns.Add("PlateLinkageIndexScore", typeof(int));
            table.Columns.Add("PlateLinkageKeyList", typeof(string));
            table.Columns.Add("SharesLinkageIndexRate", typeof(int));
            table.Columns.Add("SharesLinkageIndexScore", typeof(int));
            table.Columns.Add("SharesLinkageKeyList", typeof(string));
            table.Columns.Add("NewHighIndexRate", typeof(int));
            table.Columns.Add("NewHighIndexScore", typeof(int));
            table.Columns.Add("NewHighKeyList", typeof(string));
            table.Columns.Add("PlateVolumeCurrRate", typeof(int));
            table.Columns.Add("PlateVolumeExceptRate", typeof(int));
            table.Columns.Add("LastModified", typeof(DateTime));

            foreach (var item in dataList)
            {
                DataRow row = table.NewRow();
                row["PlateId"] = item.PlateId;
                row["DayType"] = dayType;
                row["Time"] = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:00"));
                row["EnergyIndex"] = item.EnergyIndex;
                row["LeaderIndexRate"] = item.LeaderIndex.IndexRate;
                row["LeaderIndexScore"] = item.LeaderIndex.IndexScore;
                row["LeaderKeyList"] = string.Join(",", item.LeaderIndex.KeyList.ToArray());
                row["PlateLinkageIndexRate"] = item.PlateLinkageIndex.IndexRate;
                row["PlateLinkageIndexScore"] = item.PlateLinkageIndex.IndexScore;
                row["PlateLinkageKeyList"] = string.Join(",",item.PlateLinkageIndex.KeyList.ToArray());
                row["SharesLinkageIndexRate"] = item.SharesLinkageIndex.IndexRate;
                row["SharesLinkageIndexScore"] = item.SharesLinkageIndex.IndexScore;
                row["SharesLinkageKeyList"] =string.Join(",", item.SharesLinkageIndex.KeyList.ToArray());
                row["NewHighIndexRate"] = item.NewHighIndex.IndexRate;
                row["NewHighIndexScore"] = item.NewHighIndex.IndexScore;
                row["NewHighKeyList"] = string.Join(",", item.NewHighIndex.KeyList.ToArray());
                row["PlateVolumeCurrRate"] = item.PlateVolume.CurrRate;
                row["PlateVolumeExceptRate"] = item.PlateVolume.ExceptRate;
                row["LastModified"] = DateTime.Now;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@plateEnergyIndexInfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.PlateEnergyIndexInfo";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_PlateEnergyIndexData_Update @plateEnergyIndexInfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("板块动能入库失败", ex);
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// 计算龙头指数
        /// </summary>
        private void CalLeaderIndex(int dayType, long plateId, ref PlateIndexInfo LeaderIndex)
        {
            int Coefficient = 0;
            if (sessionContainer.SettingPlateIndexSession.ContainsKey(1))
            {
                Coefficient = sessionContainer.SettingPlateIndexSession[1].Coefficient;
            }
            LeaderIndex = new PlateIndexInfo
            {
                IndexRate = 0,
                IndexScore = 0,
                Coefficient = Coefficient,
                KeyList = new List<long>()
            };
            long key = plateId * 100 + dayType;
            List<long> sharesKeyList = new List<long>();
            if (sessionContainer.LeaderSession.ContainsKey(key))
            {
                sharesKeyList.AddRange(sessionContainer.LeaderSession[key].Keys.ToList());
            }
            if (sessionContainer.DayLeaderSession.ContainsKey(key))
            {
                sharesKeyList.AddRange(sessionContainer.DayLeaderSession[key].Keys.ToList());
            }
            if (sessionContainer.MainArmySession.ContainsKey(key))
            {
                sharesKeyList.AddRange(sessionContainer.MainArmySession[key].Keys.ToList());
            }
            sharesKeyList = sharesKeyList.Distinct().ToList();

            int rate = 0;
            int limitUpRate = 0;
            int calCount = 0;
            foreach (long sharesKey in sharesKeyList)
            {
                if (!sessionContainer.SharesQuotesLastSession.ContainsKey(sharesKey))
                {
                    continue;
                }
                var temp = sessionContainer.SharesQuotesLastSession[sharesKey];
                if (temp.shares_quotes_info.LimitUpPrice == 0 || temp.shares_quotes_info.YestodayClosedPrice == 0)
                {
                    continue;
                }
                calCount++;
                rate = rate + temp.shares_quotes_info.RiseRate;
                limitUpRate = limitUpRate + (int)Math.Round((temp.shares_quotes_info.LimitUpPrice - temp.shares_quotes_info.YestodayClosedPrice) * 1.0 / temp.shares_quotes_info.YestodayClosedPrice * 10000, 0);
                LeaderIndex.KeyList.Add(sharesKey);
            }
            if (limitUpRate == 0 || calCount == 0)
            {
                return;
            }
            LeaderIndex.IndexRate = (int)Math.Round(rate * 1.0 / calCount, 0);
            LeaderIndex.IndexScore = Coefficient == 0 ? 0 : (int)Math.Round(rate * 1.0 / limitUpRate * Coefficient, 0);
        }

        /// <summary>
        /// 计算板块联动指数
        /// </summary>
        private void CalPlateLinkageIndex(long plateId, int dayType, ref PlateIndexInfo PlateLinkageIndex, Dictionary<long, List<long>> plate_real_shares)
        {
            int Coefficient = 0;
            if (sessionContainer.SettingPlateIndexSession.ContainsKey(2))
            {
                Coefficient = sessionContainer.SettingPlateIndexSession[2].Coefficient;
            }
            PlateLinkageIndex = new PlateIndexInfo
            {
                IndexRate = 0,
                IndexScore = 0,
                Coefficient = Coefficient,
                KeyList = new List<long> { plateId }
            };
            //1.查询联动板块
            if (!sessionContainer.SettingPlateLinkageSession.ContainsKey(plateId))
            {
                return;
            }
            var PlateLinkage = sessionContainer.SettingPlateLinkageSession[plateId];
            int rate = 0;
            int limitUpRate = 0;
            int calCount = 0;
            foreach (var item in PlateLinkage.SessionList)
            {
                if (!sessionContainer.PlateQuotesLastSession.ContainsKey(item.LinkagePlateId))
                {
                    continue;
                }
                //查询板块内真实股票
                if (!sessionContainer.Plate_Shares_Rel_Session.ContainsKey(item.LinkagePlateId))
                {
                    continue;
                }
                var sharesList = sessionContainer.Plate_Shares_Rel_Session[item.LinkagePlateId];
                if (PlateLinkage.IsReal)
                {
                    if (!plate_real_shares.ContainsKey(item.LinkagePlateId))
                    {
                        continue;
                    }
                    var tempSharesList = plate_real_shares[item.LinkagePlateId];
                    sharesList = (from x in sharesList
                                  where tempSharesList.Contains(long.Parse(x.SharesCode) * 10 + x.Market)
                                  select x).ToList();
                }

                int sharesCount = 0;
                int maxRate = 0;
                int sharesRate = 0;
                foreach (var share in sharesList)
                {
                    long key = long.Parse(share.SharesCode) * 10 + share.Market;
                    if (!sessionContainer.SharesQuotesLastSession.ContainsKey(key))
                    {
                        continue;
                    }
                    var tempInfo = sessionContainer.SharesQuotesLastSession[key].shares_quotes_info;
                    if (tempInfo.LimitUpPrice == 0 || tempInfo.YestodayClosedPrice == 0)
                    {
                        continue;
                    }
                    int tempMaxRate = (int)Math.Round((tempInfo.LimitUpPrice - tempInfo.YestodayClosedPrice) * 1.0 / tempInfo.YestodayClosedPrice * 10000, 0);
                    sharesCount++;
                    maxRate += tempMaxRate;
                    sharesRate += tempInfo.RiseRate;
                }
                if (sharesCount == 0)
                {
                    continue;
                }
                rate += sharesRate / sharesCount;
                limitUpRate += maxRate / sharesCount;
                calCount++;
                PlateLinkageIndex.KeyList.Add(item.LinkagePlateId);
            }
            PlateLinkageIndex.KeyList = PlateLinkageIndex.KeyList.Distinct().ToList();
            if (limitUpRate == 0 || calCount == 0)
            {
                return;
            }
            PlateLinkageIndex.IndexRate = (int)Math.Round(rate * 1.0 / calCount, 0);
            PlateLinkageIndex.IndexScore = Coefficient == 0 ? 0 : (int)Math.Round(rate * 1.0 / limitUpRate * Coefficient, 0);
        }

        /// <summary>
        /// 计算股票联动指数
        /// </summary>
        private void CalSharesLinkageIndex(long plateId, Dictionary<long, List<long>> Plate_Real_Shares_Session, ref PlateIndexInfo SharesLinkageIndex)
        {
            int Coefficient = 0;
            if (sessionContainer.SettingPlateIndexSession.ContainsKey(3))
            {
                Coefficient = sessionContainer.SettingPlateIndexSession[3].Coefficient;
            }
            SharesLinkageIndex = new PlateIndexInfo
            {
                IndexRate = 0,
                IndexScore = 0,
                Coefficient = Coefficient,
                KeyList = new List<long>()
            };
            //1.查询联动股票
            if (!sessionContainer.SettingPlateSharesLinkageSession.ContainsKey(plateId))
            {
                return;
            }
            var SharesLinkageSession = sessionContainer.SettingPlateSharesLinkageSession[plateId];
            List<Setting_Plate_Shares_Linkage_Session_Info> sharesList = new List<Setting_Plate_Shares_Linkage_Session_Info>(SharesLinkageSession.SessionList);
            var linkage_sharesKeyList = sharesList.Where(e => !string.IsNullOrEmpty(e.SharesCode)).Select(e => long.Parse(e.SharesCode) * 10 + e.Market).ToList();

            if (SharesLinkageSession.IsReal)
            {
                if (!Plate_Real_Shares_Session.ContainsKey(plateId))
                {
                    return;
                }
                var sharesKeyList = Plate_Real_Shares_Session[plateId];
                linkage_sharesKeyList = linkage_sharesKeyList.Intersect(sharesKeyList).ToList();
            }
            if (SharesLinkageSession.IsDefault)
            {
                if (Plate_Real_Shares_Session.ContainsKey(plateId))
                {
                    var sharesKeyList = Plate_Real_Shares_Session[plateId];
                    linkage_sharesKeyList = linkage_sharesKeyList.Union(sharesKeyList).ToList();
                }
            }


            int rate = 0;
            int limitUpRate = 0;
            int calCount = 0;
            foreach (var sharesKey in linkage_sharesKeyList)
            {
                if (!sessionContainer.SharesQuotesLastSession.ContainsKey(sharesKey))
                {
                    continue;
                }
                var temp = sessionContainer.SharesQuotesLastSession[sharesKey];
                if (temp.shares_quotes_info.LimitUpPrice == 0 || temp.shares_quotes_info.YestodayClosedPrice == 0)
                {
                    continue;
                }
                rate = rate + temp.shares_quotes_info.RiseRate;
                limitUpRate = limitUpRate + (int)Math.Round((temp.shares_quotes_info.LimitUpPrice - temp.shares_quotes_info.YestodayClosedPrice) * 1.0 / temp.shares_quotes_info.YestodayClosedPrice * 10000, 0);
                calCount++;
                SharesLinkageIndex.KeyList.Add(sharesKey);
            }
            if (limitUpRate == 0 || calCount == 0)
            {
                return;
            }
            SharesLinkageIndex.IndexRate = (int)Math.Round(rate * 1.0 / calCount, 0);
            SharesLinkageIndex.IndexScore = Coefficient == 0 ? 0 : (int)Math.Round(rate * 1.0 / limitUpRate * Coefficient, 0);
        }

        /// <summary>
        /// 计算新高指数(真实板块新高)
        /// </summary>
        private void CalNewHeightIndex(long plateId,int coefficient, Dictionary<long, bool> newHighResult, Dictionary<long, List<long>> Plate_Real_Shares_Session, ref PlateIndexInfo NewHighIndex)
        {
            NewHighIndex = new PlateIndexInfo
            {
                IndexRate = 0,
                IndexScore = 0,
                Coefficient= coefficient,
                KeyList = new List<long>()
            };
            long key = plateId;
            List<long> sharesKeyList = new List<long>();
            if (Plate_Real_Shares_Session.ContainsKey(key))
            {
                sharesKeyList.AddRange(Plate_Real_Shares_Session[key]);
            }
            sharesKeyList = sharesKeyList.Distinct().ToList();

            int totalCount = sharesKeyList.Count();
            if (totalCount == 0)
            {
                return;
            }
            int newHighCount = 0;
            foreach (var share in sharesKeyList)
            {
                if (!newHighResult.ContainsKey(share))
                {
                    continue;
                }
                if (!newHighResult[share])
                {
                    continue;
                }
                newHighCount++;
                NewHighIndex.KeyList.Add(share);
            }
            NewHighIndex.IndexRate = newHighCount;
            NewHighIndex.IndexScore = coefficient == 0 ? 0 : (int)Math.Round(newHighCount * 1.0 / totalCount * coefficient, 0);
            return;
        }

        private Dictionary<long,bool> CheckNewHeightIndex(ref int coefficient) 
        {
            Dictionary<long, bool> result = new Dictionary<long, bool>();
            if (!sessionContainer.SettingPlateIndexSession.ContainsKey(4))
            {
                coefficient = 0;
            }
            else
            {
                ToWriteDebugLog("21:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                coefficient = sessionContainer.SettingPlateIndexSession[4].Coefficient;
                int calDays = sessionContainer.SettingPlateIndexSession[4].CalDays;
                int calPriceType = sessionContainer.SettingPlateIndexSession[4].CalPriceType;
                ToWriteDebugLog("22:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                var shares_quotes_date = Singleton.Instance.sessionHandler.GetShares_Quotes_Date_Sort_Session(calDays);
                ToWriteDebugLog("23:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);

                var shares_quote_last = sessionContainer.SharesQuotesLastSession;
                ToWriteDebugLog("24:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                foreach (var item in shares_quote_last)
                {
                    if (!shares_quotes_date.SessionDic.ContainsKey(item.Key))
                    {
                        continue;
                    }

                    int tempCalDays = calDays;
                    if (item.Value.shares_quotes_info.Date >= DateTime.Now.Date)
                    {
                        tempCalDays = tempCalDays - 1;
                    }

                    var temp = shares_quotes_date.SessionDic[item.Key].Take(tempCalDays).LastOrDefault().Value;
                    if (temp==null)
                    {
                        continue;
                    }
                    DateTime minDate = temp.Date;

                    long maxPrice_date = 0;
                    long price_today = item.Value.shares_quotes_info.ClosedPrice;

                    if (calPriceType == 1)//开盘价
                    {
                        maxPrice_date = shares_quotes_date.OpenSessionDic[item.Key].Where(e=>e.Date>= minDate).FirstOrDefault().OpenedPrice;
                    }
                    else if (calPriceType == 2)//收盘价
                    {
                        maxPrice_date = shares_quotes_date.CloseSessionDic[item.Key].Where(e => e.Date >= minDate).FirstOrDefault().ClosedPrice;
                    }
                    else if (calPriceType == 3)//最高价
                    {
                        maxPrice_date = shares_quotes_date.MaxSessionDic[item.Key].Where(e => e.Date >= minDate).FirstOrDefault().MaxPrice;
                    }
                    else if (calPriceType == 4)//最低价
                    {
                        maxPrice_date = shares_quotes_date.MinSessionDic[item.Key].Where(e => e.Date >= minDate).FirstOrDefault().MinPrice;
                    }

                    if (price_today >= maxPrice_date)
                    {
                        result.Add(item.Key,true);
                    }
                }
                ToWriteDebugLog("25:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            }
            return result;
        }

        /// <summary>
        /// 计算板块量比
        /// </summary>
        /// <param name="plateId"></param>
        /// <param name="PlateVolume"></param>
        private void CalVolumeRate(long plateId,ref PlateVolumeInfo PlateVolume) 
        {
            PlateVolume = new PlateVolumeInfo 
            {
                CurrRate=0,
                ExceptRate=0
            };
            if (!sessionContainer.PlateQuotesLastSession.ContainsKey(plateId))
            {
                return;
            }
            var temp = sessionContainer.PlateQuotesLastSession[plateId];
            PlateVolume.CurrRate = temp.RateNow;
            PlateVolume.ExceptRate = temp.RateExpect;
            return;
        }

        /// <summary>
        /// 启动定时器
        /// </summary>
        private void StartTimer()
        {
            calTimer = new Timer(_doCalTask, null,0,Timeout.Infinite);
        }

        private void ChangeTimer() 
        {
            if (calTimer != null)
            {
                calTimer.Change(CalInterval, Timeout.Infinite);//重启计时器
            }
        }

        private void CloseTimer()
        {
            if (calTimer != null)
            {
                calTimer.Dispose();
                calTimer = null;
            }
        }

        public void Dispose()
        {
            CloseTimer();
        }
    }
}
