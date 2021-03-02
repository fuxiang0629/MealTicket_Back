using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;
using MealTicket_Handler;
using MealTicket_Handler.Model;
using FXCommon.Common;
using Ninject;

namespace MealTicket_APIService.Filter
{
    /// <summary>
    /// 授权认证
    /// </summary>
    public class AuthorizationFilter : AuthorizationFilterAttribute
    {
        AuthorityHandler _authorityHandler;

        public AuthorizationFilter()
        {
            _authorityHandler = WebApiManager.Kernel.Get<AuthorityHandler>();
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            HeadBase baseInfo=null;
            try
            {
                dynamic httpcontext = actionContext.Request.GetRequest();
                if (httpcontext == null)
                {
                    throw new Exception("httpcontext为空错误");
                }
                //设备编号
                string deviceNo = httpcontext.Request.Headers["deviceNo"] ?? string.Empty;
                //用户当前版本号
                string appVersion = httpcontext.Request.Headers["appVersion"] ?? string.Empty;
                //市场Code
                string marketCode = httpcontext.Request.Headers["marketCode"] ?? string.Empty;
                //用户访问ua
                string userAgent = actionContext.Request.Headers.UserAgent.ToString();
                //用户登入token
                string userToken = httpcontext.Request.Headers["userToken"] ?? string.Empty;
                //controller
                string controller = actionContext.ControllerContext.ControllerDescriptor.ControllerName;
                //action
                string action = actionContext.ActionDescriptor.ActionName;
                //IP地址
                string ip;
                ip = httpcontext.Request.Headers["X-Real-IP"]??string.Empty;
                if(string.IsNullOrEmpty(ip))
                {
                    ip = httpcontext.Request.Headers["x-forwarded-for"] ?? string.Empty;
                }
                if (string.IsNullOrEmpty(ip))
                {
                    ip = httpcontext.Request.Headers["Proxy-Client-IP"] ?? string.Empty;
                }
                if (string.IsNullOrEmpty(ip))
                {
                    ip = httpcontext.Request.Headers["WL-Proxy-Client-IP"] ?? string.Empty;
                }
                if (string.IsNullOrEmpty(ip))
                {
                    ip = actionContext.Request.GetClientIpAddress();
                }
                //访问方式
                string method = actionContext.Request.Method.Method;
                baseInfo = new HeadBase
                {
                    AppVersion = appVersion,
                    UserAgent = userAgent,
                    UserToken = userToken,
                    Controller = controller,
                    Action = action,
                    Ip = ip,
                    DeviceNo= deviceNo,
                    Method = method,
                    MarketCode= marketCode
                };
                //授权验证
                //_commonHandler.ChechAuthority(baseInfo);

                actionContext.ActionArguments.Add("basedata", baseInfo);
            }
            catch (AuthorizationException ex)
            {
                _authorityHandler.AddVisitLog(new VisitLog
                {
                    Function = baseInfo.Controller + "." + baseInfo.Action,
                    Ip = baseInfo.Ip,
                    Method = baseInfo.Method,
                    Request = actionContext.Request.Content.Headers.ContentLength > 2048 ?
                                string.Empty : actionContext.Request.Content.ReadAsStringAsync().Result,
                    ResultCode = ex.errorCode,
                    ResultMessage = ex.Message,
                    Time = DateTime.Now,
                    Url = actionContext.Request.RequestUri.ToString(),
                    Useragent = baseInfo.UserAgent,
                    AppVersion = baseInfo.AppVersion,
                    UserToken = baseInfo.UserToken
                });
                actionContext.Response = actionContext.Request.CreateResponse(new BaseResponse
                {
                    ErrorCode = ex.errorCode,
                    ErrorMessage = ex.Message
                });
                Logger.WriteFileLog("未授权访问", ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
