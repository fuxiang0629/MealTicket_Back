using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    /// <summary>
    /// 图片上传返回信息
    /// </summary>
    public class ImagePath
    {
        /// <summary>
        /// 图片展示路径
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 图片存储路径
        /// </summary>
        public string ImgPath { get; set; }

        /// <summary>
        /// 图片名称
        /// </summary>
        public string ImgName { get; set; }
    }
}
