using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    public class SuspensionInfo
    {
        /// <summary>
        /// 市场0深圳 1上海
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
        /// 停牌起始时间
        /// </summary>
        public DateTime SuspensionStartTime { get; set; }

        /// <summary>
        /// 停牌终止时间
        /// </summary>
        public DateTime SuspensionEndTime { get; set; }
    }
}
