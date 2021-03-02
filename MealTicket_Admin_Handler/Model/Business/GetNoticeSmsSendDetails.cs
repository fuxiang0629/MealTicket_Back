using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetNoticeSmsSendDetails
    {
    }

    public class NoticeSmsSendDetailsInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 发送手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 第三方下发Id
        /// </summary>
        public string ThirdMsgId { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDes { get; set; }

        /// <summary>
        /// 送达时间
        /// </summary>
        public DateTime? ReceiveTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
    }
}
