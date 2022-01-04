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
        public static Dictionary<long,List<Plate_Minute_KLine_Session_Info>> UpdateSession()
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
				return result.GroupBy(e => e.PlateId).ToDictionary(k => k.Key, v => v.OrderByDescending(e=>e.Time).ToList());
            }
        }

        public static Dictionary<long, List<Plate_Minute_KLine_Session_Info>> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, List<Plate_Minute_KLine_Session_Info>>;
			Dictionary<long, List<Plate_Minute_KLine_Session_Info>> resultData = new Dictionary<long, List<Plate_Minute_KLine_Session_Info>>();
			foreach (var item in data)
			{
				resultData[item.Key] = new List<Plate_Minute_KLine_Session_Info>(item.Value);
			}
            return resultData;
        }
    }
}
