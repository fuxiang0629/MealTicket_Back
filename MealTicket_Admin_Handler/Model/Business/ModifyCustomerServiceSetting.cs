using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyCustomerServiceSetting
    {
    }

    public class ModifyCustomerServiceSettingRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set;}

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
