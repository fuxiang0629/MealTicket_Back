using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using MealTicket_Admin_APIService.Filter;
using MealTicket_Admin_Handler;
using MealTicket_Admin_Handler.Model;
using Ninject;

namespace MealTicket_Admin_APIService.controller
{
    [RoutePrefix("menu")]
    public class MenuController:ApiController
    {
        private MenuHandler menuHandler;

        public MenuController()
        {
            menuHandler = WebApiManager.Kernel.Get<MenuHandler>();
        }

        /// <summary>
        /// 获取菜单列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("list"),HttpPost]
        [Description("获取菜单列表")]
        public List<MenuInfo> GetMenuList(GetMenuListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return menuHandler.GetMenuList(request, basedata);
        }
    }
}
