using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAutoBuySyncroAuthorizedAccountList
    {
    }

    public class GetAutoBuySyncroAuthorizedAccountListRequest:PageRequest
    { 
        /// <summary>
        /// Id
        /// </summary>
        public long GroupId { get; set; }
    }

    public class AutoBuySyncroAuthorizedAccountInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 账户昵称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 账户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
