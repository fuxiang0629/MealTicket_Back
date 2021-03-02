using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountBankCardList
    {
    }

    public class GetFrontAccountBankCardListRequest:PageRequest
    {
        /// <summary>
        /// 账户id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 卡号
        /// </summary>
        public string CardNumber { get; set; }
    }

    public class FrontAccountBankCardInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 持卡人姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 卡号
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// 银行名称
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// 银行卡类型
        /// </summary>
        public string CardBreed { get; set; }

        /// <summary>
        /// 持卡人手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 绑定时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
