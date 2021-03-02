using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesTradeService.Model
{
    public class SharesEntrustInfo
    {
        /// <summary>
        /// 委托时间
        /// </summary>
        public string EntrustTime { get; set; }

        /// <summary>
        /// 证券代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 交易类型0买入 1卖出
        /// </summary>
        public int TradeType { get; set; }

        /// <summary>
        /// 委托状态1未完成 2已完成
        /// </summary>
        public int Status
        {
            get
            {
                if (LastTime.Date != CurrTime.Date)
                {
                    return 1;
                }
                if (StatusDes == "已撤" || StatusDes == "废单")
                {
                    return 2;
                }
                if (Helper.CheckTradeTime(DateTime.Now.AddSeconds(-Singleton.instance.OverSecond)))
                {
                    if ((CurrTime - LastTime).TotalSeconds < Singleton.instance.OverSecond)
                    {
                        return 1;
                    }
                }
                if (StatusDes == "已成" || StatusDes == "部撤")
                {
                    return 2;
                }
                return 1;
            }
        }

        /// <summary>
        /// 状态说明
        /// </summary>
        public string StatusDes { get; set; }

        /// <summary>
        /// 委托价格
        /// </summary>
        public long EntrustPrice { get; set; }

        /// <summary>
        /// 委托数量
        /// </summary>
        public int EntrustCount { get; set; }

        /// <summary>
        /// 委托编号
        /// </summary>
        public string EntrustId { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public long DealPrice { get; set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public int TotalDealCount { get; set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public int DealCount { get; set; }

        /// <summary>
        /// 交易账户
        /// </summary>
        public string TradeAccountCode { get; set; }

        /// <summary>
        /// 上次获取数据时间
        /// </summary>
        public DateTime LastTime { get; set; }

        /// <summary>
        /// 获取数据时间
        /// </summary>
        public DateTime CurrTime { get; set; }
    }
}
