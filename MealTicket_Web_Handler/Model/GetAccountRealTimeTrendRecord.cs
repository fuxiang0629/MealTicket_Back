using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountRealTimeTrendRecord
    {
    }

    public class GetAccountRealTimeTrendRecordRequest:PageRequest
    {
        /// <summary>
        /// 股票信息
        /// </summary>
        public string SharesInfo { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime? Date { get; set; }
    }

    public class AccountRealTimeTrendRecordInfo
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
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 最新触发类型
        /// </summary>
        public string LastTrendName { get; set; }

        /// <summary>
        /// 最新触发价格
        /// </summary>
        public long LastTriPrice { get; set; }

        /// <summary>
        /// 当日触发数量
        /// </summary>
        public int TriCount { get; set; }

        /// <summary>
        /// 当前价格
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 最新触发时间
        /// </summary>
        public DateTime LastTime { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 最新触发触发描述
        /// </summary>
        public string LastTriDesc { get; set; }
    }
}
