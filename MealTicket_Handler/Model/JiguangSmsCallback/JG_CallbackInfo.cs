using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    /// <summary>
    /// 极光短信回调内容
    /// </summary>
    public class JG_CallbackInfo
    {
        /// <summary>
        /// 随机长整数
        /// </summary>
        public long nonce { get; set; }

        /// <summary>
        /// 签名，结合 appKey、appMasterSecret、nonce、timestamp 生成
        /// </summary>
        public string signature { get; set; }

        /// <summary>
        /// 当前时间戳，毫秒值
        /// </summary>
        public long timestamp { get; set; }

        /// <summary>
        /// 通知类型（SMS_REPORT/SMS_REPLY）
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 通知内容，json 字符串，开发者可以根据 type 反序列化 data
        /// </summary>
        public string data { get; set; }
    }

    /// <summary>
    /// 极光签名审核结果回调内容
    /// </summary>
    public class JG_SignExamineCallbackInfo
    {
        public int signId { get; set; }

        /// <summary>
        /// 签名状态，1代表审核通过，2代表审核不通过
        /// </summary>
        public int status { get; set; }

        public string refuseReason { get; set; }
    }

    /// <summary>
    /// 极光模板审核结果回调内容
    /// </summary>
    public class JG_TemplateExamineCallbackInfo
    {
        /// <summary>
        /// 第三方模板id
        /// </summary>
        public long tempId { get; set; }

        /// <summary>
        /// 模板状态，1代表审核通过，2代表审核不通过
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 审核不通过的原因
        /// </summary>
        public string refuseReason { get; set; }
    }

    /// <summary>
    /// 极光短信下行回调内容
    /// </summary>
    public class JG_SmsSendDownCallbackInfo
    {
        /// <summary>
        /// API 调用的时候返回的 msg_id
        /// </summary>
        public string msgId { get; set; }

        /// <summary>
        /// 发送状态返回码4001发送成功 
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public long receiveTime { get; set; }

        /// <summary>
        /// 短信送达手机号
        /// </summary>
        public string phone { get; set; }
    }

    /// <summary>
    /// 极光下行短信回调状态
    /// </summary>
    public class JG_SmsSendDownCallbackStatus
    {
        /// <summary>
        /// 状态2.失败 3.成功 
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        public string ErrorReason { get; set; }
    }

    /// <summary>
    /// 应用信息
    /// </summary>
    public class SmsAppInfo
    {
        public string AppKey { get; set; }

        public string AppSecret { get; set; }
    }
}
