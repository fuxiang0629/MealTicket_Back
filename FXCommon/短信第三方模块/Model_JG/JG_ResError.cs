using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_SendHandler
{
    /// <summary>
    /// 极光短信返回错误结构
    /// </summary>
    public class JG_ResError
    {
        /// <summary>
        /// 错误类
        /// </summary>
        public JG_Error error { get; set; }
    }

    /// <summary>
    /// 错误信息
    /// </summary>
    public class JG_Error
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 错误描述
        /// </summary>
        public string message { get; set; }
    }
}
