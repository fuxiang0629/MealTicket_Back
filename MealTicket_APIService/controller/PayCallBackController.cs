using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using FXCommon.Common;
using MealTicket_Handler.BusinessHandler;
using MealTicket_Handler.Model.PayCallBack;
using Newtonsoft.Json;
using Ninject;
using WxPayAPI;

namespace MealTicket_APIService.controller
{
    [RoutePrefix("pay/callback")]
    public class PayCallBackController:ApiController
    {
        private PayCallBackHandler payCallBackHandler;

        public PayCallBackController()
        {
            payCallBackHandler = WebApiManager.Kernel.Get<PayCallBackHandler>();
        }

        /// <summary>
        /// 支付宝支付结果回调
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        [Route("zfb"), HttpPost]
        [Description("支付宝支付结果回调")]
        public HttpResponseMessage ZFBPayResultCallback(ZFBPayCallbackInfo callbackInfo)
        {
            string result = "success";
            bool isSuccess=payCallBackHandler.ZFBPayResultCallback(callbackInfo);
            if (isSuccess)
            {
                result= "success";
            }
            else
            {
                result = "fail";
            }
            return new HttpResponseMessage
            {
                Content = new StringContent(result)
            };
        }

        /// <summary>
        /// 微信支付结果回调
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        [Route("wx"), HttpPost]
        [Description("微信支付结果回调")]
        public HttpResponseMessage WXPayResultCallback()
        {
            //接收从微信后台POST过来的数据
            System.IO.Stream s = this.Request.Content.ReadAsStreamAsync().Result;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Close();
            s.Dispose();
            WxPayData callbackInfo = new WxPayData();
            callbackInfo.FromXml(builder.ToString(), "");

            WxPayData wxModel = new WxPayData();
            string result = string.Empty;
            if (callbackInfo == null)
            {
                wxModel.SetValue("return_code", "FAIL");
                wxModel.SetValue("return_msg", "微信回调内容解析为空");
                result = wxModel.ToXml();
            }
            else
            {
                bool isSuccess = payCallBackHandler.WXPayResultCallback(callbackInfo);
                if (isSuccess)
                {
                    wxModel.SetValue("return_code", "SUCCESS");
                    wxModel.SetValue("return_msg", "OK");
                    result = wxModel.ToXml();
                }
                else
                {
                    wxModel.SetValue("return_code", "FAIL");
                    wxModel.SetValue("return_msg", "");
                    result = wxModel.ToXml();
                }
            }
            return new HttpResponseMessage
            {
                Content = new StringContent(result)
            };
        }
    }
}
