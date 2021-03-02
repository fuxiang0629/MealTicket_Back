using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    /// <summary>
    /// 请求头部基础信息
    /// </summary>
    public class HeadBase
    {
        /// <summary>
        /// 用户ua
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 用户登入token
        /// </summary>
        public string UserToken { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// 市场Code
        /// </summary>
        public string MarketCode { get; set; }

        /// <summary>
        /// 用户访问controller
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// 用户访问action
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 访问方式
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get;set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNo { get; set; }
    }
}
