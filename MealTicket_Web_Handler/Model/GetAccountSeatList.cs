using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountSeatList
    {
    }

    public class GetAccountSeatListRequest:PageRequest
    {
        /// <summary>
        /// 0全部 1有效
        /// </summary>
        public int Status { get; set; }
    }

    public class AccountSeatInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public string ValidEndTime { get; set; }

        /// <summary>
        /// 绑定的自选股Id
        /// </summary>
        public long OptionalId { get; set; }
    }
}
