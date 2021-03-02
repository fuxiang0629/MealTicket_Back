using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Net.Http;
using FXCommon.Common;
using MealTicket_Admin_Handler.Model;

namespace MealTicket_Admin_APIService.Filter
{
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            BaseResponse resResult = new BaseResponse();
            Exception ex = actionExecutedContext.Exception;
            if (ex is AggregateException)
            {
                //若为多个错误，获取内部错误
                ex = ex.InnerException;
            }
            var exception = ex as WebApiException;
            if (exception != null)
            {
                resResult.ErrorCode = exception.errorCode;
                resResult.ErrorMessage = exception.Message;
            }
            else
            {
                resResult.ErrorMessage = "服务器内部错误";
                resResult.ErrorCode = 500;
            }
            //记录错误日志
            Logger.WriteFileLog("意外出错", ex);
            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(resResult);
        }
    }
}
