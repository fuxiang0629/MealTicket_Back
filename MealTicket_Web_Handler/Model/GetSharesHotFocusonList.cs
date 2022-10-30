using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesHotFocusonList
    {
    }

    public class GetSharesHotFocusonListRequest
    {
        /// <summary>
        /// 题材Id
        /// </summary>
        public long HotId { get; set; }
    }

    public class SharesHotFocusonInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 股票位置值
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

        public int RiseRate { get; set; }

        public long RiseAmount { get; set; }

        public long ClosedPrice { get; set; }

        /// <summary>
        /// 数据添加时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
