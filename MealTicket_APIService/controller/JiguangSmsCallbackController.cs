using FXCommon.Common;
using MealTicket_Handler.BusinessHandler;
using MealTicket_Handler.Model;
using Newtonsoft.Json;
using Ninject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace MealTicket_APIService.controller
{
    [RoutePrefix("jiguang/callback")]
    public class JiguangSmsCallbackController : ApiController
    {
        private SmsCallbackHandler smsCallbackHandler;

        public JiguangSmsCallbackController()
        {
            smsCallbackHandler = WebApiManager.Kernel.Get<SmsCallbackHandler>();
        }

        /// <summary>
        /// 短信签名审核结果回调
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        [Route("sign/examine/{appkey}")]
        [HttpPost, HttpGet]
        [Description("短信签名审核结果回调")]
        public HttpResponseMessage SignExamineCallback(string appkey, JG_CallbackInfo callbackInfo, string echostr = "")
        {
            if (!string.IsNullOrEmpty(echostr))
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent(echostr)
                };
            }
            if (callbackInfo == null)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            try
            {
                callbackInfo.data = callbackInfo.data.Replace("\\", "");
                string callbackInfoJson = JsonConvert.SerializeObject(callbackInfo);

                smsCallbackHandler.SaveCallbackInfo("jiguang", appkey, 1, callbackInfoJson);

                //解析通知内容
                var data = JsonConvert.DeserializeObject<JG_SignExamineCallbackInfo>(callbackInfo.data);
                //应用信息
                var appInfo = smsCallbackHandler.GetAppInfo("jiguang", appkey);
                if (appInfo == null)
                {
                    throw new Exception("应用不存在");
                }

                string signatureStr = string.Format("appKey={0}&appMasterSecret={1}&nonce={2}&timestamp={3}", appInfo.AppKey, appInfo.AppSecret, callbackInfo.nonce, callbackInfo.timestamp);
                if (signatureStr.Sha1() == callbackInfo.signature.ToUpper())//验签通过
                {
                    bool success = smsCallbackHandler.SignExamineCallback("jiguang", appkey, data.signId.ToString(), data.status, data.refuseReason);
                    if (success)
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        throw new Exception("执行业务出错");
                    }
                }
                else
                {
                    throw new Exception("验签不通过");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("极光签名回调出错", ex);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
        }

        /// <summary>
        /// 短信模板审核结果回调
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        [Route("template/examine/{appkey}")]
        [HttpPost, HttpGet]
        [Description("短信模板审核结果回调")]
        public HttpResponseMessage TemplateExamineCallback(string appkey, JG_CallbackInfo callbackInfo, string echostr = "")
        {
            if (!string.IsNullOrEmpty(echostr))
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent(echostr)
                };
            }
            if (callbackInfo == null)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                callbackInfo.data = callbackInfo.data.Replace("\\", "");
                string callbackInfoJson = JsonConvert.SerializeObject(callbackInfo);

                smsCallbackHandler.SaveCallbackInfo("jiguang", appkey, 2, callbackInfoJson);


                //解析通知内容
                var data = JsonConvert.DeserializeObject<JG_TemplateExamineCallbackInfo>(callbackInfo.data);
                //应用key
                var appInfo = smsCallbackHandler.GetAppInfo("jiguang", appkey);

                string signatureStr = string.Format("appKey={0}&appMasterSecret={1}&nonce={2}&timestamp={3}", appInfo.AppKey, appInfo.AppSecret, callbackInfo.nonce, callbackInfo.timestamp);

                if (signatureStr.Sha1() == callbackInfo.signature.ToUpper())//验签通过
                {
                    bool success = smsCallbackHandler.TemplateExamineCallback("jiguang", appkey, data.tempId.ToString(), data.status, data.refuseReason);
                    if (success)
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        throw new Exception("业务处理出错");
                    }
                }
                else
                {
                    throw new Exception("验签出错");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("极光模板审核回调出错", ex);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
        }

        /// <summary>
        /// 短信下行结果回调
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        [Route("smssend/down/{appkey}")]
        [HttpPost, HttpGet]
        [Description("短信下行结果回调")]
        public HttpResponseMessage SmsSendDownCallback(string appkey, JG_CallbackInfo callbackInfo, string echostr = "")
        {
            if (!string.IsNullOrEmpty(echostr))
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent(echostr)
                };
            }
            if (callbackInfo == null)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                callbackInfo.data = callbackInfo.data.Replace("\\", "");
                string callbackInfoJson = JsonConvert.SerializeObject(callbackInfo);

                smsCallbackHandler.SaveCallbackInfo("jiguang", appkey, 3, callbackInfoJson);

                //解析通知内容
                var data = JsonConvert.DeserializeObject<JG_SmsSendDownCallbackInfo>(callbackInfo.data);
                //应用key
                var appInfo = smsCallbackHandler.GetAppInfo("jiguang", appkey);

                string signatureStr = string.Format("appKey={0}&appMasterSecret={1}&nonce={2}&timestamp={3}", appInfo.AppKey, appInfo.AppSecret, callbackInfo.nonce, callbackInfo.timestamp);

                if (signatureStr.Sha1() == callbackInfo.signature.ToUpper())//验签通过
                {
                    bool success = smsCallbackHandler.SmsSendDownCallback("jiguang", appkey, data.msgId.ToString(), data.status, data.receiveTime, data.phone);
                    if (success)
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        throw new Exception("业务处理出错");
                    }
                }
                else
                {
                    throw new Exception("验签出错");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("极光短信下发回调出错", ex);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
        }
    }
}
