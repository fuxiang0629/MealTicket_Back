using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetMessageList
    {
    }

    public class MessageInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string ImgUrlShow 
        {
            get 
            {
                if (string.IsNullOrEmpty(ImgUrl) || ImgUrl.ToUpper().StartsWith("HTTP"))
                {
                    return ImgUrl;
                }
                return string.Format("{0}/{1}",ConfigurationManager.AppSettings["imgurl"], ImgUrl);
            }
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
