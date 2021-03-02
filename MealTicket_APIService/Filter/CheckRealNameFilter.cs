using MealTicket_Handler;
using MealTicket_Handler.Model;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace MealTicket_APIService.Filter
{
    public class CheckRealNameFilter : CheckUserLoginFilter
    {
        AuthorityHandler _authorityHandler;

        public CheckRealNameFilter()
        {
            _authorityHandler = WebApiManager.Kernel.Get<AuthorityHandler>();
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var baseInfo = actionContext.ActionArguments["basedata"] as HeadBase;
            _authorityHandler.CheckRealName(baseInfo.AccountId);
            return;
        }
    }
}
