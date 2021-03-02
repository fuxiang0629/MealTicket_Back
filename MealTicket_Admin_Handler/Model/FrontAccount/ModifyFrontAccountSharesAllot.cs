using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyFrontAccountSharesAllot
    {
    }

    public class ModifyFrontAccountSharesAllotRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }


        /// <summary>
        /// 市场0深圳 1上海
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 派股信息
        /// </summary>
        public string AllotSharesInfo { get; set; }

        /// <summary>
        /// 派息信息
        /// </summary>
        public string AllotBonusInfo { get; set; }

        /// <summary>
        /// 派息日期
        /// </summary>
        public DateTime AllotDate { get; set; }
    }
}
