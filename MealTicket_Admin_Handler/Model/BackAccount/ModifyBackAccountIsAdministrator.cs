using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyBackAccountIsAdministrator
    {
    }

    public class ModifyBackAccountIsAdministratorRequest
    {
        /// <summary>
        /// 后台用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 是否管理员
        /// </summary>
        public bool IsAdministrator { get; set; }
    }
}
