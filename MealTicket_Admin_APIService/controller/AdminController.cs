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
    [RoutePrefix("admin")]
    public class AdminController:ApiController
    {
        private AdminHandler adminHandler;

        public AdminController()
        {
            adminHandler = WebApiManager.Kernel.Get<AdminHandler>();
        }

        /// <summary>
        /// 后台用户登录
        /// </summary>
        /// <returns></returns>
        [Route("login"),HttpPost]
        [Description("后台用户登录")]
        public AdminLoginInfo AdminLogin(AdminLoginRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return adminHandler.AdminLogin(request, basedata);
        }

        /// <summary>
        /// 查询后台用户基本信息
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("baseinfo"),HttpPost]
        [Description("查询后台用户基本信息")]
        public AdminBaseInfo GetAdminBaseInfo()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return adminHandler.GetAdminBaseInfo(basedata);
        }

        /// <summary>
        /// 修改用户基本信息
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("baseinfo/modify"), HttpPost]
        [Description("修改用户基本信息")]
        public object ModifyAdminBaseInfo(ModifyAdminBaseInfoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            adminHandler.ModifyAdminBaseInfo(request,basedata);
            return null;
        }

        /// <summary>
        /// 修改用户登录密码
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("password/modify"), HttpPost]
        [Description("修改用户登录密码")]
        public AdminLoginInfo ModifyAdminPassword(ModifyAdminPasswordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return adminHandler.ModifyAdminPassword(request, basedata);
        }

        /// <summary>
        /// 后台用户登出
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("logout"), HttpPost]
        [Description("后台用户登出")]
        public object AdminLogout()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            adminHandler.AdminLogout(basedata);
            return null;
        }
    }
}
