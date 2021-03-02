using FXCommon.Common;
using MealTicket_Admin_Handler.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MealTicket_Admin_Handler
{
    public class BackAccountHandler
    {
        #region====账户管理====
        /// <summary>
        /// 查询后台用户列表
        /// </summary>
        /// <returns></returns>
        public PageRes<BackAccountInfo> GetBackAccountList(GetBackAccountListRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var accountList = from item in db.t_admin_baseinfo
                                  where item.Id != basedata.AccountId
                                  select item;
                if (!string.IsNullOrEmpty(request.UserName))
                {
                    accountList = from item in accountList
                                  where item.UserName.Contains(request.UserName)
                                  select item;
                }
                if (!string.IsNullOrEmpty(request.Mobile))
                {
                    accountList = from item in accountList
                                  where item.Mobile.Contains(request.Mobile)
                                  select item;
                }
                if (request.Status != 0)
                {
                    accountList = from item in accountList
                                  where item.Status==request.Status
                                  select item;
                }
                if (request.MaxId > 0)
                {
                    accountList = from item in accountList
                                  where item.Id <= request.MaxId
                                  select item;
                }
                int totalCount = accountList.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = accountList.Max(e => e.Id);
                }

                return new PageRes<BackAccountInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in accountList
                            join item2 in db.t_admin_department on item.DepartmentId equals item2.Id into a from ai in a.DefaultIfEmpty()
                            join item3 in db.t_admin_position on item.PositionId equals item3.Id into b from bi in b.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new BackAccountInfo
                            {
                                Sex = item.Sex ?? 0,
                                Status = item.Status,
                                AccountId = item.Id,
                                CreateTime = item.CreateTime,
                                Mobile = item.Mobile,
                                Email = item.Email,
                                RealName = item.RealName,
                                UserName = item.UserName,
                                DepartmentId = ai == null ? 0 : ai.Id,
                                DepartmentName = ai == null ? "" : ai.Name,
                                PositionId = bi == null ? 0 : bi.Id,
                                PositionName = bi == null ? "" : bi.Name,
                                IsAdministrator = item.IsAdministrator
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加后台用户
        /// </summary>
        public void AddBackAccount(AddBackAccountRequest request) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_Admin_AddBackAccount(request.UserName,request.Password.ToMD5(),request.RealName,request.Mobile,request.Sex,request.Email,request.DepartmentId,request.PositionId, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 编辑后台用户
        /// </summary>
        public void ModifyBackAccount(ModifyBackAccountRequest request,HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_Admin_ModifyBackAccount(request.AccountId, basedata.LoginLogId, request.UserName, request.RealName, request.Mobile, request.Sex, request.Email, request.DepartmentId, request.PositionId, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 删除后台用户
        /// </summary>
        public void DeleteBackAccount(DeleteRequest request) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_Admin_DeleteBackAccount(request.Id,errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 修改后台用户状态
        /// </summary>
        public void ModifyBackAccountStatus(ModifyStatusRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var accountInfo = (from item in db.t_admin_baseinfo
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (accountInfo == null)
                {
                    throw new WebApiException(400,"用户信息不存在");
                }

                accountInfo.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改后台用户是否管理员
        /// </summary>
        public void ModifyBackAccountIsAdministrator(ModifyBackAccountIsAdministratorRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var accountInfo = (from item in db.t_admin_baseinfo
                                   where item.Id == request.AccountId
                                   select item).FirstOrDefault();
                if (accountInfo == null)
                {
                    throw new WebApiException(400, "用户信息不存在");
                }

                accountInfo.IsAdministrator = request.IsAdministrator;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改后台用户密码
        /// </summary>
        public void ModifyBackAccountPassword(ModifyBackAccountPasswordRequest request) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_Admin_ModifyBackAccountPassword(request.AccountId, request.NewPassword.ToMD5(), errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 绑定后台用户角色
        /// </summary>
        /// <param name="request"></param>
        public void BindBackAccountRole(BindBackAccountRoleRequest request) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try 
                {
                    //判断AccountId是否存在
                    var account = (from item in db.t_admin_baseinfo
                                   where item.Id == request.AccountId
                                   select item).FirstOrDefault();
                    if (account == null)
                    {
                        throw new WebApiException(400,"后台账户不存在");
                    }

                    var roleRel = (from item in db.t_admin_role_rel
                                   where item.AccountId == request.AccountId
                                   select item).ToList();
                    db.t_admin_role_rel.RemoveRange(roleRel);
                    db.SaveChanges();

                    int i = 0;
                    foreach (var roleId in request.RoleIdList)
                    {
                        //判断roleId是否有效
                        var role = (from item in db.t_admin_role
                                    where item.Id == roleId && item.Status == 1
                                    select item).FirstOrDefault();
                        if (role == null)
                        {
                            continue;
                        }
                        db.t_admin_role_rel.Add(new t_admin_role_rel 
                        {
                            AccountId=request.AccountId,
                            RoleId=roleId
                        });
                        i++;
                    }
                    if (i > 0)
                    {
                        db.SaveChanges();
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 查询后台角色列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<BackAccountRoleInfo> GetBackAccountRoleList(GetBackAccountRoleListRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var roleList = from item in db.t_admin_role
                                  select item;
                if (!string.IsNullOrEmpty(request.RoleName))
                {
                    roleList = from item in roleList
                               where item.RoleName.Contains(request.RoleName)
                               select item;
                }
                if (request.MaxId > 0)
                {
                    roleList = from item in roleList
                               where item.Id <= request.MaxId
                               select item;
                }
                int totalCount = roleList.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = roleList.Max(e => e.Id);
                }

                //查询用户权限
                List<long> accountRoleIdList = new List<long>();
                if (request.AccountId > 0)
                {
                    accountRoleIdList = (from item in db.t_admin_role_rel
                                         where item.AccountId == request.AccountId
                                         select item.RoleId).Distinct().ToList();
                }
                return new PageRes<BackAccountRoleInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in roleList
                            orderby item.CreateTime descending
                            select new BackAccountRoleInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                RoleDescription = item.Description,
                                RoleId = item.Id,
                                RoleName = item.RoleName,
                                IsCheck = accountRoleIdList.Contains(item.Id) ? true : false
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加后台角色
        /// </summary>
        public void AddBackAccountRole(AddBackAccountRoleRequest request) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_Admin_AddBackAccountRole(request.RoleName, request.RoleDescription, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 编辑后台角色
        /// </summary>
        public void ModifyBackAccountRole(ModifyBackAccountRoleRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_Admin_ModifyBackAccountRole(request.RoleId,request.RoleName, request.RoleDescription, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 修改后台角色状态
        /// </summary>
        public void ModifyBackAccountRoleStatus(ModifyStatusRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var role = (from item in db.t_admin_role
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (role == null)
                {
                    throw new WebApiException(400,"角色不存在");
                }
                role.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除后台角色
        /// </summary>
        public void DeleteBackAccountRole(DeleteRequest request) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_Admin_DeleteBackAccountRole(request.Id, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 绑定后台角色权限
        /// </summary>
        public void BindBackAccountRoleRight(BindBackAccountRoleRightRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                //判断roleId是否正确
                var role = (from item in db.t_admin_role
                            where item.Id == request.RoleId
                            select item).FirstOrDefault();
                if (role == null)
                {
                    throw new WebApiException(400,"角色不存在");
                }
                //一级菜单下所有权限Id
                var rightList = (from item in db.t_admin_menu
                                 join item2 in db.t_admin_menu on item.Father equals item2.Id
                                 join item3 in db.t_admin_menu on item2.Father equals item3.Id
                                 join item4 in db.t_admin_right on item.Id equals item4.MenuId
                                 where item.Type == 3 && item2.Type == 2 && item3.Type == 1 && item.Status == 1 && item2.Status == 1 && item3.Status == 1
                                 where item3.Id == request.MenuId
                                 select item4.Id).Distinct().ToList();
                using (var tran = db.Database.BeginTransaction())
                {
                    try 
                    {
                        var roleRight = (from item in db.t_admin_role_right_rel
                                         where item.RoleId == request.RoleId && rightList.Contains(item.RightId)
                                         select item).ToList();
                        db.t_admin_role_right_rel.RemoveRange(roleRight);
                        db.SaveChanges();

                        int i = 0;
                        foreach (var rightId in request.RightIdList)
                        {
                            if (!rightList.Contains(rightId))
                            {
                                continue;
                            }
                            //判断rightId是否正确
                            var right = (from item in db.t_admin_right
                                         where item.Id == rightId
                                         select item).FirstOrDefault();
                            if (right == null)
                            {
                                continue;
                            }
                            db.t_admin_role_right_rel.Add(new t_admin_role_right_rel 
                            {
                                RightId=rightId,
                                RoleId=request.RoleId
                            });
                            i++;
                        }
                        if (i > 0)
                        {
                            db.SaveChanges();
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 查询后台权限列表
        /// </summary>
        /// <returns></returns>
        public object GetBackAccountRightList(GetBackAccountRightListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询角色权限
                List<long> roleRightIdList = new List<long>();
                if (request.RoleId > 0)
                {
                    roleRightIdList = (from item in db.t_admin_role_right_rel
                                       where item.RoleId == request.RoleId
                                       select item.RightId).Distinct().ToList();
                }
                var menuRight = (from item in db.t_admin_menu
                                 join item2 in db.t_admin_menu on item.Father equals item2.Id
                                 join item3 in db.t_admin_menu on item2.Father equals item3.Id
                                 join item4 in db.t_admin_right on item.Id equals item4.MenuId into a
                                 from ai in a.DefaultIfEmpty()
                                 where item.Type == 3 && item2.Type == 2 && item3.Type == 1 && item.Status == 1 && item2.Status == 1 && item3.Status == 1
                                 group new { item, item2, item3, ai } by item3 into g1
                                 orderby g1.Key.OrderIndex
                                 select new 
                                 {
                                     MenuId = g1.Key.Id,
                                     MenuTitle = g1.Key.Title,
                                     MenuList = (from x1 in g1
                                                 group x1 by x1.item2 into g2
                                                 orderby g2.Key.OrderIndex
                                                 select new 
                                                 {
                                                     MenuId = g2.Key.Id,
                                                     MenuTitle = g2.Key.Title,
                                                     TreeId="menu_"+ g2.Key.Id,
                                                     TreeName= g2.Key.Title,
                                                     TreePerms=1,//竖向tre
                                                     MenuList = (from x2 in g2
                                                                 group x2 by x2.item into g3
                                                                 orderby g3.Key.OrderIndex
                                                                 select new 
                                                                 {
                                                                     MenuId = g3.Key.Id,
                                                                     MenuTitle = g3.Key.Title,
                                                                     TreeId = "menu_" + g3.Key.Id,
                                                                     TreeName = g3.Key.Title,
                                                                     TreePerms = 1,//竖向tre
                                                                     MenuList = (from x3 in g3
                                                                                  where x3.ai != null
                                                                                  orderby x3.ai.OrderIndex
                                                                                  select new 
                                                                                  {
                                                                                      RightId = x3.ai.Id,
                                                                                      RightName = x3.ai.RightName,
                                                                                      TreeId = x3.ai.Id+"",
                                                                                      TreeName = x3.ai.RightName,
                                                                                      TreePerms = 2,//横向tre
                                                                                      IsCheck = roleRightIdList.Contains(x3.ai.Id) ? true : false
                                                                                  }).ToList()
                                                                 }).ToList()
                                                 }).ToList()

                                 }).ToList();
                return menuRight;
            }
        }
        #endregion

        #region====日志管理====
        /// <summary>
        /// 获取后台用户登录日志列表
        /// </summary>
        /// <returns></returns>
        public PageRes<LoginLogInfo> GetLoginLogList(GetLoginLogListRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var loginLog = from item in db.t_admin_loginLog
                               select item;
                if (!string.IsNullOrEmpty(request.UserName))
                {
                    loginLog = from item in loginLog
                               where item.AccountName.Contains(request.UserName)
                               select item;
                }
                if (!string.IsNullOrEmpty(request.Ip))
                {
                    loginLog = from item in loginLog
                               where item.LoginIp.Contains(request.Ip)
                               select item;
                }
                if (request.StartTime != null && request.EndTime != null)
                {
                    loginLog = from item in loginLog
                               where item.LoginTime >= request.StartTime && item.LoginTime < request.EndTime
                               select item;
                }
                if (request.MaxId > 0)
                {
                    loginLog = from item in loginLog
                               where item.LogId <= request.MaxId
                               select item;
                }

                int totalCount = loginLog.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = loginLog.Max(e => e.LogId);
                }

                return new PageRes<LoginLogInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in loginLog
                            orderby item.LoginTime descending
                            select new LoginLogInfo
                            {
                                AccountName = item.AccountName,
                                DeviceUA = item.DeviceUA,
                                LogId = item.LogId,
                                LoginIp = item.LoginIp,
                                LoginError = item.LoginError,
                                LoginoutError = item.LoginoutError,
                                LoginTime = item.LoginTime,
                                LogoutTime = item.LogoutTime
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 获取后台用户操作日志列表
        /// </summary>
        /// <returns></returns>
        public PageRes<OperateLogInfo> GetOperateLogList(GetOperateLogListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var operationLog = from item in db.t_admin_operationLog
                                   select item;
                if (!string.IsNullOrEmpty(request.UserName))
                {
                    operationLog = from item in operationLog
                                   where item.AccountName.Contains(request.UserName)
                                   select item;
                }
                if (request.LoginLogId > 0)
                {
                    operationLog = from item in operationLog
                                   where item.LoginLogId == request.LoginLogId
                                   select item;
                }
                if (request.StartTime != null && request.EndTime != null)
                {
                    operationLog = from item in operationLog
                                   where item.CreateTime >= request.StartTime && item.CreateTime < request.EndTime
                                   select item;
                }
                if (request.MaxId > 0)
                {
                    operationLog = from item in operationLog
                                   where item.LogId <= request.MaxId
                                   select item;
                }

                int totalCount = operationLog.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = operationLog.Max(e => e.LogId);
                }

                return new PageRes<OperateLogInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in operationLog
                            orderby item.CreateTime descending
                            select new OperateLogInfo
                            {
                                LogId = item.LogId,
                                CreateTime = item.CreateTime,
                                LoginLogId = item.LoginLogId,
                                OperationDes = item.OperationDes,
                                UserName = item.AccountName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }
        #endregion

        #region====组织管理====
        /// <summary>
        /// 查询后台职位列表
        /// </summary>
        /// <returns></returns>
        public PageRes<BackAccountPositionInfo> GetBackAccountPositionList(GetBackAccountPositionListRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var positionList = from item in db.t_admin_position
                                   select item;
                if (!string.IsNullOrEmpty(request.PositionName))
                {
                    positionList = from item in positionList
                                   where item.Name.Contains(request.PositionName)
                                   select item;
                }
                if (request.Status != 0)
                {
                    positionList = from item in positionList
                                   where item.Status==request.Status
                                   select item;
                }
                if (request.MaxId > 0)
                {
                    positionList = from item in positionList
                                   where item.Id<=request.MaxId
                                   select item;
                }

                int totalCount = positionList.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = positionList.Max(e=>e.Id);
                }

                return new PageRes<BackAccountPositionInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in positionList
                            orderby item.CreateTime descending
                            select new BackAccountPositionInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Description = item.Description,
                                Id = item.Id,
                                Name = item.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加后台职位
        /// </summary>
        public void AddBackAccountPosition(AddBackAccountPositionRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                //判断职位名称是否存在
                var position = (from item in db.t_admin_position
                                where item.Name == request.Name
                                select item).FirstOrDefault();
                if (position != null)
                {
                    throw new WebApiException(400,"职位名已存在");
                }
                db.t_admin_position.Add(new t_admin_position 
                {
                    CreateTime=DateTime.Now,
                    Status=1,
                    Description=request.Description,
                    Name=request.Name,
                    UpdateTime=DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑后台职位
        /// </summary>
        /// <returns></returns>
        public void ModifyBackAccountPosition(ModifyBackAccountPositionRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var position = (from item in db.t_admin_position
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (position == null)
                {
                    throw new WebApiException(400, "职位不存在");
                }
                //判断职位名称是否存在
                var temp = (from item in db.t_admin_position
                            where item.Name == request.Name && item.Id != request.Id
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "职位名已存在");
                }
                position.Name = request.Name;
                position.Description = request.Description;
                position.UpdateTime = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改后台职位状态
        /// </summary>
        public void ModifyBackAccountPositionStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var position = (from item in db.t_admin_position
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (position == null)
                {
                    throw new WebApiException(400, "职位不存在");
                }
                position.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除后台职位
        /// </summary>
        public void DeleteBackAccountPosition(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var position = (from item in db.t_admin_position
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (position == null)
                {
                    throw new WebApiException(400, "职位不存在");
                }
                db.t_admin_position.Remove(position);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询后台部门列表
        /// </summary>
        /// <returns></returns>
        public PageRes<BackAccountDepartmentInfo> GetBackAccountDepartmentList(GetBackAccountDepartmentListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var departmentList = from item in db.t_admin_department
                                     select item;
                if (!string.IsNullOrEmpty(request.DepartmentName))
                {
                    departmentList = from item in departmentList
                                     where item.Name.Contains(request.DepartmentName)
                                     select item;
                }
                if (request.Status!=0)
                {
                    departmentList = from item in departmentList
                                     where item.Status==request.Status
                                     select item;
                }
                if (request.MaxId > 0)
                {
                    departmentList = from item in departmentList
                                     where item.Id <= request.MaxId
                                     select item;
                }

                int totalCount = departmentList.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = departmentList.Max(e => e.Id);
                }

                return new PageRes<BackAccountDepartmentInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in departmentList
                            orderby item.CreateTime descending
                            select new BackAccountDepartmentInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Description = item.Description,
                                Id = item.Id,
                                Name = item.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加后台部门
        /// </summary>
        public void AddBackAccountDepartment(AddBackAccountDepartmentRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断职位名称是否存在
                var department = (from item in db.t_admin_department
                                where item.Name == request.Name
                                select item).FirstOrDefault();
                if (department != null)
                {
                    throw new WebApiException(400, "部门名已存在");
                }
                db.t_admin_department.Add(new t_admin_department
                {
                    CreateTime = DateTime.Now,
                    Status = 1,
                    Description = request.Description,
                    Name = request.Name,
                    UpdateTime = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑后台部门
        /// </summary>
        /// <returns></returns>
        public void ModifyBackAccountDepartment(ModifyBackAccountDepartmentRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var department = (from item in db.t_admin_department
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (department == null)
                {
                    throw new WebApiException(400, "部门不存在");
                }
                //判断职位名称是否存在
                var temp = (from item in db.t_admin_department
                            where item.Name == request.Name && item.Id != request.Id
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "部门名已存在");
                }
                department.Name = request.Name;
                department.Description = request.Description;
                department.UpdateTime = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改后台部门状态
        /// </summary>
        public void ModifyBackAccountDepartmentStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var department = (from item in db.t_admin_department
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (department == null)
                {
                    throw new WebApiException(400, "部门不存在");
                }
                department.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除后台部门
        /// </summary>
        public void DeleteBackAccountDepartment(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var department = (from item in db.t_admin_department
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (department == null)
                {
                    throw new WebApiException(400, "部门不存在");
                }
                db.t_admin_department.Remove(department);
                db.SaveChanges();
            }
        }
        #endregion
    }
}
