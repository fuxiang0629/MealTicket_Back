using MealTicket_Handler.BusinessHandler;
using MealTicket_Handler.Model;
using Ninject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MealTicket_APIService.controller
{
    [RoutePrefix("appversion")]
    public class AppVersionController:ApiController
    {
        private AppVersionHandler appVersionHandler;

        public AppVersionController()
        {
            appVersionHandler = WebApiManager.Kernel.Get<AppVersionHandler>();
        }


        /// <summary>
        /// 获取app版本信息
        /// </summary>
        /// <returns></returns>
        [Description("获取app版本信息")]
        [Route("info"), HttpPost]
        public AppVersionInfo GetAppVersionInfo()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return appVersionHandler.GetAppVersionInfo(basedata);
        }
    }
}
