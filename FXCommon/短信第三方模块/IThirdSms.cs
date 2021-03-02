using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SMS_SendHandler
{
    public interface IThirdSms
    {
        /// <summary>
        /// 创建短信模板
        /// </summary>
        /// <returns></returns>
        HttpResponse CreateSmsTemplate(SmsTemplateInfo template);

        /// <summary>
        /// 更新短信模板
        /// </summary>
        /// <returns></returns>
        HttpResponse UpdateSmsTemplate(SmsTemplateInfo template);

        /// <summary>
        /// 根据模板Id查询短信模板
        /// </summary>
        /// <returns></returns>
        HttpResponse QuerySmsTemplate(string templateId);

        /// <summary>
        /// 删除短信模板
        /// </summary>
        /// <returns></returns>
        HttpResponse DeleteSmsTemplete(string templateId);

        /// <summary>
        /// 单条发送短信
        /// </summary>
        /// <returns></returns>
        HttpResponse SendSms(SmsSendInfo message);

        /// <summary>
        /// 批量发送短信
        /// </summary>
        /// <returns></returns>
        HttpResponse SendSmsBatch(List<SmsSendInfo> messageList);

        /// <summary>
        /// 创建短信签名
        /// </summary>
        /// <returns></returns>
        HttpResponse CreateSmsSign(SignModel signModel);

        /// <summary>
        /// 修改短信签名
        /// </summary>
        /// <returns></returns>
        HttpResponse UpdateSmsSign(int signId, SignModel signModel);

        /// <summary>
        /// 删除短信签名
        /// </summary>
        /// <returns></returns>
        HttpResponse DeleteSmsSign(string signId);
    }
}
