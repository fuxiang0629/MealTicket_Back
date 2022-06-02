using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesRiserateRankList
    {
    }

    public class GetSharesRiserateRankListRequest
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 0综合 13天 25天 310天 415天
        /// </summary>
        public int DaysType { get; set; }
    }

    public class SharesRiserateRankInfo
    {
        /// <summary>
        /// 内部日期
        /// </summary>
        public DateTime DateInside { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string Date
        {
            get
            {
                if (DateInside == DateTime.Now.Date)
                {
                    return "今日";
                }
                return DateInside.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 0综合排名 1.3天 2.5天 3.10天 4.15天
        /// </summary>
        public int DayType { get; set; }

        /// <summary>
        /// 数据数量
        /// </summary>
        public int DataCount
        {
            get
            {
                return DetailsList.Count();
            }
        }

        /// <summary>
        /// 详细列表
        /// </summary>
        public List<SharesRiserateRankDetails> DetailsList { get; set; }
    }

    public class SharesRiserateRankDetails
    {
        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }

        /// <summary>
        /// 市场
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
        /// 历史综合排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 股票当前涨跌幅
        /// </summary>
        public int RiseRate { get; set; }

        public bool IsSuspension{ get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 是否炸板
        /// </summary>
        public bool IsLimitUpBomb { get; set; }

        /// <summary>
        /// 是否一字
        /// </summary>
        public bool IsLimitUpLikeOne { get; set; }

        /// <summary>
        /// 是否T字
        /// </summary>
        public bool IsLimitUpLikeT { get; set; }

        /// <summary>
        /// 是否涨停
        /// </summary>
        public bool IsLimitUp { get; set; }

        /// <summary>
        /// 涨停时间
        /// </summary>
        public DateTime LimitUpTime { get; set; }

        /// <summary>
        /// 涨跌额
        /// </summary>
        public long RiseAmount { get; set; }

        /// <summary>
        /// 股票综合涨跌幅
        /// </summary>
        public int OverallRiseRate { get; set; }

        /// <summary>
        /// 今日综合排名
        /// </summary>
        public int OverallRank { get; set; }

        /// <summary>
        /// 预期量比
        /// </summary>
        public int RateExpect { get; set; }

        /// <summary>
        /// 当前量比
        /// </summary>
        public int RateNow { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo> PlateList { get; set; }

        /// <summary>
        /// 板块排名
        /// </summary>
        public List<PlateSharesRank> PlateSharesRank { get; set; }

        /// <summary>
        /// 所属板块Id列表
        /// </summary>
        public List<long> TotalPlateIdList 
        {
            get 
            {
                if (PlateList == null)
                {
                    return new List<long>();
                }
                return PlateList.Where(e => e.IsFocusOn || e.IsTrendLike || e.IsForce1.Count() > 0 || e.IsForce2.Count() > 0 || e.Type == 1).Select(e => e.Id).ToList();
            }
        }

        /// <summary>
        /// 所属板块所有联动板块Id列表
        /// </summary>
        public List<long> TotalLinkagePlateIdList { get; set; }

        public bool showChildren { get; set; }


        public int PlateShowCount 
        { 
            get 
            { 
                return 3;
            }
        }

        public int DropDown
        {
            get
            {
                if (PlateList.Count() > 2)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 背景色
        /// </summary>
        public string BgColor { get; set; }
    }
}
