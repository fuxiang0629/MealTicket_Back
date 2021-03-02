using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyBannerGroup
    {
    }

    public class ModifyBannerGroupRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分组描述
        /// </summary>
        public string GroupDes { get; set; }
    }
}
