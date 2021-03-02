using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetServiceInfo
    {
    }

    public class GetServiceInfoRequest
    {
        /// <summary>
        /// 页面code
        /// </summary>
        public string PageCode { get; set; }
    }

    public class ServiceInfo
    {
        /// <summary>
        /// 二维码图片地址
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 二维码图片地址
        /// </summary>
        public string ImgUrlShow 
        {
            get
            {
                if (string.IsNullOrEmpty(ImgUrl) || ImgUrl.ToUpper().StartsWith("HTTP"))
                {
                    return ImgUrl;
                }
                return string.Format("{0}/{1}",ConfigurationManager.AppSettings["imgurl"],ImgUrl);
            } 
        }

        /// <summary>
        /// 微信号
        /// </summary>
        public string WechatNumber { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
    }
}
