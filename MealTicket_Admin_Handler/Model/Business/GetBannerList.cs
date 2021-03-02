using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBannerList
    {
    }

    public class GetBannerListRequest : PageRequest
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 广告名称
        /// </summary>
        public string BannerName { get; set; }
    }

    public class BannerInfo
    {
        /// <summary>
        /// banner Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// banner Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 图片地址(网络地址)
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
        /// 点击跳转类型 0无跳转 1跳转外部页面 2跳转本地页面
        /// </summary>
        public int ActionType { get; set; }

        /// <summary>
        /// 跳转地址
        /// </summary>
        public string ActionPath { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

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
