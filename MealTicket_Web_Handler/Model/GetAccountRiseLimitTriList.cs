using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountRiseLimitTriList
    {
    }

    public class GetAccountRiseLimitTriListRequest
    {
        public DateTime? LastDataTime { get; set; }
    }

    public class AccountRiseLimitTriInfo
    {
        /// <summary>
        /// 唯一Id
        /// </summary>
        public string RelId { get; set; }

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
        /// 所属行业
        /// </summary>
        public string Industry { get; set; }

        /// <summary>
        /// 主营业务
        /// </summary>
        public string Business { get; set; }

        /// <summary>
        /// 类型-1涨停池 -2跌停池 -3即将涨停池 -4即将跌停池 -5炸板池 -6翘板池
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 最大涨跌幅
        /// </summary>
        public long Range { get; set; }

        public int Fundmultiple { get; set; }

        /// <summary>
        /// 所属自定义分组
        /// </summary>
        public List<long> GroupList { get; set; }

        /// <summary>
        /// 条件买入状态1不存在 2未开启 3已开启
        /// </summary>
        public int ConditionStatus { get; set; }
    }
}
