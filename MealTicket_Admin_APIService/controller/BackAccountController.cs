using MealTicket_Admin_APIService.Filter;
using MealTicket_Admin_Handler;
using MealTicket_Admin_Handler.Model;
using Ninject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MealTicket_Admin_APIService.controller
{
    [RoutePrefix("backaccount")]
    public class BackAccountController:ApiController
    {
        private BackAccountHandler backAccountHandler;

        public BackAccountController()
        {
            backAccountHandler = WebApiManager.Kernel.Get<BackAccountHandler>();
        }

        #region====账户管理====
        /// <summary>
        /// 查询后台用户列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("list"),HttpPost]
        [Description("查询后台用户列表")]
        public PageRes<BackAccountInfo> GetBackAccountList(GetBackAccountListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return backAccountHandler.GetBackAccountList(request, basedata);
        }

        /// <summary>
        /// 添加后台用户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("add"), HttpPost]
        [Description("添加后台用户")]
        public object AddBackAccount(AddBackAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (string.IsNullOrEmpty(request.UserName))
            {
                throw new WebApiException(400, "参数错误:用户名不能为空");
            }
            if (string.IsNullOrEmpty(request.Password))
            {
                throw new WebApiException(400, "参数错误:密码不能为空");
            }
            backAccountHandler.AddBackAccount(request);
            return null;
        }

        /// <summary>
        /// 编辑后台用户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("modify"), HttpPost]
        [Description("编辑后台用户")]
        public object ModifyBackAccount(ModifyBackAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            backAccountHandler.ModifyBackAccount(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除后台用户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("delete"), HttpPost]
        [Description("删除后台用户")]
        public object DeleteBackAccount(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.DeleteBackAccount(request);
            return null;
        }

        /// <summary>
        /// 修改后台用户状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("status/modify"), HttpPost]
        [Description("修改后台用户状态")]
        public object ModifyBackAccountStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.ModifyBackAccountStatus(request);
            return null;
        }

        /// <summary>
        /// 修改后台用户是否管理员
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("isadministrator/modify"), HttpPost]
        [Description("修改后台用户是否管理员")]
        public object ModifyBackAccountIsAdministrator(ModifyBackAccountIsAdministratorRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.ModifyBackAccountIsAdministrator(request);
            return null;
        }

        /// <summary>
        /// 修改后台用户密码
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("password/modify"), HttpPost]
        [Description("修改后台用户密码")]
        public object ModifyBackAccountPassword(ModifyBackAccountPasswordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.ModifyBackAccountPassword(request);
            return null;
        }

        /// <summary>
        /// 绑定后台用户角色
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("role/bind"), HttpPost]
        [Description("绑定后台用户角色")]
        public object BindBackAccountRole(BindBackAccountRoleRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.BindBackAccountRole(request);
            return null;
        }

        /// <summary>
        /// 查询后台角色列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("role/list"), HttpPost]
        [Description("查询后台角色列表")]
        public PageRes<BackAccountRoleInfo> GetBackAccountRoleList(GetBackAccountRoleListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return backAccountHandler.GetBackAccountRoleList(request);
        }

        /// <summary>
        /// 添加后台角色
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("role/add"), HttpPost]
        [Description("添加后台角色")]
        public object AddBackAccountRole(AddBackAccountRoleRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.AddBackAccountRole(request);
            return null;
        }

        /// <summary>
        /// 编辑后台角色
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("role/modify"), HttpPost]
        [Description("编辑后台角色")]
        public object ModifyBackAccountRole(ModifyBackAccountRoleRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.ModifyBackAccountRole(request);
            return null;
        }

        /// <summary>
        /// 修改后台角色状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("role/status/modify"), HttpPost]
        [Description("修改后台角色状态")]
        public object ModifyBackAccountRoleStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.ModifyBackAccountRoleStatus(request);
            return null;
        }

        /// <summary>
        /// 删除后台角色
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("role/delete"), HttpPost]
        [Description("删除后台角色")]
        public object DeleteBackAccountRole(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.DeleteBackAccountRole(request);
            return null;
        }

        /// <summary>
        /// 绑定后台角色权限
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("role/right/bind"), HttpPost]
        [Description("绑定后台角色权限")]
        public object BindBackAccountRoleRight(BindBackAccountRoleRightRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.BindBackAccountRoleRight(request);
            return null;
        }

        /// <summary>
        /// 查询后台权限列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("right/list"), HttpPost]
        [Description("查询后台权限列表")]
        public object GetBackAccountRightList(GetBackAccountRightListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return backAccountHandler.GetBackAccountRightList(request);
        }
        #endregion

        #region====日志管理====
        /// <summary>
        /// 查询后台用户登录日志列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("login/log/list"),HttpPost]
        [Description("查询后台用户登录日志列表")]
        public PageRes<LoginLogInfo> GetLoginLogList(GetLoginLogListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            return backAccountHandler.GetLoginLogList(request);
        }

        /// <summary>
        /// 查询后台用户操作日志列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("operate/log/list"), HttpPost]
        [Description("查询后台用户操作日志列表")]
        public PageRes<OperateLogInfo> GetOperateLogList(GetOperateLogListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return backAccountHandler.GetOperateLogList(request);
        }
        #endregion

        #region====组织管理====
        /// <summary>
        /// 查询后台职位列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("position/list"), HttpPost]
        [Description("查询后台职位列表")]
        public PageRes<BackAccountPositionInfo> GetBackAccountPositionList(GetBackAccountPositionListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return backAccountHandler.GetBackAccountPositionList(request);
        }

        /// <summary>
        /// 添加后台职位
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("position/add"), HttpPost]
        [Description("添加后台职位")]
        public object AddBackAccountPosition(AddBackAccountPositionRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.AddBackAccountPosition(request);
            return null;
        }

        /// <summary>
        /// 编辑后台职位
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("position/modify"), HttpPost]
        [Description("编辑后台职位")]
        public object ModifyBackAccountPosition(ModifyBackAccountPositionRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.ModifyBackAccountPosition(request);
            return null;
        }

        /// <summary>
        /// 修改后台职位状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("position/status/modify"), HttpPost]
        [Description("修改后台职位状态")]
        public object ModifyBackAccountPositionStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.ModifyBackAccountPositionStatus(request);
            return null;
        }

        /// <summary>
        /// 删除后台职位
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("position/delete"), HttpPost]
        [Description("删除后台职位")]
        public object DeleteBackAccountPosition(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.DeleteBackAccountPosition(request);
            return null;
        }

        /// <summary>
        /// 查询后台部门列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("department/list"), HttpPost]
        [Description("查询后台部门列表")]
        public PageRes<BackAccountDepartmentInfo> GetBackAccountDepartmentList(GetBackAccountDepartmentListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return backAccountHandler.GetBackAccountDepartmentList(request);
        }

        /// <summary>
        /// 添加后台部门
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("department/add"), HttpPost]
        [Description("添加后台部门")]
        public object AddBackAccountDepartment(AddBackAccountDepartmentRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.AddBackAccountDepartment(request);
            return null;
        }

        /// <summary>
        /// 编辑后台部门
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("department/modify"), HttpPost]
        [Description("编辑后台部门")]
        public object ModifyBackAccountDepartment(ModifyBackAccountDepartmentRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.ModifyBackAccountDepartment(request);
            return null;
        }

        /// <summary>
        /// 修改后台部门状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("department/status/modify"), HttpPost]
        [Description("修改后台部门状态")]
        public object ModifyBackAccountDepartmentStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.ModifyBackAccountDepartmentStatus(request);
            return null;
        }

        /// <summary>
        /// 删除后台部门
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("department/delete"), HttpPost]
        [Description("删除后台部门")]
        public object DeleteBackAccountDepartment(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            backAccountHandler.DeleteBackAccountDepartment(request);
            return null;
        }
        #endregion
    }
}
