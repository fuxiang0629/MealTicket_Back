using MealTicket_Admin_Handler.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler
{
    public class AuthorityHandler
    {
        /// <summary>
        /// 添加访问日志
        /// </summary>
        public void AddVisitLog(VisitLog log)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_authority_admin_visitlog.Add(new t_authority_admin_visitlog
                {
                    Function = log.Function,
                    Ip = log.Ip,
                    Request = log.Request,
                    Method = log.Method,
                    Time = log.Time,
                    Url = log.Url,
                    UserAgent = log.Useragent,
                    ResultCode = log.ResultCode,
                    ResultMessage = log.ResultMessage,
                    UserToken = log.UserToken
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 检查用户是否登录
        /// </summary>
        public void CheckAccountLogin(string userToken, HeadBase baseData)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                long accountId = 0;
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter accountIdDb = new ObjectParameter("accountId", 0);
                    ObjectParameter loginLogIdDb = new ObjectParameter("loginLogId", 0);
                    db.P_Admin_CheckLogin(userToken, errorCodeDb, errorMessageDb, accountIdDb, loginLogIdDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    accountId = (long)accountIdDb.Value;
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                    baseData.AccountId = accountId;
                    baseData.LoginLogId = (long)loginLogIdDb.Value;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                var account = (from item in db.t_admin_baseinfo
                               where item.Id == accountId
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new AuthorizationException(301,"用户未登录,请先登录");
                }
                baseData.IsAdministrator = account.IsAdministrator;
            }
        }

        /// <summary>
        /// 检查用户权限
        /// </summary>
        public void CheckUserPowerFilter(HeadBase baseInfo)
        {
            using (var db = new meal_ticketEntities())
            {
                var menu = (from item in db.t_admin_menu
                            where item.Url == baseInfo.MenuUrl
                            select item).FirstOrDefault();
                if (menu == null)
                {
                    throw new WebApiException(400, "无权访问");
                }

                var api = (from item in db.t_admin_api
                           where item.Url == (baseInfo.Controller + "." + baseInfo.Action)
                           select item).FirstOrDefault();
                if (api == null)
                {
                    return;
                }

                //查询当前权限Id
                var right = (from item in db.t_admin_right
                             join item2 in db.t_admin_right_api_rel on item.Id equals item2.RightId
                             where item2.ApiUrl== api.Url && item.MenuId== menu.Id
                             select item.Id).ToList();
                if (right.Count()<=0)
                {
                    throw new WebApiException(400,"无权访问");
                }

                //查询用户角色组
                var roleIdList = (from item in db.t_admin_role_rel
                                  join item2 in db.t_admin_role on item.RoleId equals item2.Id
                                  where item.AccountId == baseInfo.AccountId && item2.Status == 1
                                  select item.RoleId).Distinct().ToList();
                //判断用户是否有权限
                var roleRight = (from item in db.t_admin_role_right_rel
                                 where right.Contains(item.RightId) && roleIdList.Contains(item.RoleId)
                                 select item).FirstOrDefault();
                if (roleRight == null)
                {
                    throw new WebApiException(400, "无权访问");
                }
                return;
            }
        }

        /// <summary>
        /// 更新Api
        /// </summary>
        public void InitActionList(List<ActionList> list)
        {
            using (var db = new meal_ticketEntities())
            {
                var actionNow = (from item in db.t_admin_api
                                 select item.Url).ToList();


                var actionList = (from item in list
                                  where !actionNow.Contains(item.Url)
                                  select new t_admin_api
                                  {
                                      CreateTime = DateTime.Now,
                                      Description = item.Description,
                                      Name = item.Description,
                                      Url = item.Url
                                  }).ToList();

                db.t_admin_api.AddRange(actionList);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 新增操作记录
        /// </summary>
        public void AddOperationLog(long accountId, long loginLogId, string operationCode, string operationDes)
        {
            using (var db = new meal_ticketEntities())
            {
                if (string.IsNullOrEmpty(operationDes))
                {
                    operationDes = (from item in db.t_admin_api
                                    where item.Url == operationCode
                                    select item.Name).FirstOrDefault();
                }
                var account = (from item in db.t_admin_baseinfo
                               where item.Id == accountId
                               select item).FirstOrDefault();
                db.t_admin_operationLog.Add(new t_admin_operationLog
                {
                    AccountId = accountId,
                    AccountName = account==null?"": account.UserName,
                    CreateTime = DateTime.Now,
                    LoginLogId = loginLogId,
                    OperationCode = operationCode,
                    OperationDes = operationDes
                });
                db.SaveChanges();
            }
        }
    }
}
