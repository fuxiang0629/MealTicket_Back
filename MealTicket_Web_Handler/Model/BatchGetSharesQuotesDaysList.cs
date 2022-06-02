using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BatchGetSharesQuotesDaysList
    {
    }

    public class BatchGetSharesQuotesDaysListRequest
    {
        public DateTime StartDate { get; set; }

        public List<long> SharesKeyList { get; set; }
    }

    public class SharesQuotesDaysInfo
    {
        public long SharesKey { get; set; }

        public string SharesCode { get; set; }

        public string SharesName { get; set; }

        public List<SharesQuotesDaysDetails> LineList { get; set; }
    }

    public class SharesQuotesDaysDetails
    {
        public string Date { get; set; }

        public long PresentPrice { get; set; }

        public long ClosePrice { get; set; }

        public long RiseRate
        {
            get 
            {
                if (ClosePrice == 0)
                {
                    return 0;
                }
                return (long)Math.Round((PresentPrice - ClosePrice) * 1.0 / ClosePrice * 10000, 0);
            }
        }
    }

    public class BatchGetSharesQuotesDaysListRes
    {
        /// <summary>
        /// X轴坐标
        /// </summary>
        public List<string> XData { get; set; }

        /// <summary>
        /// 线条列表
        /// </summary>
        public List<SharesQuotesDaysInfo> SharesLineList { get; set; }
    }
}
