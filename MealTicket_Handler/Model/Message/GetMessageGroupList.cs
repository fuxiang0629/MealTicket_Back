using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetMessageGroupList
    {
    }

    public class MessageGroupInfo
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 分组标题
        /// </summary>
        public string GroupTitle { get; set; }

        /// <summary>
        /// 图标相对地址
        /// </summary>
        [IgnoreDataMember]
        public string ImgUrl { get; set; }

        /// <summary>
        /// 图标网络地址
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
        /// 未读数量
        /// </summary>
        public int UnreadCount { get; set; }
    }
}
