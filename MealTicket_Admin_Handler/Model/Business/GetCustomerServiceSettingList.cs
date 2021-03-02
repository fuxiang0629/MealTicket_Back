using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetCustomerServiceSettingList
    {
    }

    public class GetCustomerServiceSettingListRequest : PageRequest
    {
        /// <summary>
        /// 页面code
        /// </summary>
        public string PageCode { get; set; }
    }

    public class CustomerServiceSettingInfo
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
        /// 图标地址
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 图标地址（网络地址）
        /// </summary>
        public string ImgUrlShow
        {
            get
            {
                if (string.IsNullOrEmpty(ImgUrl) || ImgUrl.ToUpper().Contains("HTTP"))
                {
                    return ImgUrl;
                }
                return string.Format("{0}/{1}",ConfigurationManager.AppSettings["imgurl"], ImgUrl);
            }
        }

        /// <summary>
        /// 微信号码
        /// </summary>
        public string WechatNumber { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
