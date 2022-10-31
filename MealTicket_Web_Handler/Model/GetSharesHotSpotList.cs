using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesHotSpotList
    {
    }

    public class GetSharesHotSpotListPar 
    {
        public long AccountId { get; set; }

        public int DaysType { get; set; }
        public long GroupId { get; set; }
    }

    public class GetSharesHotSpotListRequest
    {
        /// <summary>
        /// 0综合 13天 25天 310天 415天
        /// </summary>
        public int DaysType { get; set; }

        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }
    }

    public class SharesHotSpotInfoPar 
    {
        public int DaysType { get; set; }

        public long HotId { get; set; }

        public Dictionary<long, DateTime> FocusonDictionary { get; set; }
    }

    public class SharesHotSpotInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 昨日涨停表现
        /// </summary>
        public int RiseRateYestoday { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 今日涨停家数
        /// </summary>
        public int LimitUpCount { get; set; }

        /// <summary>
        /// 今日跌停家数
        /// </summary>
        public int LimitDownCount { get; set; }

        /// <summary>
        /// 炸板家数
        /// </summary>
        public int LimitUpBombCount { get; set; }

        /// <summary>
        /// 是否显示背景色
        /// </summary>
        public bool ShowBgColor { get; set; }

        /// <summary>
        /// 背景色
        /// </summary>
        public string BgColor { get; set; }

        /// <summary>
        /// 板块Id列表
        /// </summary>
        public List<long> PlateIdList { get; set; }

        /// <summary>
        /// 分组Id列表
        /// </summary>
        public List<long> GroupIdList { get; set; }

        /// <summary>
        /// 板块名称
        /// </summary>
        public string PlateName { get; set; }

        /// <summary>
        /// 是否只显示关注股票
        /// </summary>
        public bool IsShowFocuson { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<SharesRiseLimitInfo> TableList { get; set; }
    }

    public class SharesRiseLimitInfo
    {
        /// <summary>
        /// 板数名称
        /// </summary>
        public string LimitUpName { get; set; }

        /// <summary>
        /// 股票Key
        /// </summary>
        public long SharesKey { get; set; }

        /// <summary>
        /// 市场code
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票Code
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 股票当前涨跌幅
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 涨跌额
        /// </summary>
        public long RiseAmount { get; set; }

        /// <summary>
        /// 是否停牌
        /// </summary>
        public bool IsSuspension { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 预期量比
        /// </summary>
        public int RateExpect { get; set; }

        /// <summary>
        /// 当前量比
        /// </summary>
        public int RateNow { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime TriTime { get; set; }

        /// <summary>
        /// 涨停时间（用于排序）
        /// </summary>
        public DateTime LimitUpTime { get; set; }

        /// <summary>
        /// 即将涨停触发时间
        /// </summary>
        public DateTime NearLimitUpTime { get; set; }

        /// <summary>
        /// 再触发类型
        /// </summary>
        public int NearLimitLastPushType { get; set; }

        /// <summary>
        /// 炸板时间
        /// </summary>
        public DateTime LimitUpBombTime { get; set; }

        /// <summary>
        /// 展示颜色状态1涨停1分钟内 2炸板1分钟内 0正常
        /// </summary>
        public int TextColorStatus 
        {
            get
            {
                DateTime timeNow = DateTime.Now;
                if (!IsLimitUpBomb && !IsLimitUpToday)
                {
                    return 0;
                }
                if (IsLimitUpBomb && (timeNow- LimitUpBombTime).TotalSeconds<= 60 && (timeNow - LimitUpBombTime).TotalSeconds >=0)
                {
                    return 2;
                }
                if (IsLimitUpToday && (timeNow - LimitUpTime).TotalSeconds <= 60 && (timeNow - LimitUpTime).TotalSeconds >=0)
                {
                    return 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// 是否炸板
        /// </summary>
        public bool IsLimitUpBomb { get; set; }

        /// <summary>
        /// 炸板次数
        /// </summary>
        public int LimitUpBombCount { get; set; }

        /// <summary>
        /// 是否一字
        /// </summary>
        public bool IsLimitUpLikeOne { get; set; }

        /// <summary>
        /// 是否T字
        /// </summary>
        public bool IsLimitUpLikeT { get; set; }

        /// <summary>
        /// 是否反包
        /// </summary>
        public bool IsReverse { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 涨停次数
        /// </summary>
        public int RiseLimitCount { get; set; }

        /// <summary>
        /// 涨停次数
        /// </summary>
        public int TempRiseLimitCount { get; set; }

        /// <summary>
        /// 天数
        /// </summary>
        public int RiseLimitDays { get; set; }

        /// <summary>
        /// 今天是否涨停
        /// </summary>
        public bool IsLimitUpToday { get; set; }

        /// <summary>
        /// 昨天是否涨停
        /// </summary>
        public bool IsLimitUpYesday { get; set; }

        /// <summary>
        /// 今天是否跌停
        /// </summary>
        public bool IsLimitDownToday { get; set; }

        /// <summary>
        /// 当天是否即将涨停
        /// </summary>
        public bool IsNearLimit { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo> PlateList { get; set; }

        /// <summary>
        /// 板块排名
        /// </summary>
        public List<PlateSharesRank> PlateSharesRank { get; set; }

        /// <summary>
        /// 所属真实板块Id列表
        /// </summary>
        public List<long> TotalPlateIdList{ get; set; }

        /// <summary>
        /// 所属板块Id列表
        /// </summary>
        public List<long> AllPlateIdList { get; set; }

        /// <summary>
        /// 所属板块所有联动板块Id列表
        /// </summary>
        public List<long> TotalLinkagePlateIdList { get; set; }

        /// <summary>
        /// 是否关注
        /// </summary>
        public bool IsFocuson { get; set; }
    }
}
