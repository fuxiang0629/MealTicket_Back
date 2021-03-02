using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_SendHandler
{
    /// <summary>
    /// 短信签名信息
    /// </summary>
    public class ThirdSmsSignInfo
    {
        /// <summary>
        /// 签名Id
        /// </summary>
        public string SignId { get; set; }

        /// <summary>
        /// 单个文件
        /// </summary>
        public Stream Image { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string SignName { get; set; }
    }
}
