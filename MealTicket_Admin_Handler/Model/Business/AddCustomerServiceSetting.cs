using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddCustomerServiceSetting
    {
    }

    public class AddCustomerServiceSettingRequest
    {
        /// <summary>
        /// 页面code
        /// </summary>
        public string PageCode { get; set; }

        /// <summary>
        /// 页面名称
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// 图标地址
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 微信号码
        /// </summary>
        public string WechatNumber { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        public string Mobile { get; set; }
    }
}
