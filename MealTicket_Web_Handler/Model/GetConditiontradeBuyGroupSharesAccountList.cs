using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetConditiontradeBuyGroupSharesAccountList
    {
    }

    public class ConditiontradeBuyGroupSharesAccount
    {
        /// <summary>
        /// id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 绑定时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
