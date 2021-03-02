using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using MealTicket_Admin_Handler;
using MealTicket_Admin_Handler.Model;
using Ninject;

namespace MealTicket_Admin_APIService.Filter
{
    /// <summary>
    /// 检查用户是否登入
    /// </summary>
    public class CheckUserLoginFilter: ActionFilterAttribute
    {
        AuthorityHandler _authorityHandler;

        public CheckUserLoginFilter()
        {
            _authorityHandler = WebApiManager.Kernel.Get<AuthorityHandler>();
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var baseInfo = actionContext.ActionArguments["basedata"] as HeadBase;
            _authorityHandler.CheckAccountLogin(baseInfo.UserToken, baseInfo);
            return;
        }
    }
}
