using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    public class GetFrontAccountSharesTradeAbnormalRecordListRequest:PageRequest
    {
        /// <summary>
        /// 0队列自动处理 2队列手动处理
        /// </summary>
        public int HandlerType { get; set; }
    }
}
