using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddAccountSharesGroupShareActive
    {
    }

    public class AddAccountSharesGroupShareActiveRequest
    {
        /// <summary>
        /// 共享给账户手机号
        /// </summary>
        public string ShareAccountMobile { get; set; }

        /// <summary>
        /// 分组Id列表
        /// </summary>
        public List<long> GroupIdList { get; set; }
    }
}
