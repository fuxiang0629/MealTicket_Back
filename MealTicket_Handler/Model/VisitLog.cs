using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    /// <summary>
    /// 访问日志
    /// </summary>
    public class VisitLog
    {
        /// <summary>
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 调用方法
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// 访问来源ip
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 访问结果code
        /// </summary>
        public int ResultCode { get; set; }

        /// <summary>
        /// 访问结果描述
        /// </summary>
        public string ResultMessage { get; set; }

        /// <summary>
        /// 访问时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 访问url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 访问方式
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        public string Useragent { get; set; }

        /// <summary>
        /// 访问内容序列化
        /// </summary>
        public string Request { get; set; }

        /// <summary>
        /// app版本号
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// 用户token
        /// </summary>
        public string UserToken { get; set; }
    }
}
