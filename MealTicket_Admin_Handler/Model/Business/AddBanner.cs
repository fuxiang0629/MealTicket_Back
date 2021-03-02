using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddBanner
    {
    }

    public class AddBannerRequest
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// banner Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        public string ImgUrl { get; set; }

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
    }
}
