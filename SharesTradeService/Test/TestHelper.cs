using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesTradeService
{
    public class TestHelper
    {
        /// <summary>
        /// 购买
        /// </summary>
        /// <param name="sResult"></param>
        /// <param name="sErrInfo"></param>
        public static void SendBuyOrder(string SharesCode,string EntrustPrice,string EntrustCount, StringBuilder sResult, StringBuilder sErrInfo) 
        {
            using (var db = new meal_ticketEntities())
            {
                //生成委托编号
                string entrustId = Guid.NewGuid().ToString("N");

                if (EntrustPrice == "0")
                {
                    var quotes = (from item in db.v_shares_quotes_last
                                  where item.SharesCode == SharesCode
                                  select item).FirstOrDefault();
                    if (quotes == null || quotes.PresentPrice<=0)
                    {
                        return;
                    }
                    EntrustPrice = Math.Round(quotes.PresentPrice*1.0/Singleton.instance.PriceFormat,2).ToString();
                }

                db.t_test_entrust.Add(new t_test_entrust
                {
                    SharesCode = SharesCode,
                    StatusDes = "已成",
                    DealCount = EntrustCount,
                    DealPrice = EntrustPrice,
                    EntrustCount = EntrustCount,
                    EntrustId = entrustId,
                    EntrustPrice = EntrustPrice,
                    IsRead = false,
                    TradeType = "0"
                });

                db.t_test_deal.Add(new t_test_deal 
                { 
                    DealCount= EntrustCount,
                    DealPrice= EntrustPrice,
                    EntrustId= entrustId,
                    DealId= Guid.NewGuid().ToString("N")
                });

                db.SaveChanges();

                sResult.AppendLine("委托编号");
                sResult.Append(entrustId);

                return;
            }
        }

        /// <summary>
        /// 卖出
        /// </summary>
        /// <param name="sResult"></param>
        /// <param name="sErrInfo"></param>
        public static void SendSellOrder(string SharesCode, string EntrustPrice, string EntrustCount, StringBuilder sResult, StringBuilder sErrInfo)
        {
            using (var db = new meal_ticketEntities())
            {
                if (int.Parse(EntrustCount) % 100 != 0)
                {
                    return;
                }
                //生成委托编号
                string entrustId = Guid.NewGuid().ToString("N");

                if (EntrustPrice == "0")
                {
                    var quotes = (from item in db.v_shares_quotes_last
                                  where item.SharesCode == SharesCode
                                  select item).FirstOrDefault();
                    if (quotes == null || quotes.PresentPrice <= 0)
                    {
                        return;
                    }
                    EntrustPrice = Math.Round(quotes.PresentPrice * 1.0 / Singleton.instance.PriceFormat, 2).ToString();
                }

                db.t_test_entrust.Add(new t_test_entrust
                {
                    SharesCode = SharesCode,
                    StatusDes = "已成",
                    DealCount = EntrustCount,
                    DealPrice = EntrustPrice,
                    EntrustCount = EntrustCount,
                    EntrustId = entrustId,
                    EntrustPrice = EntrustPrice,
                    IsRead = false,
                    TradeType = "1"
                });

                db.t_test_deal.Add(new t_test_deal
                {
                    DealCount = EntrustCount,
                    DealPrice = EntrustPrice,
                    EntrustId = entrustId,
                    DealId = Guid.NewGuid().ToString("N")
                });

                db.SaveChanges();

                sResult.AppendLine("委托编号");
                sResult.Append(entrustId);

                return;
            }
        }

        /// <summary>
        /// 查询委托记录
        /// </summary>
        /// <param name="sResult"></param>
        /// <param name="sErrInfo"></param>
        public static void GetEntrustRecord(StringBuilder sResult, StringBuilder sErrInfo)
        {
            using (var db = new meal_ticketEntities())
            {
                sResult.Append("委托时间");
                sResult.Append("\t");
                sResult.Append("证券代码");
                sResult.Append("\t");
                sResult.Append("股票名称");
                sResult.Append("\t");
                sResult.Append("交易类型");
                sResult.Append("\t");
                sResult.Append("");
                sResult.Append("\t");
                sResult.Append("");
                sResult.Append("\t");
                sResult.Append("状态描述");
                sResult.Append("\t");
                sResult.Append("委托价格");
                sResult.Append("\t");
                sResult.Append("委托数量");
                sResult.Append("\t");
                sResult.Append("委托编号");
                sResult.Append("\t");
                sResult.Append("成交价格");
                sResult.Append("\t");
                sResult.Append("成交数量");
                sResult.Append("\n");

                var entrust = (from item in db.t_test_entrust
                               select item).ToList();
                foreach (var item in entrust)
                {
                    sResult.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    sResult.Append("\t");
                    sResult.Append(item.SharesCode);
                    sResult.Append("\t");
                    sResult.Append(item.SharesName);
                    sResult.Append("\t");
                    sResult.Append(item.TradeType);
                    sResult.Append("\t");
                    sResult.Append("");
                    sResult.Append("\t");
                    sResult.Append("");
                    sResult.Append("\t");
                    sResult.Append(item.StatusDes);
                    sResult.Append("\t");
                    sResult.Append(item.EntrustPrice);
                    sResult.Append("\t");
                    sResult.Append(item.EntrustCount);
                    sResult.Append("\t");
                    sResult.Append(item.EntrustId);
                    sResult.Append("\t");
                    sResult.Append(item.DealPrice);
                    sResult.Append("\t");
                    sResult.Append(item.DealCount);
                    sResult.Append("\n");
                }
            }
        }

        /// <summary>
        /// 查询成交记录
        /// </summary>
        /// <param name="sResult"></param>
        /// <param name="sErrInfo"></param>
        public static void GetDealRecord(StringBuilder sResult, StringBuilder sErrInfo)
        {
            using (var db = new meal_ticketEntities())
            {
                sResult.Append("成交时间");
                sResult.Append("\t");
                sResult.Append("");
                sResult.Append("\t");
                sResult.Append("");
                sResult.Append("\t");
                sResult.Append("");
                sResult.Append("\t");
                sResult.Append("");
                sResult.Append("\t");
                sResult.Append("成交价格");
                sResult.Append("\t");
                sResult.Append("成交数量");
                sResult.Append("\t");
                sResult.Append("成交金额");
                sResult.Append("\t");
                sResult.Append("成交Id");
                sResult.Append("\t");
                sResult.Append("委托Id");
                sResult.Append("\n");

                var entrust = (from item in db.t_test_deal
                               select item).ToList();
                foreach (var item in entrust)
                {
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    sResult.Append(time);
                    sResult.Append("\t");
                    sResult.Append("");
                    sResult.Append("\t");
                    sResult.Append("");
                    sResult.Append("\t");
                    sResult.Append("");
                    sResult.Append("\t");
                    sResult.Append("");
                    sResult.Append("\t");
                    sResult.Append(item.DealPrice);
                    sResult.Append("\t");
                    sResult.Append(item.DealCount);
                    sResult.Append("\t");
                    sResult.Append(Math.Round(float.Parse(item.DealPrice)* float.Parse(item.DealCount),2).ToString());
                    sResult.Append("\t");
                    sResult.Append(item.DealId);
                    sResult.Append("\t");
                    sResult.Append(item.EntrustId);
                    sResult.Append("\n");
                }
            }
        }
    }
}
