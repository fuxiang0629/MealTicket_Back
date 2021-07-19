﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountTrendTriList
    {
    }

    public class GetAccountTrendTriListRequest
    {
        /// <summary>
        /// 最后数据时间
        /// </summary>
        public DateTime? LastDataTime { get; set; }    
    }

    public class AccountTrendTriInfo
    {
        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// RelId
        /// </summary>
        public long RelId { get; set; }

        /// <summary>
        /// 自选股Id
        /// </summary>
        public long OptionalId { get; set; }

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
        /// 现价
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 涨幅
        /// </summary>
        public int RiseRate 
        {
            get 
            {
                if (ClosedPrice <= 0) 
                {
                    return 0;
                }
                return (int)Math.Round(((PresentPrice - ClosedPrice) * 1.0 / ClosedPrice) * 10000, 0);
            } 
        }

        /// <summary>
        /// 开盘涨跌幅
        /// </summary>
        public int OpenedRiseRate
        {
            get
            {
                if (ClosedPrice <= 0)
                {
                    return 0;
                }
                return (int)Math.Round(((OpenedPrice - ClosedPrice) * 1.0 / ClosedPrice) * 10000, 0);
            }
        }

        /// <summary>
        /// 昨日涨跌幅
        /// </summary>
        public int YestodayRiseRate { get; set; }

        /// <summary>
        /// 前2日涨跌幅
        /// </summary>
        public int TwoDaysRiseRate { get; set; }

        /// <summary>
        /// 前3日涨跌幅
        /// </summary>
        public int ThreeDaysRiseRate { get; set; }

        /// <summary>
        /// 触发推送时间
        /// </summary>
        public DateTime PushTime { get; set; }

        /// <summary>
        /// 今日触发次数
        /// </summary>
        public int TriCountToday { get; set; }

        /// <summary>
        /// 文字颜色
        /// </summary>
        public string TextColor 
        {
            get 
            {
                DateTime timeNow = DateTime.Now;
                int intervalSecond = (int)(timeNow - PushTime).TotalSeconds;
                if (intervalSecond < 60)
                {
                    return "rgb(255, 0, 0)";
                }
                else if (intervalSecond < 120)
                {
                    return "rgb(180, 0, 0)";
                }
                else if (intervalSecond < 180)
                {
                    return "rgb(100, 0, 0)";
                }
                else 
                {
                    return "rgb(0, 0, 0)";
                }
            }
        }

        /// <summary>
        /// 走势Id
        /// </summary>
        public long TrendId { get; set; }

        /// <summary>
        /// 触发描述
        /// </summary>
        public string TriDesc { get; set; }

        /// <summary>
        /// 所属分组Id列表
        /// </summary>
        public List<AccountTrendTriInfoGroup> GroupList { get; set; }

        /// <summary>
        /// 所属行业
        /// </summary>
        public string Industry { get; set; }

        /// <summary>
        /// 主营业务
        /// </summary>
        public string Business { get; set; }

        /// <summary>
        /// 条件买入状态1不存在 2未开启 3已开启
        /// </summary>
        public int ConditionStatus { get; set; }

        public long? ConditionId { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo> PlateList { get; set; }
    }
}
