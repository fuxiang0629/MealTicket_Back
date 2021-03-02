using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_SendHandler
{
    /// <summary>
    /// 极光模板接口返回结果
    /// </summary>
    public class JG_TemlateRes
    {
        /// <summary>
        /// 极光模板id
        /// </summary>
        public int temp_id { get; set; }
    }

    public class JG_SmsSendRes
    {
        public long msg_id { get; set; }
    }

    public class JG_SignRes
    {
        public int sign_id { get; set; }
    }

    public class JG_SmsBatchSendRes
    {
        public int success_count { get; set; }

        public int failure_count { get;set;}

        public List<JG_SmsBatchSendResData> recipients { get; set; }
    }

    public class JG_SmsBatchSendResData
    {
        public string error_code { get; set; }

        public string error_message { get; set; }

        public string msg_id { get; set; }

        public string mobile { get; set; }
    }
}
