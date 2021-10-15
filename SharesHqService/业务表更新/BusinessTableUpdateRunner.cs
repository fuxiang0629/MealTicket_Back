using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    public class BusinessTableUpdateRunner : Runner
    {

        public BusinessTableUpdateRunner()
        {
            SleepTime = Singleton.Instance.BusinessRunTime;
            Name = "BusinessTableUpdateRunner";
        }

        public override bool Check
        {
            get
            {
                return (Singleton.Instance.GetIsRun() && Helper.CheckTradeTimeForQuotes());
            }
        }

        public override void Execute()
        {
            if (!Singleton.Instance.TryQuotesBusinessCanEnter())
            {
                return;
            }
            Task task = new Task(() =>
            {
                try
                {
                    DoTask();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("更新五档业务数据出错", ex);
                }
                finally
                {
                    Singleton.Instance.TryQuotesBusinessCanLeave();
                }
            });
            task.Start();
        }

        private void DoTask() 
        {
            ThreadMsgTemplate<int> taskData2 = new ThreadMsgTemplate<int>();
            taskData2.Init();
            taskData2.AddMessage(3);
            //taskData2.AddMessage(4);
            int taskCount = taskData2.GetCount();
            Task[] tArr2 = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                tArr2[i] = new Task(() =>
                {
                    do
                    {
                        int taskType = 0;
                        if (!taskData2.GetMessage(ref taskType, true))
                        {
                            break;
                        }
                        if (taskType == 3)
                        {
                            try
                            {
                                UpdateQuotesRecord();
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteFileLog("UpdateQuotesRecord出错", ex);
                            }
                        }
                        if (taskType == 4)
                        {
                            try
                            {
                                AddPlateRiserate();
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteFileLog("AddPlateRiserate出错", ex);
                            }
                        }
                    } while (true);
                });
                tArr2[i].Start();
            }
            Task.WaitAll(tArr2);
            taskData2.Release();
        }



        /// <summary>
        /// 添加五档记录
        /// </summary>
        public static void UpdateQuotesRecord()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"insert into t_shares_quotes_record
(Market,SharesCode,PresentPrice,ClosedPrice,OpenedPrice,MaxPrice,MinPrice,TotalCount,PresentCount,TotalAmount,InvolCount
 ,OuterCount,BuyPrice1,BuyCount1,BuyPrice2,BuyCount2,BuyPrice3,BuyCount3,BuyPrice4,BuyCount4,BuyPrice5,BuyCount5,SellPrice1
 ,SellCount1,SellPrice2,SellCount2,SellPrice3,SellCount3,SellPrice4,SellCount4,SellPrice5,SellCount5,SpeedUp,Activity
 ,LastModified,LimitUpPrice,LimitDownPrice,PriceType,TriPriceType,TotalCapital,CirculatingCapital)
 select t.Market,t.SharesCode,PresentPrice,ClosedPrice,OpenedPrice,MaxPrice,MinPrice,TotalCount,PresentCount,TotalAmount,InvolCount
 ,OuterCount,BuyPrice1,BuyCount1,BuyPrice2,BuyCount2,BuyPrice3,BuyCount3,BuyPrice4,BuyCount4,BuyPrice5,BuyCount5,SellPrice1
 ,SellCount1,SellPrice2,SellCount2,SellPrice3,SellCount3,SellPrice4,SellCount4,SellPrice5,SellCount5,SpeedUp,Activity
 ,LastModified,LimitUpPrice,LimitDownPrice,PriceType,TriPriceType,isnull(t2.TotalCapital,0),isnull(t2.CirculatingCapital,0)
 from v_shares_quotes_last t with(nolock)
 left join t_shares_markettime t2 with(nolock) on t.Market=t2.Market and t.SharesCode=t2.SharesCode
 where TriPriceType=1;";
                db.Database.ExecuteSqlCommand(sql);
            }
        }

        /// <summary>
        /// 记录板块涨跌幅
        /// </summary>
        private static void AddPlateRiserate()
        {
            string sql = @"declare @riserateTable table
 (
	PlateId bigint,
	PlateName nvarchar(200),
	PlateType int,
	SharesCount int,
	RiseRate bigint,
	RiseIndex bigint,
	WeightRiseRate bigint,
	WeightRiseIndex bigint,
	RiseLimitCount int,
	DownLimitCount int,
	LastModified datetime
 )
 insert into @riserateTable
 select PlateId,PlateName,PlateType,SharesCount,RiseRate,RiseIndex,WeightRiseRate,WeightRiseIndex,RiseLimitCount,DownLimitCount,getdate() 
 from v_plate where [Status]=1;


 insert into t_shares_plate_riserate 
 select PlateId,PlateName,PlateType,SharesCount,RiseRate,LastModified,RiseIndex,WeightRiseRate,WeightRiseIndex,RiseLimitCount,DownLimitCount
 from @riserateTable;

 merge into t_shares_plate_riserate_last as t
 using (select * from @riserateTable) as t1
 ON t.PlateId = t1.PlateId
 when matched
 then update set t.SharesCount = t1.SharesCount,t.RiseRate = t1.RiseRate,t.RiseIndex = t1.RiseIndex,
 t.WeightRiseRate = t1.WeightRiseRate,t.WeightRiseIndex = t1.WeightRiseIndex,t.RiseLimitCount = t1.RiseLimitCount,
 t.DownLimitCount = t1.DownLimitCount,t.LastModified = t1.LastModified
 when not matched by target
 then insert(PlateId,SharesCount,RiseRate,RiseIndex,WeightRiseRate,WeightRiseIndex,RiseLimitCount,DownLimitCount,LastModified) 
 values(t1.PlateId,t1.SharesCount,t1.RiseRate,t1.RiseIndex,t1.WeightRiseRate,t1.WeightRiseIndex,t1.RiseLimitCount,t1.DownLimitCount,t1.LastModified);";
            using (var db = new meal_ticketEntities())
            {
                db.Database.ExecuteSqlCommand(sql);
            }
        }

    }
}
