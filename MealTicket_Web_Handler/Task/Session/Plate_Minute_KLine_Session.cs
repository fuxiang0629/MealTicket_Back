using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Minute_KLine_Session
    {
        public static Plate_Minute_KLine_RiseRate_Obj UpdateSession()
		{
			DateTime dateNow = DbHelper.GetLastTradeDate2();
			using (var db = new meal_ticketEntities())
            {
                string sql = string.Format(@"declare @paramValue_IndexInitValue nvarchar(max);
	select @paramValue_IndexInitValue=ParamValue from t_system_param where ParamName='IndexInitValue';
	declare @plateIndex_KLineType int;
	set @plateIndex_KLineType=dbo.f_parseJson(@paramValue_IndexInitValue,'PlateIndex_KLineType');
	if(@plateIndex_KLineType=1)
	begin
		set @plateIndex_KLineType=2;
	end
	else
	begin
		set @plateIndex_KLineType=1;
	end
	declare @marketIndex_KLineType int;
	set @marketIndex_KLineType=dbo.f_parseJson(@paramValue_IndexInitValue,'MarketIndex_KLineType');
	if(@marketIndex_KLineType=2)
	begin
		set @marketIndex_KLineType=1;
	end
	else
	begin
		set @marketIndex_KLineType=2;
	end
	declare @unitIndex_KLineType int;
	set @unitIndex_KLineType=dbo.f_parseJson(@paramValue_IndexInitValue,'UnitIndex_KLineType');
	if(@unitIndex_KLineType=2)
	begin
		set @unitIndex_KLineType=1;
	end
	else
	begin
		set @unitIndex_KLineType=2;
	end
	declare @otherIndex_KLineType int;
	set @otherIndex_KLineType=dbo.f_parseJson(@paramValue_IndexInitValue,'OtherIndex_KLineType');
	if(@otherIndex_KLineType=1)
	begin
		set @otherIndex_KLineType=2;
	end
	else
	begin
		set @otherIndex_KLineType=1;
	end

	select t.PlateId,t.[Time],t.ClosedPrice,t.PreClosePrice
	from 
	(
		select PlateId,WeightType,[Time],ClosedPrice,PreClosePrice
		from
		(
			select PlateId,WeightType,[Time],ClosedPrice,PreClosePrice,ROW_NUMBER()OVER(partition by PlateId,WeightType order by [Time] desc)num
			from t_shares_plate_securitybarsdata_1min
			where [Time]>'{0}'
		)t
		where t.num<=15
	) t
	inner join 
	(
		select Id PlateId,case when CalType=1 then @plateIndex_KLineType when CalType=2 then @marketIndex_KLineType when CalType=3 then @unitIndex_KLineType else @otherIndex_KLineType end WeightType
		from t_shares_plate
	)t1 on t.PlateId=t1.PlateId and t.WeightType=t1.WeightType", dateNow.ToString("yyyy-MM-dd"));
                var result = db.Database.SqlQuery<Plate_Minute_KLine_Session_Info>(sql).ToList();
				return BuildDic(result);
            }
        }
		private static Plate_Minute_KLine_RiseRate_Obj BuildDic(List<Plate_Minute_KLine_Session_Info> dataList)
		{
			var result = new Plate_Minute_KLine_RiseRate_Obj();
			result.RankInfo = new Plate_Minute_KLine_RiseRate_Rank();
			result.RankInfo.Minute1Rank = new SortedSet<Plate_Minute_KLine_RiseRate_Rank_Info>();
			result.RankInfo.Minute3Rank = new SortedSet<Plate_Minute_KLine_RiseRate_Rank_Info>();
			result.RankInfo.Minute5Rank = new SortedSet<Plate_Minute_KLine_RiseRate_Rank_Info>();
			result.RankInfo.Minute10Rank = new SortedSet<Plate_Minute_KLine_RiseRate_Rank_Info>();
			result.RankInfo.Minute15Rank = new SortedSet<Plate_Minute_KLine_RiseRate_Rank_Info>();
			result.RiseRateInfo = new Dictionary<long, Plate_Minute_KLine_RiseRate>();
			result .DataDic= dataList.GroupBy(e => e.PlateId).ToDictionary(k => k.Key, v => v.OrderByDescending(e => e.Time).ToList());

			var plate_base = Singleton.Instance.sessionHandler.GetPlate_Base_Session();
			foreach(var item in result.DataDic)
			{
				if (!plate_base.ContainsKey(item.Key))
				{
					continue;
				}
				var plateBase = plate_base[item.Key];
				if (plateBase.BaseStatus != 1)
				{
					continue;
				}
				if (!result.RiseRateInfo.ContainsKey(item.Key))
				{
					result.RiseRateInfo.Add(item.Key, new Plate_Minute_KLine_RiseRate());
				}
				var riseInfo = result.RiseRateInfo[item.Key];
				int idx = 0;
				long firstPrice = 0;
				foreach (var min in item.Value)
				{
					if (min.PreClosePrice == 0)
					{
						continue;
					}
					idx++;
					if (idx == 1)
					{
						firstPrice = min.ClosedPrice;
					}
					if (idx <= 1)
					{
						riseInfo.Minute1RiseRate = (int)Math.Round((firstPrice - min.PreClosePrice) * 1.0 / min.PreClosePrice * 10000, 0);
					}
					if (idx <= 3)
					{
						riseInfo.Minute3RiseRate = (int)Math.Round((firstPrice - min.PreClosePrice) * 1.0 / min.PreClosePrice * 10000, 0);
					}
					if (idx <= 5)
					{
						riseInfo.Minute5RiseRate = (int)Math.Round((firstPrice - min.PreClosePrice) * 1.0 / min.PreClosePrice * 10000, 0);
					}
					if (idx <= 10)
					{
						riseInfo.Minute10RiseRate = (int)Math.Round((firstPrice - min.PreClosePrice) * 1.0 / min.PreClosePrice * 10000, 0);
					}
					if (idx <= 15)
					{
						riseInfo.Minute15RiseRate = (int)Math.Round((firstPrice - min.PreClosePrice) * 1.0 / min.PreClosePrice * 10000, 0);
					}
				}

				
				result.RankInfo.Minute1Rank.Add(new Plate_Minute_KLine_RiseRate_Rank_Info 
				{
					PlateId= item.Key,
					RiseRate= riseInfo.Minute1RiseRate
				});
				result.RankInfo.Minute3Rank.Add(new Plate_Minute_KLine_RiseRate_Rank_Info
				{
					PlateId = item.Key,
					RiseRate = riseInfo.Minute3RiseRate
				});
				result.RankInfo.Minute5Rank.Add(new Plate_Minute_KLine_RiseRate_Rank_Info
				{
					PlateId = item.Key,
					RiseRate = riseInfo.Minute5RiseRate
				});
				result.RankInfo.Minute10Rank.Add(new Plate_Minute_KLine_RiseRate_Rank_Info
				{
					PlateId = item.Key,
					RiseRate = riseInfo.Minute10RiseRate
				});
				result.RankInfo.Minute15Rank.Add(new Plate_Minute_KLine_RiseRate_Rank_Info
				{
					PlateId = item.Key,
					RiseRate = riseInfo.Minute15RiseRate
				});
			}
			return result;
		}

		public static Plate_Minute_KLine_RiseRate_Obj CopySessionData(object objData)
        {
            var data = objData as Plate_Minute_KLine_RiseRate_Obj;
			var resultData = new Plate_Minute_KLine_RiseRate_Obj();
			resultData = data;
			return resultData;
        }
	}
}
