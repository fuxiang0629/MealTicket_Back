using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class BindServerAccount
    {
    }

    public class BindServerAccountRequest
    {
        /// <summary>
        /// 服务器编号
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// 股票账户Id列表
        /// </summary>
        public List<long> AccountIdList { get; set; }
    }
}
