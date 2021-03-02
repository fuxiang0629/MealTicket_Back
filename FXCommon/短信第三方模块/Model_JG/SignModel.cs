using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace SMS_SendHandler
{
    /// <summary>
    /// 签名
    /// </summary>
    public class SignModel
    {
        /// <summary>
        /// 签名内容
        /// </summary>
        public string Sign { get; set; }
        /// <summary>
        /// 签名类型，填写数字代号即可
        /// </summary>
        public int? Type { get; set; }
        /// <summary>
        /// 请提供签名相关的资质证件图片
        /// </summary>
        public List<Stream> Image0 { get; set; }
        /// <summary>
        /// 请简略描述您的业务使用场景，不超过100个字
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 请简略描述您的业务使用场景，不超过100个字
        /// </summary>
        public MultipartFormDataContent ToForm()
        {
            MultipartFormDataContent content = new MultipartFormDataContent
            {
                { new StringContent(this.Sign), "sign" }
            };
            if (this.Image0 != null)
            {
                int i = 0;
                foreach (var item in this.Image0)
                {
                    content.Add(new StreamContent(item), "image"+i, "image"+i+".png");
                    i++;
                }
            }
            if (this.Type != null)
            {
                content.Add(new StringContent(Convert.ToString(this.Type)), "type");
            }
            if (!string.IsNullOrEmpty(this.Remark))
            {
                if (this.Remark?.Length > 100)
                {
                    throw new NotSupportedException("签名备注不能超过100个字");
                }
                content.Add(new StringContent(this.Remark), "remark");
            }
            return content;
        }
    }
}
