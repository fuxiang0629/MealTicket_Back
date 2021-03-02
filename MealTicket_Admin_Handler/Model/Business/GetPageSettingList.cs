using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetPageSettingList
    {
    }

    public class GetPageSettingListRequest:PageRequest
    {
        /// <summary>
        /// 页面code
        /// </summary>
        public string PageCode { get; set; }
    }

    public class PageSettingInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 页面code
        /// </summary>
        public string PageCode { get; set; }

        /// <summary>
        /// 页面名称
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// 状态1有效 无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
