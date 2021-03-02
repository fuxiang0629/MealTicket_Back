using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_SendHandler
{
    public static class ThirdSms_JG_ErrorParse
    {
        public static string JGErrorParse(this string errorCode) 
        {
            string errorDes = "未知错误";
            switch (errorCode) 
            {
                case "50001":
                    errorDes = "auth 为空";
                    break;
                case "50002":
                    errorDes = "auth 鉴权失败";
                    break;
                case "50003":
                    errorDes = "body 为空";
                    break;
                case "50004":
                    errorDes = "手机号码为空";
                    break;
                case "50005":
                    errorDes = "模版ID 为空";
                    break;
                case "50006":
                    errorDes = "手机号码无效";
                    break;
                case "50007":
                    errorDes = "body 无效";
                    break;
                case "50008":
                    errorDes = "未开通短信业务";
                    break;
                case "50009":
                    errorDes = "发送超频";
                    break;
                case "50010":
                    errorDes = "验证码无效";
                    break;
                case "50011":
                    errorDes = "验证码过期";
                    break;
                case "50012":
                    errorDes = "验证码已验证";
                    break;
                case "50013":
                    errorDes = "模版ID 无效";
                    break;
                case "50014":
                    errorDes = "可发短信余量不足";
                    break;
                case "50015":
                    errorDes = "验证码为空";
                    break;
                case "50016":
                    errorDes = "API 不存在";
                    break;
                case "50017":
                    errorDes = "媒体类型不支持";
                    break;
                case "50018":
                    errorDes = "请求方法不支持";
                    break;
                case "50019":
                    errorDes = "服务端异常";
                    break;
                case "50020":
                    errorDes = "模板审核中";
                    break;
                case "50021":
                    errorDes = "模板审核未通过";
                    break;
                case "50022":
                    errorDes = "模板中参数未全部替换";
                    break;
                case "50023":
                    errorDes = "参数为空";
                    break;
                case "50024":
                    errorDes = "手机号码已退订";
                    break;
                case "50025":
                    errorDes = "该 API 不支持此模版类型";
                    break;
                case "50026":
                    errorDes = "msg_id 无效";
                    break;
                case "50027":
                    errorDes = "发送时间为空或在当前时间之前";
                    break;
                case "50028":
                    errorDes = "schedule_id 无效";
                    break;
                case "50029":
                    errorDes = "定时短信已发送或已删除，无法再修改";
                    break;
                case "50030":
                    errorDes = "recipients 为空";
                    break;
                case "50031":
                    errorDes = "recipients 短信接收者数量超过1000";
                    break;
                case "50034":
                    errorDes = "重复发送";
                    break;
                case "50035":
                    errorDes = "请求 IP 不合法";
                    break;
                case "50036":
                    errorDes = "应用被列为黑名单";
                    break;
                case "50037":
                    errorDes = "短信内容存在敏感词汇";
                    break;
                case "50038":
                    errorDes = "短信内容长度错误，文本短信长度不超过350个字，语音短信验证码长度4～8数字";
                    break;
                case "50039":
                    errorDes = "语音验证码内容错误，验证码仅支持数字";
                    break;
                case "50040":
                    errorDes = "语音验证码播报语言类型错误";
                    break;
                case "50041":
                    errorDes = "验证码有效期错误";
                    break;
                case "50042":
                    errorDes = "模板内容为空";
                    break;
                case "50043":
                    errorDes = "模板内容过长，含签名长度限制为350字符";
                    break;
                case "50044":
                    errorDes = "模板参数无效";
                    break;
                case "50045":
                    errorDes = "备注内容过长，长度限制为500字符";
                    break;
                case "50046":
                    errorDes = "该应用未设置签名，请先设置签名";
                    break;
                case "50047":
                    errorDes = "该模版不支持修改，仅状态为审核不通过的模板支持修改";
                    break;
                case "50052":
                    errorDes = "模板不能含有特殊符号";
                    break;
                case "50053":
                    errorDes = "模板中存在链接变量，请在 remark 参数中填写链接以报备，避免短信发送时因进入人工审核而导致发送延迟";
                    break;
                case "50054":
                    errorDes = "短信正文不能含有特殊符号";
                    break;
                case "50101":
                    errorDes = "图片不合法";
                    break;
                case "50102":
                    errorDes = "签名 ID 不合法";
                    break;
                case "50103":
                    errorDes = "已经存在其他待审核的签名，不能提交";
                    break;
                case "50104":
                    errorDes = "签名内容不合法";
                    break;
                case "50105":
                    errorDes = "默认签名不能删除";
                    break;
                case "50201":
                    errorDes = "超频，API 调用频率：单个 appKey 5 秒/次";
                    break;
                case "50202":
                    errorDes = "禁止拉取";
                    break;
                case "50301":
                    errorDes = "短信开发者账号冻结，请联系技术支持";
                    break;
                default:
                    break;
            }
            return errorDes;
        }
    }
}
