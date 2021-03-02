using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.IO;
using System.Web;
using MealTicket_Handler;
using Ninject;
using MealTicket_Handler.Model;
using FXCommon.Common;
using NoticeHandler;

namespace MealTicket_APIService.Filter
{
    /// <summary>
    /// 访问日志筛选器，用于记录访问日志.通过注入IVisitLogger实现访问日志的记录
    /// </summary>
    public class LogActionFilter : ActionFilterAttribute
    {
        AuthorityHandler _authorityHandler;

        /// <summary>
        /// 访问日志筛选器，用于记录访问日志.通过注入IVisitLogger实现访问日志的记录
        /// </summary>
        /// <param name="commonHandler">日志记录接口</param>
        public LogActionFilter()
        {
            _authorityHandler = WebApiManager.Kernel.Get<AuthorityHandler>();
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            try
            {
                int resultCode = 0;
                string resultMessage = string.Empty;
                var baseInfo = context.ActionContext.ActionArguments["basedata"] as HeadBase;

                string request = string.Empty;
                try
                {
                    request = JsonConvert.SerializeObject(context.ActionContext.ActionArguments["request"]);
                }
                catch (Exception ex)
                { }

                BaseResponse resResult = new BaseResponse();
                if (context.Exception != null)
                {
                    Exception ex = context.Exception;
                    if (ex is AggregateException)
                    {
                        //若为多个错误，获取内部错误
                        ex = ex.InnerException;
                    }
                    if (ex is HttpResponseException)
                    {
                        //若是HttpResponseException则表示为系统抛出的异常
                        var temp = ex as HttpResponseException;
                        resultCode = 500;
                        resultMessage = temp.Message;
                    }
                    else
                    {
                        var exception = ex as WebApiException;
                        if (exception == null)
                        {
                            resultCode = 500;
                            resultMessage = "服务器内部错误";
                        }
                        else
                        {
                            resultCode = exception.errorCode;
                            resultMessage = exception.Message;
                        }
                    }

                    resResult.Data = null;
                    //记录错误日志
                    Logger.WriteFileLog(resultMessage, ex);
                    resResult.ErrorMessage = resultMessage;
                    resResult.ErrorCode = resultCode;
                    context.Response = context.Request.CreateResponse(resResult);
                }
                else
                {
                    if (context.Response.Content.Headers.ContentType.MediaType == "text/html" || context.Response.Content.Headers.ContentType.MediaType == "image/jpg")
                    {

                    }
                    else
                    {
                        resResult.Data = context.Response.Content.ReadAsAsync<object>().Result;
                        resResult.ErrorMessage = resultMessage;
                        resResult.ErrorCode = resultCode;

                        var requestObj=JsonConvert.DeserializeObject<dynamic>(request);
                        //进入通知发送判断
                        string tempPara = null;
                        string apiUrl = baseInfo.Controller + "." + baseInfo.Action;
                        if (apiUrl == "Account.AccountModifyTransactionPassword")//修改交易密码
                        {
                            tempPara = JsonConvert.SerializeObject(new
                            {
                                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                        if (apiUrl == "Account.CashApply")//申请提现
                        {
                            long amount = requestObj.Amount;
                            tempPara = JsonConvert.SerializeObject(new
                            {
                                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                amount= Math.Round(amount*1.0/10000,2)
                            });
                        }
                        NoticeSender.SendExecute(apiUrl, baseInfo.AccountId, tempPara);

                        context.Response = context.Request.CreateResponse(resResult);
                    }
                }
                VisitLog log = new VisitLog
                {
                    Ip = baseInfo.Ip,
                    Function = baseInfo.Controller
                                + "." + baseInfo.Action,
                    Time = DateTime.Now,
                    Useragent = baseInfo.UserAgent,
                    Url = context.Request.RequestUri.ToString(),
                    Request = request,
                    Method = baseInfo.Method,
                    ResultCode = resultCode,
                    ResultMessage = resultMessage,
                    AppVersion = baseInfo.AppVersion,
                    UserToken = baseInfo.UserToken
                };
                visitLogDel del = new visitLogDel(AddVisitLog);
                IAsyncResult ar = del.BeginInvoke(log, null, del);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private delegate void visitLogDel(VisitLog log);

        private void AddVisitLog(VisitLog log)
        {
            try
            {
                _authorityHandler.AddVisitLog(log);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("记录访问日志失败", ex);
            }
        }
    }
}
