using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_RiseLimit_Session
    {
		static DateTime Last_Excute_Date = DateTime.Parse("1991-01-01 00:00:00");

		public static Shares_RiseLimit_Session_Obj UpdateSession()
		{
			Logger.WriteFileLog("开始执行涨停板数统计缓存更新",null);
			var time_limit_session = Singleton.Instance.sessionHandler.GetShares_Limit_Time_Session();
			var timeNow = DateTime.Now;
			var dataDate = timeNow.Date;
			using (var db = new meal_ticketEntities())
			{
				if (!DbHelper.CheckTradeDate(timeNow.Date))
				{
					dataDate = DbHelper.GetLastTradeDate2();
				}
				else if (DbHelper.CheckTradeTime7Over(timeNow, time_limit_session))
				{
					dataDate = timeNow.Date;
					if (dataDate > Last_Excute_Date)
					{
						db.P_Shares_LimitUp_His_Cal(dataDate);
                        db.P_Shares_LimitDown_His_Cal(dataDate);
                        Last_Excute_Date = dataDate;
					}
				}
				else
				{
					dataDate = DbHelper.GetLastTradeDate2(0,0,0,-1, timeNow.Date);
				}
				var result = (from item in db.t_shares_quotes_riselimit
							  where item.DataDate == dataDate
							  select new Shares_RiseLimit_Session_Info
							  {
								  LastRiseLimitDate = item.LastRiseLimitDate,
								  SharesCode = item.SharesCode,
								  IsLimitUpToday = item.IsLimitUpToday,
								  IsLimitUpYesday = item.IsLimitUpYesday,
								  LastRiseLimitTime = item.LastRiseLimitTime,
								  Market = item.Market,
								  RiseLimitCount = item.RiseLimitCount,
								  RiseLimitDays = item.RiseLimitDays,
								  SharesKey = item.SharesKey ?? 0
							  }).ToList().ToDictionary(k => k.SharesKey, v => v);
				return new Shares_RiseLimit_Session_Obj 
				{
					DataDate= dataDate,
					DataDic= result
				};
			}
		}

		public static Shares_RiseLimit_Session_Obj CopySessionData(object objData)
		{
			var data = objData as Shares_RiseLimit_Session_Obj;
			return data;
		}
	}
}
