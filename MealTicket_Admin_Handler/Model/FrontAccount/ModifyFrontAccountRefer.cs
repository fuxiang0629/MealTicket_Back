using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyFrontAccountRefer
    {
    }

    public class ModifyFrontAccountReferRequest
    {
        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 推荐人分享码
        /// </summary>
        public string ReferRecommandCode { get; set; }
    }
}
