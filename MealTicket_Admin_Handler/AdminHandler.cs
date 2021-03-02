using FXCommon.Common;
using MealTicket_Admin_Handler.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler
{
    public class AdminHandler
    {
        /// <summary>
        /// 后台用户登录
        /// </summary>
        /// <returns></returns>
        public AdminLoginInfo AdminLogin(AdminLoginRequest request,HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter tokenDb = new ObjectParameter("token", "");
                    ObjectParameter accountIdDb = new ObjectParameter("accountId", 0);
                    ObjectParameter isAdministratorDb = new ObjectParameter("isAdministrator", false);
                    db.P_Admin_Login(request.UserName, request.Password.ToMD5(), basedata.Ip, basedata.UserAgent, errorCodeDb, errorMessageDb, tokenDb, accountIdDb, isAdministratorDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    long accountId = (long)accountIdDb.Value;
                    bool isAdministrator = (bool)isAdministratorDb.Value;
                    List<string> rightCodeId = new List<string>();
                    if (!isAdministrator)
                    {
                        rightCodeId = (from item in db.t_admin_role_rel
                                       join item2 in db.t_admin_role on item.RoleId equals item2.Id
                                       join item3 in db.t_admin_role_right_rel on item2.Id equals item3.RoleId
                                       join item4 in db.t_admin_right on item3.RightId equals item4.Id
                                       where item.AccountId == accountId && item2.Status == 1
                                       select item4.RightCode).Distinct().ToList();
                    }
                    tran.Commit();
                    return new AdminLoginInfo
                    {
                        UserToken = tokenDb.Value.ToString(),
                        IsAdministrator= isAdministrator,
                        RightCodeList= rightCodeId
                    };
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 查询后台用户基本信息
        /// </summary>
        /// <returns></returns>
        public AdminBaseInfo GetAdminBaseInfo(HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var adminInfo = (from item in db.t_admin_baseinfo
                                 join item2 in db.t_admin_department on item.DepartmentId equals item2.Id into a
                                 from ai in a.DefaultIfEmpty()
                                 join item3 in db.t_admin_position on item.PositionId equals item3.Id into b
                                 from bi in b.DefaultIfEmpty()
                                 where item.Id == basedata.AccountId
                                 select new AdminBaseInfo
                                 {
                                     Sex = item.Sex ?? 0,
                                     DepartmentId = ai == null ? 0 : ai.Id,
                                     DepartmentName = ai == null ? "" : ai.Name,
                                     Email = item.Email,
                                     Mobile = item.Mobile,
                                     PositionId = bi == null ? 0 : bi.Id,
                                     PositionName = bi == null ? "" : bi.Name,
                                     RealName = item.RealName,
                                     UserName = item.UserName
                                 }).FirstOrDefault();
                return adminInfo;
            }
        }

        /// <summary>
        /// 修改用户基本信息
        /// </summary>
        public void ModifyAdminBaseInfo(ModifyAdminBaseInfoRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var adminInfo = (from item in db.t_admin_baseinfo
                                 where item.Id == basedata.AccountId
                                 select item).FirstOrDefault();
                if (adminInfo == null)
                {
                    throw new WebApiException(400,"用户不存在");
                }

                adminInfo.Email = request.Email;
                adminInfo.LastModified = DateTime.Now;
                adminInfo.Mobile = request.Mobile;
                adminInfo.RealName = request.RealName;
                adminInfo.Sex = request.Sex;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改用户登录密码
        /// </summary>
        public AdminLoginInfo ModifyAdminPassword(ModifyAdminPasswordRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter tokenDb = new ObjectParameter("token", "");
                    db.P_Admin_ModifyPassword(basedata.AccountId, request.OldPassword.ToMD5(), request.NewPassword.ToMD5(), basedata.LoginLogId, errorCodeDb, errorMessageDb, tokenDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                    return new AdminLoginInfo
                    {
                        UserToken = tokenDb.Value.ToString()
                    };
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 后台用户登出
        /// </summary>
        /// <param name="basedata"></param>
        public void AdminLogout(HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_Admin_Logout(basedata.AccountId, basedata.LoginLogId, errorCodeDb, errorMessageDb);
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
    }
}
