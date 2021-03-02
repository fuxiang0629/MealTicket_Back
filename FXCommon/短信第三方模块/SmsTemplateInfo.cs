using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_SendHandler
{
    /// <summary>
    /// 短信模板信息
    /// </summary>
    public class SmsTemplateInfo
    {
        /// <summary>
        /// 模板Id
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// 模板内容 根据运营商规定下发短信的内容不能超过 350 字符。
        /// 变量替换{{name}}
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 模板类型。1 为验证码类，2 为通知类，3 为营销类。
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }
    }
}
