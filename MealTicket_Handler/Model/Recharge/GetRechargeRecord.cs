using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetRechargeRecord
    {
    }

    public class GetRechargeRecordRequest:PageRequest
    {
        /// <summary>
        /// 年份（0表示全部）
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 月份（0表示全部）
        /// </summary>
        public int Month { get; set; }
    }

    public class RechargeRecordInfo
    {
        /// <summary>
        /// 渠道code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 充值金额
        /// </summary>
        public long RechargeAmount { get; set; }

        /// <summary>
        /// 充值时间
        /// </summary>
        public DateTime RechargedTime { get; set; }
    }
}
