using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBrokerAccountBindFrontAccountList
    {
    }

    public class BrokerAccountBindFrontAccountInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 前端账户id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 前端账户昵称
        /// </summary>
        public string AccountNickName { get; set; }

        /// <summary>
        /// 前端账户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
