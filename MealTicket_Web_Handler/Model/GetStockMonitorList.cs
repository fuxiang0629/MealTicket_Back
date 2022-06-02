using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetStockMonitorList
    {
    }

    public class StockMonitorInfo
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 详情列表
        /// </summary>
        public List<StockMonitorDetails> DetailsList { get; set; }
    }

    public class StockMonitorDetails
    {
        /// <summary>
        /// 关系Id
        /// </summary>
        public long RelId { get; set; }

        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }

        /// <summary>
        /// 市场代码
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
        /// 涨跌幅
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 涨跌额
        /// </summary>
        public long RiseAmount { get; set; }

        /// <summary>
        /// 是否涨停
        /// </summary>
        public int PriceType { get; set; }

        /// <summary>
        /// 涨停时间
        /// </summary>
        public DateTime LimitUpTime { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 今日综合排名
        /// </summary>
        public int OverallRank { get; set; }

        /// <summary>
        /// 综合涨幅
        /// </summary>
        public int OverallRiseRate { get; set; }

        /// <summary>
        /// 预期量比
        /// </summary>
        public int RateExpect { get; set; }

        /// <summary>
        /// 当前量比
        /// </summary>
        public int RateNow { get; set; }

        /// <summary>
        /// 是否置顶大哥
        /// </summary>
        public bool IsTop { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo> PlateList { get; set; }

        /// <summary>
        /// 板块排名
        /// </summary>
        public List<PlateSharesRank> PlateSharesRank { get; set; }

        /// <summary>
        /// 数据添加时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime? TriTime { get; set; }

        /// <summary>
        /// 类型1新数据 2当日新高 3当日新低 4涨幅达标 5跌幅达标
        /// </summary>
        public int TriType { get; set; }

        /// <summary>
        /// 最近获取数据时间
        /// </summary>
        public DateTime? DataGetTime { get; set; }

        /// <summary>
        /// 触发次数
        /// </summary>
        public int TriCount { get; set; }

        public bool ShowTri 
        {
            get
            {
                if (TriCount <= 0 || DataGetTime == null)
                {
                    return false;
                }
                DateTime timeNow = DateTime.Now;
                int intervalSecond = (int)(timeNow - DataGetTime.Value).TotalSeconds;

                if (intervalSecond < 60)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
