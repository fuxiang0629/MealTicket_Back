using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_APIService.runner
{
    /// <summary>
    /// 市场情绪计算
    /// </summary>
    public class MarketSentimentCalRunner:Runner
    {
        bool isInit = true;

        public MarketSentimentCalRunner()
        {
            Name = "MarketSentimentCalRunner";
            SleepTime = 3000;
        }

        public override bool Check
        {
            get
            {
                try
                {
                    if (!DbHelper.CheckTradeTime7() && !isInit)
                    {
                        return false;
                    }
                    isInit = false;
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public override void Execute()
        {
            try
            {
                //Logger.WriteFileLog(DateTime.Now.ToString("开始yyyy-MM-dd HH:mm:ss.fff"), null);
                var date = DbHelper.GetLastTradeDate2(0, 0, 0, 0, DateTime.Now.AddHours(-9));
                MarketSentimentCalHelper.Cal_MarketSentiment(date, true);

                //var date = DbHelper.GetLastTradeDate2(0, 0, 0, 0, DateTime.Now.Date);
                //DateTime endDate = DateTime.Parse("2022-06-28 00:00:00");
                //for (DateTime tempDate = date; tempDate >= endDate; tempDate = tempDate.AddDays(-1))
                //{
                //    if (!DbHelper.CheckTradeDate(tempDate))
                //    {
                //        continue;
                //    }
                //    MarketSentimentCalHelper.Cal_MarketSentiment(tempDate, false);
                //}
                //Logger.WriteFileLog(DateTime.Now.ToString("结束yyyy-MM-dd HH:mm:ss.fff"), null);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("MarketSentimentCalRunner", ex);
            }
        }
    }
}
