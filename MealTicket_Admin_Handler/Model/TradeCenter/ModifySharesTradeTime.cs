using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySharesTradeTime
    {
    }

    public class ModifySharesTradeTimeRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// 类型1.开市竞价时间 2.等待开市时间 3.开市时间 4.尾市竞价时间 5.闭市时间 6交易时间
        /// </summary>
        public int Type { get; set; }
    }
}
