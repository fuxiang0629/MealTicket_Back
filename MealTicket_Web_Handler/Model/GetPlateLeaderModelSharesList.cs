using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetPlateLeaderModelSharesList
    {
    }

    public struct SharesRank
    {
        public int DayType { get; set; }

        public long SharesKey { get; set; }

        public int RiseRate { get; set; }

        public int PlateRank { get; set; }

        public int TotalRank { get; set; }

        public int RealSharesCount { get; set; }

        public int RealDays { get; set; }

        public int LeaderType { get; set; }

        public int DayLeaderType { get; set; }

        public int MainArmyType { get; set; }

        public bool IsLeader 
        {
            get 
            {
                if (Singleton.Instance.SharesLeaderType.Contains(1) && LeaderType > 0)
                {
                    return true;
                }
                if (Singleton.Instance.SharesLeaderType.Contains(2) && DayLeaderType > 0)
                {
                    return true;
                }
                if (Singleton.Instance.SharesLeaderType.Contains(3) && MainArmyType > 0)
                {
                    return true;
                }
                return false;
            }  
        }
    }
    public class PlateSharesInfo
    {
        /// <summary>
        /// 条件买入Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 条件买入状态
        /// </summary>
        public int ConditionStatus { get; set; }

        /// <summary>
        /// 关系Id
        /// </summary>
        public long RelId { get; set; }

        /// <summary>
        /// 自定义分组Id
        /// </summary>
        public List<long> GroupList { get; set; }

        /// <summary>
        /// 市场代码0深圳 1上海
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
        /// 涨跌幅(1/万)
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 涨跌价
        /// </summary>
        public long RisePrice { get; set; }

        /// <summary>
        /// 当前价
        /// </summary>
        public long CurrPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 总参数数量
        /// </summary>
        public int ParTotalCount { get; set; }

        /// <summary>
        /// 有效参数数量
        /// </summary>
        public int ParValidCount { get; set; }

        /// <summary>
        /// 已执行参数数量
        /// </summary>
        public int ParExecuteCount { get; set; }

        /// <summary>
        /// 最大涨跌幅
        /// </summary>
        public int Range { get; set; }

        /// <summary>
        /// 杠杆倍数
        /// </summary>
        public int Fundmultiple { get; set; }

        /// <summary>
        /// 上市状态1正常 2退市
        /// </summary>
        public int MarketStatus { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo> PlateList { get; set; }

        /// <summary>
        /// 是否存在股票监控列表
        /// </summary>
        public bool IsExists { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 换手率
        /// </summary>
        public int TodayHandsRate
        {
            get
            {
                if (CirculatingCapital == 0)
                {
                    return 0;
                }
                return (int)((TodayDealCount * 1.0 / CirculatingCapital) * 10000);
            }
        }

        /// <summary>
        /// 流通股本
        /// </summary>
        public long CirculatingCapital { get; set; }

        /// <summary>
        /// 成交金额
        /// </summary>
        public long TodayDealAmount { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public long TodayDealCount { get; set; }

        /// <summary>
        /// 自选股分组数量
        /// </summary>
        public List<long> MySharesGroupList { get; set; }

        /// <summary>
        /// 此刻量比
        /// </summary>
        public int StockRate_Now { get; set; }

        /// <summary>
        /// 预计量比
        /// </summary>
        public int StockRate_All { get; set; }

        /// <summary>
        /// 是否外面传入的股票
        /// </summary>
        public bool IsMain { get; set; }

        /// <summary>
        /// 股票排名信息
        /// </summary>
        public List<SharesRank> Rank { get; set; }

        /// <summary>
        /// 板块内股票排名信息
        /// </summary>
        public List<PlateSharesRank> PlateSharesRank { get; set; }

        /// <summary>
        /// 综合排名
        /// </summary>
        public int OverallRank { get; set; }

        /// <summary>
        /// 综合涨跌幅
        /// </summary>
        public int OverallRiseRate { get; set; }
    }

    public class PlateSharesRank 
    {
        public long PlateId { get; set; }

        public bool IsLeader 
        {
            get 
            {
                if (Rank.Count() == 0)
                {
                    return false;
                }
                return Rank.Where(e => e.IsLeader).Count() > 0;
            }
        }

        public bool IsShowPlate { get; set; }

        public int OverallRank 
        {
            get 
            {
                if (Rank == null)
                {
                    return int.MaxValue;
                }
                var temp = Rank.Where(e => e.PlateRank > 0).ToList();
                if (temp.Count() == 0)
                {
                    return int.MaxValue;
                }
                return (int)(temp.Average(e => e.PlateRank) * 10000);
            }
        }

        /// <summary>
        /// 板块内综合排名
        /// </summary>
        public int OverallPlateRank { get; set; }

        public List<SharesRank> Rank { get; set; }
    }

    public class GetPlateLeaderModelSharesListRequest:PageRequest
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 是否展示联动板块龙头股票
        /// </summary>
        public bool IsLinkagePlate { get; set; }

        /// <summary>
        /// 排序方式1.根据排名排序 2.根据涨跌幅排序
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public string OrderMethod { get; set; }

        /// <summary>
        /// 自带股票市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 自带股票code
        /// </summary>
        public string SharesCode { get; set; }

        public bool IsShowPlateSharesLeader { get; set; }
    }
}
