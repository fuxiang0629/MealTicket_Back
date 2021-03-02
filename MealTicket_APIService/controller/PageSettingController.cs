using MealTicket_Handler;
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
    [RoutePrefix("pagesetting")]
    public class PageSettingController:ApiController
    {
        private PageSettingHandler pageSettingHandler;

        public PageSettingController()
        {
            pageSettingHandler = WebApiManager.Kernel.Get<PageSettingHandler>();
        }

        /// <summary>
        /// 查询页面配置信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("info")]
        [Description("查询页面配置信息"),HttpPost]
        public object GetPageSettingInfo(GetPageSettingInfoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            return pageSettingHandler.GetPageSettingInfo(request);
        }
    }
}
