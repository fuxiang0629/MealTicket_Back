using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class AccountGetBaseInfo
    {
    }

    public class QueryAccountBaseInfoRequest
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        public string AccountInfo { get; set; }
    }

    public class AccountBaseInfo
    {
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 头像相对地址
        /// </summary>
        public string HeadUrl { get; set; }

        /// <summary>
        /// 头像网络地址
        /// </summary>
        public string HeadUrlShow 
        {
            get 
            {
                if (string.IsNullOrEmpty(HeadUrl) || HeadUrl.ToUpper().StartsWith("HTTP"))
                {
                    return HeadUrl;
                }
                return string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"],HeadUrl);
            }
        }

        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public string BirthDay { get; set; }

        /// <summary>
        /// 推荐码
        /// </summary>
        public string RecommandCode { get; set; }

        /// <summary>
        /// 是否实名认证
        /// </summary>
        public bool IsRealName { get; set; }

        /// <summary>
        /// 注册日期
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
