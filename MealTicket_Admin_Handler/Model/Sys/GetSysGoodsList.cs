using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSysGoodsList
    {
    }

    public class GetSysGoodsListRequest : PageRequest 
    {
        /// <summary>
        /// 商品类型1监控席位
        /// </summary>
        public int GoodsType { get; set; }
    }

    public class SysGoodsInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string GoodsName { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        public string GoodsDescription { get; set; }

        /// <summary>
        /// 图片Url
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 图片Url展示
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
        /// 商品类型1监控席位
        /// </summary>
        public int GoodsType { get; set; }

        /// <summary>
        /// 商品详情
        /// </summary>
        public string GoodsDetails { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
