using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesListQuotes
    {
    }

    public class SharesListQuotesInfo
    {
        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 当前价
        /// </summary>
        public long CurrPrice { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 涨跌价
        /// </summary>
        public long RisePrice { get; set; }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime PushTime { get; set; }

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
    }
}
