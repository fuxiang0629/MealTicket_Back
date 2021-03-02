using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetNoticeSmsSendRecord
    {
    }

    public class NoticeSmsSendRecordInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 短信渠道名称
        /// </summary>
        public string SmsChannelName { get; set; }

        /// <summary>
        /// 短信应用名称
        /// </summary>
        public string SmsAppName { get; set; }

        /// <summary>
        /// 发送内容
        /// </summary>
        public string SendContent { get; set; }

        /// <summary>
        /// 发送参数
        /// </summary>
        public string SmsTempPara { get; set; }

        /// <summary>
        /// 发送手机号
        /// </summary>
        public string SmsMobile { get; set; }

        /// <summary>
        /// 发送错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
