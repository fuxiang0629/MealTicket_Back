using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetNoticeList
    {
    }

    public class NoticeInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Decription { get; set; }

        /// <summary>
        /// 发送类型1.短信 2.推送消息（1+2表示都发）
        /// </summary>
        public int SendType { get; set; }

        /// <summary>
        /// 短信发送渠道code
        /// </summary>
        public string SmsChannelCode { get; set; }

        /// <summary>
        /// 短信发送渠道名称
        /// </summary>
        public string SmsChannelName { get; set; }

        /// <summary>
        /// 短信发送应用Key
        /// </summary>
        public string SmsAppKey { get; set; }

        /// <summary>
        /// 短信发送应用名称
        /// </summary>
        public string SmsAppName { get; set; }

        /// <summary>
        /// 短信模板Id
        /// </summary>
        public long SmsTemplateId { get; set; }

        /// <summary>
        /// 短信模板名称
        /// </summary>
        public string SmsTemplateName { get; set; }

        /// <summary>
        /// 短信签名Id
        /// </summary>
        public long SmsSignId { get; set; }

        /// <summary>
        /// 短信签名名称
        /// </summary>
        public string SmsSignName { get; set; }

        /// <summary>
        /// 触发用户
        /// </summary>
        public string SmsTriggerAccount { get; set; }

        /// <summary>
        /// 目标1用户 2指定号码（1+2表示都支持）
        /// </summary>
        public int SmsTarget { get; set; }

        /// <summary>
        /// 指定号码池
        /// </summary>
        public string SmsOtherMobile { get; set; }

        /// <summary>
        /// 推送目标分组
        /// </summary>
        public string PushTargetTag { get; set; }

        /// <summary>
        /// 发送时间类型1api触发发送 2定时发送
        /// </summary>
        public int TimeType { get; set; }

        /// <summary>
        /// 触发apiurl
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// 触发api描述
        /// </summary>
        public string ApiDes { get; set; }

        /// <summary>
        /// 定时发送规则
        /// </summary>
        public string SendTimeRules { get; set; }

        /// <summary>
        /// 发送超时秒数
        /// </summary>
        public long SendOverSecond { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
