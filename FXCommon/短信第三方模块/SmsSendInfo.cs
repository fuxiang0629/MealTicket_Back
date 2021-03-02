using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_SendHandler
{
    public class SmsSendInfo
    {
        /// <summary>
        /// 签名Id
        /// </summary>
        public string SignId { get; set; }

        /// <summary>
        /// 模板Id
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 1 为验证码类，2 为通知类，3 为营销类。
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 模板参数，需要替换的键值对。
        /// </summary>
        public Dictionary<string, string> TemplateParameters { get; set; }
    }
}
