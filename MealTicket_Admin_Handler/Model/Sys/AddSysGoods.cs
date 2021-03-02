using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSysGoods
    {
    }

    public class AddSysGoodsRequest
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
        /// 图片Url
        /// </summary>
        public string ImgUrl { get; set; }

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
    }
}
