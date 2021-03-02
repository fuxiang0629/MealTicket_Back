using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    class GetGoodsList
    {
    }

    public class GetGoodsListRequest
    {
        /// <summary>
        /// 商品类型1监控席位购买 2监控席位续费
        /// </summary>
        public int GoodsType { get; set; }
    }

    public class GoodsInfo
    {
        /// <summary>
        /// 商品名称
        /// </summary>
        public string GoodsName { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        public string GoodsDescription { get; set; }

        /// <summary>
        /// 商品详情
        /// </summary>
        public string GoodsDetails { get; set; }

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
                return string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], ImgUrl);
            }
        }
    }
}
