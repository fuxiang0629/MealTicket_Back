using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyFrontAccountBaseInfo
    {
    }

    public class ModifyFrontAccountBaseInfoRequest 
    {
        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 头像地址
        /// </summary>
        public string HeadUrl { get; set; }

        /// <summary>
        /// 性别0未知 1男 2女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 出生年月
        /// </summary>
        public string BirthDay { get; set; }
    }
}
