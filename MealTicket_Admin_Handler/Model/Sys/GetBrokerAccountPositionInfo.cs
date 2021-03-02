using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBrokerAccountPositionInfo
    {
    }

    public class GetBrokerAccountPositionInfoRequest : PageRequest
    {
        /// <summary>
        /// 券商账户code
        /// </summary>
        public string TradeAccountCode { get; set; }
    }

    public class GetBrokerAccountPositionInfoRes
    {
        /// <summary>
        /// 余额
        /// </summary>
        public long TotalBalance { get; set; }

        /// <summary>
        /// 可用
        /// </summary>
        public long AvailableBalance { get; set; }

        /// <summary>
        /// 冻结
        /// </summary>
        public long FreezeBalance { get; set; }

        /// <summary>
        /// 可取
        /// </summary>
        public long WithdrawBalance { get; set; }

        /// <summary>
        /// 总资产
        /// </summary>
        public long TotalValue { get; set; }

        /// <summary>
        /// 市值
        /// </summary>
        public long MarketValue { get; set; }

        /// <summary>
        /// 同步状态0正常 1同步中
        /// </summary>
        public int SynchronizationStatus { get; set; }

        /// <summary>
        /// 持仓列表
        /// </summary>
        public PageRes<BrokerAccountPositionInfo> PositionList { get; set; }
    }

    public class BrokerAccountPositionInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 市场0深圳 1上海
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 持仓数量
        /// </summary>
        public int TotalSharesCount { get; set; }

        /// <summary>
        /// 可卖数量
        /// </summary>
        public int CanSoldSharesCount { get; set; }

        /// <summary>
        /// 成本价
        /// </summary>
        public long CostPrice { get; set; }

        /// <summary>
        /// 当前价
        /// </summary>
        public long CurrPrice { get; set; }

        /// <summary>
        /// 今买数量
        /// </summary>
        public int BuyCountToday { get; set; }

        /// <summary>
        /// 今卖数量
        /// </summary>
        public int SellCountToday { get; set; }

        /// <summary>
        /// 市值
        /// </summary>
        public long MarketValue { get; set; }

        /// <summary>
        /// 盈亏
        /// </summary>
        public long ProfitValue { get; set; }

        /// <summary>
        /// 盈亏比例
        /// </summary>
        public int ProfitRate { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}
