using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetBannerList
    {
    }

    public class GetBannerListRequest
    {
        /// <summary>
        /// 广告位code
        /// </summary>
        public string BannerCode { get; set; }
    }

    public class BannerInfo
    {
        /// <summary>
        /// 广告位图片相对地址
        /// </summary>
        [IgnoreDataMember]
        public string ImgUrl { get; set; }

        /// <summary>
        /// 广告位图片地址
        /// </summary>
        public string ImgUrlShow 
        {
            get
            {
                if (string.IsNullOrEmpty(ImgUrl) || ImgUrl.ToUpper().StartsWith("HTTP"))
                {
                    return ImgUrl;
                }
                return string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], ImgUrl);
            }
        }

        /// <summary>
        /// 点击跳转类型 1跳转外部页面 2跳转本地页面
        /// </summary>
        public int ActionType { get; set; }

        /// <summary>
        /// 跳转地址
        /// </summary>
        public string ActionPath { get; set; }
    }
}
