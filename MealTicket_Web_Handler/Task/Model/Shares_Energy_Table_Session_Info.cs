using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Energy_Table_Session_Info
    {
        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 涨跌额
        /// </summary>
        public long RiseAmount { get; set; }

        /// <summary>
        /// 是否炸板
        /// </summary>
        public bool IsLimitUpBomb { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime TriTime { get; set; }

        /// <summary>
        /// 触发次数
        /// </summary>
        public int TriCount { get; set; }

        /// <summary>
        /// 预期量比
        /// </summary>
        public int RateExpect { get; set; }

        /// <summary>
        /// 当前量比
        /// </summary>
        public int RateNow { get; set; }
    }
}
