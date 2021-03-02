using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using MealTicket_Admin_Handler;
using MealTicket_Admin_Handler.Model;

namespace MealTicket_Admin_APIService.Filter
{
    /// <summary>
    /// 检查权限
    /// </summary>
    public class CheckUserPowerFilter: CheckUserLoginFilter
    {
        AuthorityHandler _authorityHandler;

        public CheckUserPowerFilter()
        {
            _authorityHandler = WebApiManager.Kernel.Get<AuthorityHandler>();
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var baseInfo = actionContext.ActionArguments["basedata"] as HeadBase;
            if (baseInfo.IsAdministrator)
            {
                return;
            }
            _authorityHandler.CheckUserPowerFilter(baseInfo);
            return;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            var baseInfo = actionExecutedContext.ActionContext.ActionArguments["basedata"] as HeadBase;
            //无异常表示请求成功，记录操作日志
            if (actionExecutedContext.Exception == null)
            {
                _authorityHandler.AddOperationLog(baseInfo.AccountId, baseInfo.LoginLogId, baseInfo.Controller + "." + baseInfo.Action, "");
            }
            return;
        }
    }
}
