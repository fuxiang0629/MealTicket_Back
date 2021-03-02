using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SMS_SendHandler
{
    /// <summary>
    /// 第三方短信平台对接接口
    /// </summary>
    public class ThirdSmsBase
    {
        /// <summary>
        /// 渠道code
        /// </summary>
        string ChannelCode;

        /// <summary>
        /// 应用key
        /// </summary>
        string AddKey;

        /// <summary>
        /// 应用秘钥
        /// </summary>
        string AppSecret;

        public ThirdSmsBase(string channelCode,string appKey, string appSecret)
        {
            this.ChannelCode = channelCode;
            this.AddKey = appKey;
            this.AppSecret = appSecret;
        }

        /// <summary>
        /// 获取第三方短信发送对象
        /// </summary>
        /// <returns></returns>
        public IThirdSms GetThirdSmsObj()
        {
            if (ChannelCode == "jiguang")
            {
                return new ThirdSms_JG(AddKey, AppSecret);
            }
            return null;
        }
    }
}
