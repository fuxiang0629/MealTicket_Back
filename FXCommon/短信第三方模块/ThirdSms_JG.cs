using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SMS_SendHandler
{
    public class ThirdSms_JG:IThirdSms
    {
        /// <summary>
        /// 对外暴露 HttpClient，可以增加或修改设置。
        /// </summary>
        public HttpClient httpClient;

        /// <summary>
        /// 初始化 Client 对象。
        /// </summary>
        /// <param name="appKey">应用的 App Key</param>
        /// <param name="masterSecret">应用的 Master Secret</param>
        public ThirdSms_JG(string appKey, string masterSecret)
        {
            if (string.IsNullOrEmpty(appKey))
                throw new ArgumentNullException("appKey");

            if (string.IsNullOrEmpty(masterSecret))
                throw new ArgumentNullException("masterSecret");

            httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.sms.jpush.cn/v1/")
            };
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(appKey + ":" + masterSecret));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        }

        private async Task<HttpResponse> CreateSmsTemplateAsync(TemplateMessage template)
        {
            HttpContent httpContent = new StringContent(template.ToString(), Encoding.UTF8);
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("templates", httpContent).ConfigureAwait(false);
            string httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new HttpResponse(httpResponseMessage.StatusCode, httpResponseMessage.Headers, httpResponseContent);
        }

        public HttpResponse CreateSmsTemplate(SmsTemplateInfo template)
        {
            Task<HttpResponse> task = Task.Run(() => CreateSmsTemplateAsync(new TemplateMessage 
            {
                Content=template.Content,
                Type=template.Type,
                Remark=template.Remark
            }));
            task.Wait();
            return task.Result;
        }

        private async Task<HttpResponse> UpdateSmsTemplateAsync(TemplateMessage template)
        {
            if (template.TemplateId == null)
                throw new ArgumentNullException("TemplateId");

            HttpContent httpContent = new StringContent(template.ToString(), Encoding.UTF8);
            HttpResponseMessage httpResponseMessage = await httpClient.PutAsync("templates/" + template.TemplateId, httpContent).ConfigureAwait(false);
            string httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new HttpResponse(httpResponseMessage.StatusCode, httpResponseMessage.Headers, httpResponseContent);
        }

        public HttpResponse UpdateSmsTemplate(SmsTemplateInfo template)
        {
            Task<HttpResponse> task = Task.Run(() => UpdateSmsTemplateAsync(new TemplateMessage
            {
                Content=template.Content,
                Remark=template.Remark,
                TemplateId=int.Parse(template.TemplateId),
                Type=template.Type
            }));
            task.Wait();
            return task.Result;
        }

        private async Task<HttpResponse> QuerySmsTemplateAsync(int tempId)
        {
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("templates/" + tempId).ConfigureAwait(false);
            string httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new HttpResponse(httpResponseMessage.StatusCode, httpResponseMessage.Headers, httpResponseContent);
        }

        public HttpResponse QuerySmsTemplate(string templateId)
        {
            Task<HttpResponse> task = Task.Run(() => QuerySmsTemplateAsync(int.Parse(templateId)));
            task.Wait();
            return task.Result;
        }

        private async Task<HttpResponse> DeleteSmsTempleteAsync(int tempId)
        {
            HttpResponseMessage httpResponseMessage = await httpClient.DeleteAsync("templates/" + tempId).ConfigureAwait(false);
            string httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new HttpResponse(httpResponseMessage.StatusCode, httpResponseMessage.Headers, httpResponseContent);
        }

        public HttpResponse DeleteSmsTemplete(string templateId)
        {
            Task<HttpResponse> task = Task.Run(() => DeleteSmsTempleteAsync(int.Parse(templateId)));
            task.Wait();
            return task.Result;
        }

        private async Task<HttpResponse> SendSmsAsync(TemplateMessage message)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            if (message.SignId == 0)
            {
                message.SignId = null;
            }

            HttpContent httpContent = new StringContent(message.ToString(), Encoding.UTF8);
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("messages", httpContent).ConfigureAwait(false);
            string httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new HttpResponse(httpResponseMessage.StatusCode, httpResponseMessage.Headers, httpResponseContent);
        }

        public HttpResponse SendSms(SmsSendInfo message)
        {
            Task<HttpResponse> task = Task.Run(() => SendSmsAsync(new TemplateMessage 
            {
                Mobile = message.Mobile,
                TemplateId = int.Parse(message.TemplateId),
                TemplateParameters = message.TemplateParameters,
                SignId =string.IsNullOrEmpty(message.SignId)?0:int.Parse(message.SignId)
            }));
            task.Wait();
            return task.Result;
        }

        private async Task<HttpResponse> SendSmsBatchAsync(List<TemplateMessage> messageList)
        {
            if (messageList == null || messageList.Count == 0)
                throw new ArgumentException("templateMessageList");

            int? tempId = messageList[0].TemplateId;
            int? signId = messageList[0].SignId==0?null:messageList[0].SignId;

            JArray recipients = new JArray();
            foreach (TemplateMessage msg in messageList)
            {
                JObject item = new JObject
                {
                    { "mobile", msg.Mobile }
                };

                if (msg.TemplateParameters != null && msg.TemplateParameters.Count != 0)
                {
                    item.Add("temp_para", JObject.FromObject(msg.TemplateParameters));
                }

                recipients.Add(item);
            }

            JObject json = new JObject
            {
                { "temp_id", tempId },
                { "sign_id", signId },
                { "recipients", recipients }
            };

            HttpContent httpContent = new StringContent(json.ToString(), Encoding.UTF8);
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("messages/batch", httpContent).ConfigureAwait(false);
            string httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new HttpResponse(httpResponseMessage.StatusCode, httpResponseMessage.Headers, httpResponseContent);
        }

        public HttpResponse SendSmsBatch(List<SmsSendInfo> messageList)
        {
            List<TemplateMessage> list = (from item in messageList
                                          select new TemplateMessage
                                          {
                                              Mobile = item.Mobile,
                                              SignId = string.IsNullOrEmpty(item.SignId) ? 0 : int.Parse(item.SignId),
                                              TemplateId = int.Parse(item.TemplateId),
                                              TemplateParameters = item.TemplateParameters
                                          }).ToList();
            Task<HttpResponse> task = Task.Run(() => SendSmsBatchAsync(list));
            task.Wait();
            return task.Result;
        }

        #region 签名接口
        /// <summary>
        /// 创建签名
        /// </summary>
        /// <param name="signModel"></param>
        /// <returns></returns>
        public async Task<HttpResponse> CreateSignAsync(SignModel signModel)
        {
            if (signModel == null || string.IsNullOrEmpty(signModel.Sign))
                throw new ArgumentNullException(nameof(signModel));

            using (var resp = await httpClient.PostAsync("sign", signModel.ToForm()))
            {
                string respStr = await resp.Content.ReadAsStringAsync();
                return new HttpResponse(resp.StatusCode, resp.Headers, respStr);
            }
        }

        /// <summary>
        /// 新增签名
        /// </summary>
        /// <param name="signModel"></param>
        /// <returns></returns>
        public HttpResponse CreateSmsSign(SignModel signModel)
        {
            Task<HttpResponse> task = Task.Run(() => CreateSignAsync(signModel));
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// 修改签名
        /// </summary>
        /// <param name="signId"></param>
        /// <param name="signModel"></param>
        /// <returns></returns>
        public async Task<HttpResponse> UpdateSignAsync(int signId, SignModel signModel)
        {
            if (signModel == null || string.IsNullOrEmpty(signModel.Sign))
                throw new ArgumentNullException(nameof(signModel));

            using (var resp = await httpClient.PostAsync($"sign/{signId}", signModel.ToForm()))
            {
                string respStr = await resp.Content.ReadAsStringAsync();
                return new HttpResponse(resp.StatusCode, resp.Headers, respStr);
            }
        }

        /// <summary>
        /// 修改签名
        /// </summary>
        /// <param name="signId"></param>
        /// <param name="signModel"></param>
        /// <returns></returns>
        public HttpResponse UpdateSmsSign(int signId, SignModel signModel)
        {
            Task<HttpResponse> task = Task.Run(() => UpdateSignAsync(signId, signModel));
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// 删除签名
        /// </summary>
        /// <param name="signId"></param>
        /// <returns></returns>
        private async Task<HttpResponse> DeleteSmsSignAsync(int signId)
        {
            HttpResponseMessage httpResponseMessage = await httpClient.DeleteAsync("sign/" + signId).ConfigureAwait(false);
            string httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new HttpResponse(httpResponseMessage.StatusCode, httpResponseMessage.Headers, httpResponseContent);
        }

        /// <summary>
        /// 删除签名
        /// </summary>
        /// <param name="signId"></param>
        /// <returns></returns>
        public HttpResponse DeleteSmsSign(string signId)
        {
            Task<HttpResponse> task = Task.Run(() => DeleteSmsSignAsync(int.Parse(signId)));
            task.Wait();
            return task.Result;
        }

        #endregion
    }
}
