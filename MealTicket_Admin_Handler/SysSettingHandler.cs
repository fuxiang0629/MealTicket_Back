using FXCommon.Common;
using MealTicket_Admin_Handler.Model;
using MealTicket_DBCommon;
using Newtonsoft.Json;
using SMS_SendHandler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler
{
    public class SysSettingHandler
    {
        #region====参数管理====
        /// <summary>
        /// 查询系统参数列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SysParInfo> GetSysParList(GetSysParListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var par = from item in db.t_system_param
                          select item;
                if (!string.IsNullOrEmpty(request.ParName))
                {
                    par = from item in par
                          where item.ParamName.Contains(request.ParName)
                          select item;
                }
                int totalCount = par.Count();

                return new PageRes<SysParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in par
                            orderby item.CreateTime descending
                            select new SysParInfo
                            {
                                CreateTime = item.LastModified,
                                ParDescription = item.Description,
                                ParName = item.ParamName,
                                ParValue = item.ParamValue,
                                IsJson = item.IsJson,
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 编辑系统参数
        /// </summary>
        public void ModifySysPar(ModifySysParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var par = (from item in db.t_system_param
                           where item.ParamName == request.ParName
                           select item).FirstOrDefault();
                if (par == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                par.ParamValue = request.ParValue;
                par.Description = request.ParDescription;
                par.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询app版本列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SysAppversionInfo> GetSysAppversionList(PageRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var appVersion = from item in db.t_app_version
                                 select item;
                int totalCount = appVersion.Count();

                return new PageRes<SysAppversionInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in appVersion
                            orderby item.CreateTime descending
                            select new SysAppversionInfo
                            {
                                AppVersion = item.AppVersion,
                                AppVersionOrder = item.AppVersionOrder,
                                ExamineStatus = item.ExamineStatus,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                MarketCode = item.MarketCode,
                                WaitTime = item.WaitTime,
                                WebViewUrl = item.WebViewUrl
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加app版本
        /// </summary>
        /// <param name="request"></param>
        public void AddSysAppversion(AddSysAppversionRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断版本是否已存在
                var version = (from item in db.t_app_version
                               where item.MarketCode == request.MarketCode && item.AppVersion == request.AppVersion
                               select item).FirstOrDefault();
                if (version != null)
                {
                    throw new WebApiException(400,"版本已存在");
                }
                db.t_app_version.Add(new t_app_version 
                {
                    AppVersion=request.AppVersion,
                    AppVersionOrder=request.AppVersionOrder,
                    ExamineStatus=request.ExamineStatus,
                    CreateTime=DateTime.Now,
                    LastModified=DateTime.Now,
                    MarketCode=request.MarketCode,
                    WaitTime=request.WaitTime,
                    WebViewUrl=request.WebViewUrl
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑app版本
        /// </summary>
        /// <param name="request"></param>
        public void ModifySysAppversion(ModifySysAppversionRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                //判断版本是否已存在
                var version = (from item in db.t_app_version
                               where item.Id==request.Id
                               select item).FirstOrDefault();
                if (version == null)
                {
                    throw new WebApiException(400, "数据已存在");
                }

                var tempVersion = (from item in db.t_app_version
                                   where item.MarketCode == request.MarketCode && item.AppVersion == request.AppVersion && item.Id != request.Id
                                   select item).FirstOrDefault();
                if (tempVersion != null)
                {
                    throw new WebApiException(400, "版本已存在");
                }
                version.AppVersion = request.AppVersion;
                version.AppVersionOrder = request.AppVersionOrder;
                version.ExamineStatus = request.ExamineStatus;
                version.LastModified = DateTime.Now;
                version.MarketCode = request.MarketCode;
                version.WaitTime = request.WaitTime;
                version.WebViewUrl = request.WebViewUrl;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除app版本
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSysAppversion(DeleteRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                //判断版本是否已存在
                var version = (from item in db.t_app_version
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (version == null)
                {
                    throw new WebApiException(400, "数据已存在");
                }
                db.t_app_version.Remove(version);
                db.SaveChanges();
            }
        }
        #endregion

        #region====股票账号====
        /// <summary>
        /// 查询服务器列表
        /// </summary>
        /// <returns></returns>
        public PageRes<ServerInfo> GetServerList(GetServerListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var serverList = from item in db.t_server
                                 select item;
                if (!string.IsNullOrEmpty(request.ServerId))
                {
                    serverList = from item in serverList
                                 where item.ServerId.Contains(request.ServerId)
                                 select item;
                }

                int totalCount = serverList.Count();

                return new PageRes<ServerInfo>
                {
                    TotalCount = totalCount,
                    MaxId = 0,
                    List = (from item in serverList
                            orderby item.CreateTime descending
                            select new ServerInfo
                            {
                                ServerDes = item.ServerDes,
                                ServerId = item.ServerId,
                                AccountCount = (from x in db.t_server_broker_account_rel
                                                where x.ServerId == item.ServerId
                                                select x).Count(),
                                CreateTime = item.CreateTime
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加服务器
        /// </summary>
        public void AddServer(AddServerRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var server = (from item in db.t_server
                              where item.ServerId == request.ServerId
                              select item).FirstOrDefault();
                if (server != null)
                {
                    throw new WebApiException(400, "服务器代码已存在");
                }

                db.t_server.Add(new t_server
                {
                    ServerDes = request.ServerDes,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    ServerId = request.ServerId
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑服务器
        /// </summary>
        public void ModifyServer(ModifyServerRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var server = (from item in db.t_server
                              where item.ServerId == request.ServerId
                              select item).FirstOrDefault();
                if (server == null)
                {
                    throw new WebApiException(400, "服务器不存在");
                }

                server.ServerDes = request.ServerDes;
                server.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除服务器
        /// </summary>
        public void DeleteServer(DeleteServerRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var server = (from item in db.t_server
                                  where item.ServerId == request.ServerId
                                  select item).FirstOrDefault();
                    if (server == null)
                    {
                        throw new WebApiException(400, "服务器不存在");
                    }

                    db.t_server.Remove(server);
                    db.SaveChanges();

                    var serverAccount = (from item in db.t_server_broker_account_rel
                                         where item.ServerId == request.ServerId
                                         select item).ToList();
                    if (serverAccount.Count() > 0)
                    {
                        db.t_server_broker_account_rel.RemoveRange(serverAccount);
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
        /// 绑定服务器账户
        /// </summary>
        /// <param name="request"></param>
        public void BindServerAccount(BindServerAccountRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //判断serverId是否存在
                    var server = (from item in db.t_server
                                  where item.ServerId == request.ServerId
                                  select item).FirstOrDefault();
                    if (server == null)
                    {
                        throw new WebApiException(400, "服务器不存在");
                    }
                    //删除原先绑定数据
                    var serverAccount = (from item in db.t_server_broker_account_rel
                                         where item.ServerId == request.ServerId
                                         select item).ToList();
                    if (serverAccount.Count() > 0)
                    {
                        db.t_server_broker_account_rel.RemoveRange(serverAccount);
                        db.SaveChanges();
                    }

                    foreach (var item in request.AccountIdList)
                    {
                        //判断账户是否存在
                        var account = (from x in db.t_broker_account_info
                                       where x.Status == 1 && x.Id == item
                                       select x).FirstOrDefault();
                        if (account == null)
                        {
                            throw new WebApiException(400, "存在无效账户");
                        }
                        //判断账户有没有被其他服务器绑定
                        var accountServer = (from x in db.t_server_broker_account_rel
                                             where x.BrokerAccountId == item
                                             select x).FirstOrDefault();
                        if (accountServer != null)
                        {
                            throw new WebApiException(400, "存在账户已被绑定");
                        }
                        //绑定
                        db.t_server_broker_account_rel.Add(new t_server_broker_account_rel
                        {
                            ServerId = request.ServerId,
                            BrokerAccountId = item
                        });
                    }
                    db.SaveChanges();
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
        /// 查询券商账户列表
        /// </summary>
        /// <returns></returns>
        public PageRes<BrokerAccountInfo> GetBrokerAccountList(GetBrokerAccountListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var brokerAccount = from item in db.t_broker_account_info
                                    select item;
                if (!string.IsNullOrEmpty(request.AccountNo))
                {
                    brokerAccount = from item in brokerAccount
                                    where item.AccountNo.Contains(request.AccountNo)
                                    select item;
                }
                if (request.Status != 0)
                {
                    brokerAccount = from item in brokerAccount
                                    where item.Status == request.Status
                                    select item;
                }
                if (request.MaxId > 0)
                {
                    brokerAccount = from item in brokerAccount
                                    where item.Id <= request.MaxId
                                    select item;
                }

                int totalCount = brokerAccount.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = brokerAccount.Max(e => e.Id);
                }

                var checkerList = new List<long>();
                var busyList = new List<long>();
                if (!string.IsNullOrEmpty(request.ServerId))
                {
                    checkerList = (from item in brokerAccount
                                   join item2 in db.t_server_broker_account_rel on item.Id equals item2.BrokerAccountId
                                   where item2.ServerId == request.ServerId
                                   select item.Id).ToList();
                    busyList = (from item in brokerAccount
                                join item2 in db.t_server_broker_account_rel on item.Id equals item2.BrokerAccountId
                                where item2.ServerId != request.ServerId
                                select item.Id).ToList();
                }

                return new PageRes<BrokerAccountInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in brokerAccount
                            join item2 in db.t_broker_department on item.DepartmentCode equals item2.DepartmentCode into a
                            from ai in a.DefaultIfEmpty()
                            join item3 in db.t_broker on item.BrokerCode equals item3.BrokerCode into b
                            from bi in b.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new BrokerAccountInfo
                            {
                                Status = item.Status,
                                AccountCode = item.AccountCode,
                                AccountNo = item.AccountNo,
                                AccountType = item.AccountType,
                                BrokerCode = item.BrokerCode,
                                CreateTime = item.CreateTime,
                                DepartmentCode = item.DepartmentCode,
                                Id = item.Id,
                                JyPassword = item.JyPassword,
                                IsPrivate=item.IsPrivate,
                                TradeAccountNo0 = item.TradeAccountNo0,
                                TradeAccountNo1 = item.TradeAccountNo1,
                                TxPassword = item.TxPassword,
                                Holder0 = item.Holder0,
                                Holder1 = item.Holder1,
                                InitialFunding = item.InitialFunding,
                                DepartmentName = ai == null ? "" : ai.DepartmentName,
                                BrokerName = bi == null ? "" : bi.BrokerName,
                                IsChecked = checkerList.Contains(item.Id) ? true : false,
                                CanChecked = busyList.Contains(item.Id) ? false : true
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加券商账户
        /// </summary>
        public void AddBrokerAccount(AddBrokerAccountRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断AccountCode是否重复
                var brokerAccount = (from item in db.t_broker_account_info
                                     where item.AccountCode == request.AccountCode || item.AccountNo == request.AccountNo
                                     select item).FirstOrDefault();
                if (brokerAccount != null)
                {
                    if (brokerAccount.AccountCode == request.AccountCode)
                    {
                        throw new WebApiException(400, "账户code已存在");
                    }
                    if (brokerAccount.AccountNo == request.AccountNo)
                    {
                        throw new WebApiException(400, "账户已存在");
                    }
                }
                //判断券商是否存在
                if (request.BrokerCode != 0)
                {
                    var broker = (from item in db.t_broker
                                  where item.BrokerCode == request.BrokerCode && item.Status == 1
                                  select item).FirstOrDefault();
                    if (broker == null)
                    {
                        throw new WebApiException(400, "无效的券商");
                    }
                }
                //判断营业部是否存在
                if (request.DepartmentCode != 0)
                {
                    var department = (from item in db.t_broker_department
                                      where item.DepartmentCode == request.DepartmentCode && item.Status == 1
                                      select item).FirstOrDefault();
                    if (department == null)
                    {
                        throw new WebApiException(400, "无效的营业部");
                    }
                }

                db.t_broker_account_info.Add(new t_broker_account_info
                {
                    Status = 1,
                    AccountCode = request.AccountCode,
                    AccountNo = request.AccountNo,
                    BrokerCode = request.BrokerCode,
                    CreateTime = DateTime.Now,
                    AccountType = request.AccountType,
                    DepartmentCode = request.DepartmentCode,
                    JyPassword = request.JyPassword,
                    LastModified = DateTime.Now,
                    TradeAccountNo0 = request.TradeAccountNo0,
                    TradeAccountNo1 = request.TradeAccountNo1,
                    Holder0 = request.Holder0,
                    Holder1 = request.Holder1,
                    InitialFunding = request.InitialFunding,
                    TxPassword = request.TxPassword,
                    IsPrivate=request.IsPrivate
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑券商账户
        /// </summary>
        /// <param name="request"></param>
        public void ModifyBrokerAccount(ModifyBrokerAccountRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var brokerAccount = (from item in db.t_broker_account_info
                                     where item.Id == request.Id
                                     select item).FirstOrDefault();
                if (brokerAccount == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                //判断AccountCode是否重复
                var temp = (from item in db.t_broker_account_info
                            where (item.AccountCode == request.AccountCode || item.AccountNo == request.AccountNo) && item.Id != request.Id
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    if (temp.AccountCode == request.AccountCode)
                    {
                        throw new WebApiException(400, "账户code已存在");
                    }
                    if (temp.AccountNo == request.AccountNo)
                    {
                        throw new WebApiException(400, "账户已存在");
                    }
                }
                //判断券商是否存在
                if (request.BrokerCode != 0)
                {
                    var broker = (from item in db.t_broker
                                  where item.BrokerCode == request.BrokerCode
                                  select item).FirstOrDefault();
                    if (broker == null)
                    {
                        throw new WebApiException(400, "无效的券商");
                    }
                }
                //判断营业部是否存在
                if (request.DepartmentCode != 0)
                {
                    var department = (from item in db.t_broker_department
                                      where item.DepartmentCode == request.DepartmentCode
                                      select item).FirstOrDefault();
                    if (department == null)
                    {
                        throw new WebApiException(400, "无效的营业部");
                    }
                }

                brokerAccount.AccountCode = request.AccountCode;
                brokerAccount.AccountNo = request.AccountNo;
                brokerAccount.BrokerCode = request.BrokerCode;
                brokerAccount.AccountType = request.AccountType;
                brokerAccount.DepartmentCode = request.DepartmentCode;
                brokerAccount.JyPassword = request.JyPassword;
                brokerAccount.LastModified = DateTime.Now;
                brokerAccount.TradeAccountNo0 = request.TradeAccountNo0;
                brokerAccount.TradeAccountNo1 = request.TradeAccountNo1;
                brokerAccount.Holder0 = request.Holder0;
                brokerAccount.Holder1 = request.Holder1;
                brokerAccount.InitialFunding = request.InitialFunding;
                brokerAccount.TxPassword = request.TxPassword;
                brokerAccount.IsPrivate = request.IsPrivate;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改券商账户状态
        /// </summary>
        public void ModifyBrokerAccountStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var brokerAccount = (from item in db.t_broker_account_info
                                     where item.Id == request.Id
                                     select item).FirstOrDefault();
                if (brokerAccount == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                brokerAccount.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除券商账户
        /// </summary>
        public void DeleteBrokerAccount(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var brokerAccount = (from item in db.t_broker_account_info
                                         where item.Id == request.Id
                                         select item).FirstOrDefault();
                    if (brokerAccount == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }

                    db.t_broker_account_info.Remove(brokerAccount);
                    db.SaveChanges();

                    var serverAccount = (from item in db.t_server_broker_account_rel
                                         where item.BrokerAccountId == request.Id
                                         select item).ToList();
                    if (serverAccount.Count() > 0)
                    {
                        db.t_server_broker_account_rel.RemoveRange(serverAccount);
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
        /// 查询券商账户绑定前端账户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<BrokerAccountBindFrontAccountInfo> GetBrokerAccountBindFrontAccountList(DetailsPageRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_broker_account_info_frontaccount_rel
                             join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id
                             where item.BrokerAccountId==request.Id
                             select new { item, item2 };

                int totalCount = result.Count();

                return new PageRes<BrokerAccountBindFrontAccountInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in result
                            orderby item.item.CreateTime descending
                            select new BrokerAccountBindFrontAccountInfo
                            {
                                Status = item.item.Status,
                                AccountId = item.item.AccountId,
                                AccountMobile = item.item2.Mobile,
                                AccountNickName = item.item2.NickName,
                                CreateTime = item.item.CreateTime,
                                Id = item.item.Id
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加券商账户绑定前端账户
        /// </summary>
        /// <param name="request"></param>
        public void AddBrokerAccountBindFrontAccount(AddBrokerAccountBindFrontAccountRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断券商Id是否存在
                var brokerAccount = (from item in db.t_broker_account_info
                                     where item.Id == request.BrokerAccountId
                                     select item).FirstOrDefault();
                if (brokerAccount == null)
                {
                    throw new WebApiException(400,"券商账户不存在");
                }

                //判断前端账户是否存在
                var frontAccount = (from item in db.t_account_baseinfo
                                    where item.Id == request.FrontAccountId
                                    select item).FirstOrDefault();
                if (frontAccount == null)
                {
                    throw new WebApiException(400,"前端账户不存在");
                }

                //判断是否已经添加
                var rel = (from item in db.t_broker_account_info_frontaccount_rel
                           where item.BrokerAccountId == request.BrokerAccountId && item.AccountId == request.FrontAccountId
                           select item).FirstOrDefault();
                if (rel != null)
                {
                    throw new WebApiException(400,"账户已添加");
                }

                db.t_broker_account_info_frontaccount_rel.Add(new t_broker_account_info_frontaccount_rel 
                { 
                    Status=1,
                    AccountId=request.FrontAccountId,
                    BrokerAccountId=request.BrokerAccountId,
                    CreateTime=DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改券商账户绑定前端账户状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyBrokerAccountBindFrontAccountStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var rel = (from item in db.t_broker_account_info_frontaccount_rel
                           where item.Id==request.Id
                           select item).FirstOrDefault();
                if (rel == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                rel.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除券商账户绑定前端账户
        /// </summary>
        /// <param name="request"></param>
        public void DeleteBrokerAccountBindFrontAccount(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var rel = (from item in db.t_broker_account_info_frontaccount_rel
                           where item.Id == request.Id
                           select item).FirstOrDefault();
                if (rel == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_broker_account_info_frontaccount_rel.Remove(rel);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询券商账户持仓信息
        /// </summary>
        /// <returns></returns>
        public GetBrokerAccountPositionInfoRes GetBrokerAccountPositionInfo(GetBrokerAccountPositionInfoRequest request)
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var account = (from item in db.t_broker_account_info
                               where item.AccountCode == request.TradeAccountCode
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400, "无效的账户");
                }
                var capital = (from item in db.t_broker_account_info_capital
                               where item.AccountCode == request.TradeAccountCode
                               select item).FirstOrDefault();
                if (capital == null)
                {
                    return new GetBrokerAccountPositionInfoRes
                    {
                        MarketValue = 0,
                        TotalBalance = 0,
                        TotalValue = 0,
                        AvailableBalance = 0,
                        WithdrawBalance = 0,
                        FreezeBalance = 0,
                        SynchronizationStatus = (account.SynchronizationTime != null && account.SynchronizationTime.Value.AddSeconds(60) < timeNow) ? 0 : account.SynchronizationStatus,
                        PositionList = new PageRes<BrokerAccountPositionInfo>()
                    };
                }
                var position = from item in db.t_broker_account_info_position
                               where item.AccountCode == request.TradeAccountCode
                               select item;
                if (request.MaxId > 0)
                {
                    position = from item in position
                               where item.Id <= request.MaxId
                               select item;
                }
                int totalCount = position.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = position.Max(e => e.Id);
                }
                return new GetBrokerAccountPositionInfoRes
                {
                    MarketValue = capital.MarketValue,
                    TotalBalance = capital.TotalBalance,
                    TotalValue = capital.TotalValue,
                    AvailableBalance = capital.AvailableBalance,
                    WithdrawBalance = capital.WithdrawBalance,
                    FreezeBalance = capital.FreezeBalance,
                    SynchronizationStatus = (account.SynchronizationTime != null && account.SynchronizationTime.Value.AddSeconds(60) < timeNow) ? 0 : account.SynchronizationStatus,
                    PositionList = new PageRes<BrokerAccountPositionInfo>
                    {
                        MaxId = maxId,
                        TotalCount = totalCount,
                        List = (from item in position
                                orderby item.Id
                                select new BrokerAccountPositionInfo
                                {
                                    SellCountToday = item.SellCountToday,
                                    SharesName = item.SharesName,
                                    SharesCode = item.SharesCode,
                                    TotalSharesCount = item.TotalSharesCount,
                                    BuyCountToday = item.BuyCountToday,
                                    CanSoldSharesCount = item.CanSoldSharesCount,
                                    CostPrice = item.CostPrice,
                                    CurrPrice = item.CurrPrice,
                                    Id = item.Id,
                                    LastModified = item.LastModified,
                                    Market = item.Market,
                                    MarketValue = item.MarketValue,
                                    ProfitRate = item.ProfitRate,
                                    ProfitValue = item.ProfitValue
                                }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                    }
                };
            }
        }

        /// <summary>
        /// 同步券商账户持仓信息
        /// </summary>
        /// <param name="request"></param>
        public void UpdateBrokerAccountPositionInfo(UpdateBrokerAccountPositionInfoRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询账户所在服务器
                var account = (from item in db.t_broker_account_info
                               where item.AccountCode == request.TradeAccountCode
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400, "无效账户");
                }

                var sendData = new
                {
                    TradeAccountCode = request.TradeAccountCode
                };

                var server = (from item in db.t_server_broker_account_rel
                              where item.BrokerAccountId == account.Id
                              select item).FirstOrDefault();
                if (server == null)
                {
                    throw new WebApiException(400, "服务器配置有误");
                }

                bool isSendSuccess = Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesQuery", server.ServerId);
                if (!isSendSuccess)
                {
                    throw new WebApiException(400, "操作超时，请重新操作");
                }
                account.SynchronizationStatus = 1;
                account.SynchronizationTime = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询券商账户系统持仓信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<BrokerAccountSysPositionInfo> GetBrokerAccountSysPositionInfo(PageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var list = from item in db.t_broker_account_shares_rel
                           where item.TotalSharesCount > 0
                           group item by new { item.Market, item.SharesCode } into g
                           select g;
                int totalCount = list.Count();

                return new PageRes<BrokerAccountSysPositionInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in list
                            join item2 in db.t_shares_all on new { item.Key.Market, item.Key.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                            from ai in a.DefaultIfEmpty()
                            orderby item.Key.SharesCode
                            select new BrokerAccountSysPositionInfo
                            {
                                SharesCode = item.Key.SharesCode,
                                Market = item.Key.Market,
                                SharesName = ai == null ? "" : ai.SharesName,
                                CanSoldSharesCount = item.Sum(e => e.AccountCanSoldCount + e.SimulateCanSoldCount),
                                TotalSharesCount = item.Sum(e => e.TotalSharesCount)
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询券商账户系统持仓详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<BrokerAccountSysPositionDetails> GetBrokerAccountSysPositionDetails(GetBrokerAccountSysPositionDetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var sysHold = (from item in db.t_broker_account_shares_rel
                               where item.Market + "" + item.SharesCode == request.Code && item.SimulateSharesCount > 0
                               select item).ToList();

                var hold = (from item in db.t_account_shares_hold
                            join item2 in db.v_shares_quotes_last on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                            join item3 in db.t_account_baseinfo on item.AccountId equals item3.Id
                            where item.Market + "" + item.SharesCode == request.Code && item.RemainCount > 0
                            let PresentPrice = item2.PresentPrice <= 0 ? item2.ClosedPrice : item2.PresentPrice
                            let MarketValue = item.RemainCount * PresentPrice
                            let ProfitAmount = MarketValue - (item.BuyTotalAmount - item.SoldAmount)
                            let CostPrice = (item.BuyTotalAmount - item.SoldAmount) / item.RemainCount
                            select new BrokerAccountSysPositionDetails
                            {
                                CanSoldCount = item.CanSoldCount,
                                LastModified = item.LastModified,
                                TotalCount = item.RemainCount,
                                Id = item.Id,
                                AccountMobile = item3.Mobile,
                                AccountName = item3.NickName,
                                ProfitAmount = ProfitAmount,
                                CostPrice = CostPrice
                            }).ToList();
                if (sysHold.Count() > 0)
                {
                    hold.Add(new BrokerAccountSysPositionDetails
                    {
                        CanSoldCount = sysHold.Sum(e => e.SimulateCanSoldCount),
                        AccountMobile = "(系统持仓)",
                        AccountName = "",
                        Id = 0,
                        LastModified = DateTime.Now,
                        TotalCount = sysHold.Sum(e => e.SimulateSharesCount),
                    });
                }
                int totalCount = hold.Count();

                return new PageRes<BrokerAccountSysPositionDetails>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in hold
                            orderby item.LastModified descending
                            select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询系统持仓券商账户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<BrokerSysHoldAccountInfo> GetBrokerSysHoldAccountList(GetBrokerSysHoldAccountListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var list = (from item in db.t_broker_account_shares_rel
                            where item.Market + "" + item.SharesCode == request.Code && item.TotalSharesCount > 0
                            orderby item.AccountCanSoldCount descending
                            select new BrokerSysHoldAccountInfo
                            {
                                TradeAccountCode = item.TradeAccountCode,
                                AccountCanSoldCount = item.AccountCanSoldCount,
                                SimulateCanSoldCount = item.SimulateCanSoldCount
                            }).ToList();
                return list;
            }
        }

        /// <summary>
        /// 券商账户系统持仓详情-回收
        /// </summary>
        /// <param name="request"></param>
        public void RecoveryBrokerAccountSysPosition(RecoveryBrokerAccountSysPositionRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_TradeRecovery(request.HoldId, request.Code, request.Count, request.Price, errorCodeDb, errorMessageDb);
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
        /// 查询券商列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<BrokerInfo> GetBrokerList(GetBrokerListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var broker = from item in db.t_broker
                             select item;
                if (!string.IsNullOrEmpty(request.BrokerName))
                {
                    broker = from item in broker
                             where item.BrokerName.Contains(request.BrokerName)
                             select item;
                }
                if (request.MaxId > 0)
                {
                    broker = from item in broker
                             where item.Id <= request.MaxId
                             select item;
                }

                int totalCount = broker.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = broker.Max(e => e.Id);
                }

                return new PageRes<BrokerInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in broker
                            orderby item.CreateTime descending
                            select new BrokerInfo
                            {
                                BrokerCode = item.BrokerCode,
                                BrokerName = item.BrokerName,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                Version = item.Version,
                                Status = item.Status
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加券商
        /// </summary>
        public void AddBroker(AddBrokerRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断券商code是否存在
                var broker = (from item in db.t_broker
                              where item.BrokerCode == request.BrokerCode
                              select item).FirstOrDefault();
                if (broker != null)
                {
                    throw new WebApiException(400, "券商code已存在");
                }

                db.t_broker.Add(new t_broker
                {
                    BrokerCode = request.BrokerCode,
                    Status = 1,
                    BrokerName = request.BrokerName,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    Version = request.Version
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑券商
        /// </summary>
        public void ModifyBroker(ModifyBrokerRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var broker = (from item in db.t_broker
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (broker == null)
                {
                    throw new WebApiException(400, "数据部存爱");
                }

                //判断券商code是否存在
                var temp = (from item in db.t_broker
                            where item.BrokerCode == request.BrokerCode && item.Id != request.Id
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "券商code已存在");
                }


                broker.BrokerCode = request.BrokerCode;
                broker.BrokerName = request.BrokerName;
                broker.LastModified = DateTime.Now;
                broker.Version = request.Version;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改券商状态
        /// </summary>
        public void ModifyBrokerStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var broker = (from item in db.t_broker
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (broker == null)
                {
                    throw new WebApiException(400, "数据部存爱");
                }
                broker.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除券商
        /// </summary>
        /// <param name="request"></param>
        public void DeleteBroker(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var broker = (from item in db.t_broker
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (broker == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_broker.Remove(broker);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询券商营业部列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<BrokerDepartmentInfo> GetBrokerDepartmentList(GetBrokerDepartmentListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var department = from item in db.t_broker_department
                                 where item.BrokerCode == request.BrokerCode
                                 select item;
                if (!string.IsNullOrEmpty(request.DepartmentName))
                {
                    department = from item in department
                                 where item.DepartmentName.Contains(request.DepartmentName)
                                 select item;
                }
                if (request.MaxId > 0)
                {
                    department = from item in department
                                 where item.Id <= request.MaxId
                                 select item;
                }
                int totalCount = department.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = department.Max(e => e.Id);
                }

                return new PageRes<BrokerDepartmentInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in department
                            join item2 in db.t_broker on item.BrokerCode equals item2.BrokerCode
                            orderby item.CreateTime descending
                            select new BrokerDepartmentInfo
                            {
                                BrokerCode = item.BrokerCode,
                                Status = item.Status,
                                BrokerName = item2.BrokerName,
                                CreateTime = item.CreateTime,
                                DepartmentCode = item.DepartmentCode,
                                DepartmentName = item.DepartmentName,
                                Id = item.Id
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加券商营业部
        /// </summary>
        public void AddBrokerDepartment(AddBrokerDepartmentRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断券商是否存在
                var broker = (from item in db.t_broker
                              where item.BrokerCode == request.BrokerCode
                              select item).FirstOrDefault();
                if (broker == null)
                {
                    throw new WebApiException(400, "券商不存在");
                }
                //判断营业部代码是否存在
                var department = (from item in db.t_broker_department
                                  where item.BrokerCode == request.BrokerCode && item.DepartmentCode == request.DepartmentCode
                                  select item).FirstOrDefault();
                if (department != null)
                {
                    throw new WebApiException(400, "营业部代码已存在");
                }

                db.t_broker_department.Add(new t_broker_department
                {
                    Status = 1,
                    BrokerCode = request.BrokerCode,
                    CreateTime = DateTime.Now,
                    DepartmentCode = request.DepartmentCode,
                    DepartmentName = request.DepartmentName,
                    LastModified = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑券商营业部
        /// </summary>
        public void ModifyBrokerDepartment(ModifyBrokerDepartmentRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var department = (from item in db.t_broker_department
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (department == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                //判断营业部代码是否存在
                var temp = (from item in db.t_broker_department
                            where item.BrokerCode == department.BrokerCode && item.DepartmentCode == request.DepartmentCode && item.Id != request.Id
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "营业部代码已存在");
                }
                department.DepartmentCode = request.DepartmentCode;
                department.DepartmentName = request.DepartmentName;
                department.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改券商营业部状态
        /// </summary>
        public void ModifyBrokerDepartmentStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var department = (from item in db.t_broker_department
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (department == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                department.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除券商营业部
        /// </summary>
        public void DeleteBrokerDepartment(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var department = (from item in db.t_broker_department
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (department == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_broker_department.Remove(department);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询券商交易服务器列表
        /// </summary>
        /// <returns></returns>
        public PageRes<BrokerTradeHostInfo> GetBrokerTradeHostList(GetBrokerTradeHostListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var host = from item in db.t_broker_host
                           where item.BrokerCode == request.BrokerCode
                           select item;
                if (!string.IsNullOrEmpty(request.Ip))
                {
                    host = from item in host
                           where item.IpAddress.Contains(request.Ip)
                           select item;
                }
                if (request.MaxId > 0)
                {
                    host = from item in host
                           where item.Id <= request.MaxId
                           select item;
                }
                int totalCount = host.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = host.Max(e => e.Id);
                }

                return new PageRes<BrokerTradeHostInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in host
                            join item2 in db.t_broker on item.BrokerCode equals item2.BrokerCode
                            orderby item.CreateTime descending
                            select new BrokerTradeHostInfo
                            {
                                BrokerCode = item.BrokerCode,
                                Status = item.Status,
                                BrokerName = item2.BrokerName,
                                CreateTime = item.CreateTime,
                                Ip = item.IpAddress,
                                Port = item.Port,
                                Id = item.Id
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加券商交易服务器
        /// </summary>
        public void AddBrokerTradeHost(AddBrokerTradeHostRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断券商是否存在
                var broker = (from item in db.t_broker
                              where item.BrokerCode == request.BrokerCode
                              select item).FirstOrDefault();
                if (broker == null)
                {
                    throw new WebApiException(400, "券商不存在");
                }
                //判断地址是否存在
                var host = (from item in db.t_broker_host
                            where item.BrokerCode == request.BrokerCode && item.IpAddress == request.Ip
                            select item).FirstOrDefault();
                if (host != null)
                {
                    throw new WebApiException(400, "Ip地址已存在");
                }

                db.t_broker_host.Add(new t_broker_host
                {
                    Status = 1,
                    BrokerCode = request.BrokerCode,
                    CreateTime = DateTime.Now,
                    IpAddress = request.Ip,
                    Port = request.Port,
                    LastModified = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑券商交易服务器
        /// </summary>
        public void ModifyBrokerTradeHost(ModifyBrokerTradeHostRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var host = (from item in db.t_broker_host
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (host == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                //判断地址是否存在
                var temp = (from item in db.t_broker_host
                            where item.BrokerCode == host.BrokerCode && item.IpAddress == request.Ip && item.Id != request.Id
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "Ip地址已存在");
                }
                host.IpAddress = request.Ip;
                host.Port = request.Port;
                host.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改券商交易服务器状态
        /// </summary>
        public void ModifyBrokerTradeHostStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var host = (from item in db.t_broker_host
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (host == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                host.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除券商交易服务器
        /// </summary>
        public void DeleteBrokerTradeHost(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var host = (from item in db.t_broker_host
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (host == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                db.t_broker_host.Remove(host);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询行情服务器列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<BrokerHqHostInfo> GetBrokerHqHostList(GetBrokerHqHostListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var host = from item in db.t_shares_hq_host
                           select item;
                if (!string.IsNullOrEmpty(request.Ip))
                {
                    host = from item in host
                           where item.IpAddress.Contains(request.Ip)
                           select item;
                }
                if (request.MaxId > 0)
                {
                    host = from item in host
                           where item.Id <= request.MaxId
                           select item;
                }
                int totalCount = host.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = host.Max(e => e.Id);
                }
                return new PageRes<BrokerHqHostInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in host
                            orderby item.CreateTime descending
                            select new BrokerHqHostInfo
                            {
                                Status = item.Status,
                                BrokerName = item.BrokerName,
                                CreateTime = item.CreateTime,
                                IpAddress = item.IpAddress,
                                Port = item.Port,
                                Id = item.Id
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加行情服务器
        /// </summary>
        /// <param name="request"></param>
        public void AddBrokerHqHost(AddBrokerHqHostRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断地址是否存在
                var host = (from item in db.t_shares_hq_host
                            where item.IpAddress == request.IpAddress
                            select item).FirstOrDefault();
                if (host != null)
                {
                    throw new WebApiException(400, "Ip地址已存在");
                }

                db.t_shares_hq_host.Add(new t_shares_hq_host
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    IpAddress = request.IpAddress,
                    BrokerName = request.BrokerName,
                    Port = request.Port,
                    LastModified = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑行情服务器
        /// </summary>
        /// <param name="request"></param>
        public void ModifyBrokerHqHost(ModifyBrokerHqHostRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var host = (from item in db.t_shares_hq_host
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (host == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                var temp = (from item in db.t_shares_hq_host
                            where item.Id != request.Id && item.IpAddress == request.IpAddress
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "Ip地址已存在");
                }

                host.IpAddress = request.IpAddress;
                host.LastModified = DateTime.Now;
                host.Port = request.Port;
                host.BrokerName = request.BrokerName;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改行情服务器状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyBrokerHqHostStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var host = (from item in db.t_shares_hq_host
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (host == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                host.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除行情服务器
        /// </summary>
        /// <param name="request"></param>
        public void DeleteBrokerHqHost(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var host = (from item in db.t_shares_hq_host
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (host == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                db.t_shares_hq_host.Remove(host);
                db.SaveChanges();
            }
        }
        #endregion

        #region====支付管理====
        /// <summary>
        /// 查询支付渠道分组
        /// </summary>
        /// <returns></returns>
        public List<SysPaymentChannelGroupInfo> GetSysPaymentChannelGroup()
        {
            using (var db = new meal_ticketEntities())
            {
                var paygroup = (from item in db.t_payment_channel_group
                                select new SysPaymentChannelGroupInfo
                                {
                                    Description = item.Description,
                                    Name = item.Name,
                                    Tag = item.Tag,
                                    Type = item.Type,
                                    Status=item.Status,
                                    CreateTime=item.LastModified,
                                    Id=item.Id
                                }).ToList();
                return paygroup;
            }
        }

        /// <summary>
        /// 编辑支付渠道分组
        /// </summary>
        public void ModifySysPaymentChannelGroup(ModifySysPaymentChannelGroupRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var paygroup = (from item in db.t_payment_channel_group
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (paygroup == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                paygroup.Description = request.Description;
                paygroup.LastModified = DateTime.Now;
                paygroup.Name = request.Name;
                paygroup.Tag = request.Tag;
                paygroup.Type = request.Type;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改支付渠道分组状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySysPaymentChannelGroupStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var paygroup = (from item in db.t_payment_channel_group
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (paygroup == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                paygroup.Status = request.Status;
                paygroup.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询支付渠道列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SysPaymentChannelInfo> GetSysPaymentChannelList(GetSysPaymentChannelListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var paymentChannel = from item in db.t_payment_channel
                                     select item;
                if (!string.IsNullOrEmpty(request.ChannelCode))
                {
                    paymentChannel = from item in paymentChannel
                                     where item.ChannelCode == request.ChannelCode
                                     select item;
                }
                if (request.Status != 0)
                {
                    paymentChannel = from item in paymentChannel
                                     where item.Status == request.Status
                                     select item;
                }
                if (request.PayType != 0)
                {
                    paymentChannel = from item in paymentChannel
                                     where item.Type == request.PayType
                                     select item;
                }
                int totalCount = paymentChannel.Count();
                return new PageRes<SysPaymentChannelInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in paymentChannel
                            orderby item.CreateTime descending
                            select new SysPaymentChannelInfo
                            {
                                SupportRefund = item.SupportRefund,
                                BusinessCode = item.BusinessCode,
                                Status = item.Status,
                                BusinessName = item.BusinessName,
                                ChannelCode = item.ChannelCode,
                                ChannelDescription = item.ChannelDescription,
                                ChannelName = item.ChannelName,
                                CreateTime = item.LastModified,
                                OrderIndex = item.OrderIndex,
                                Type = item.Type,
                                PaymentAccountCount = (from x in db.t_payment_channel_account_rel
                                                       join x2 in db.t_payment_account on x.PaymentAccountId equals x2.Id
                                                       where x.ChannelCode == item.ChannelCode && x.BusinessCode == item.BusinessCode
                                                       select x).Count()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()

                };
            }
        }

        /// <summary>
        /// 编辑支付渠道排序值
        /// </summary>
        /// <param name="request"></param>
        public void ModifySysPaymentChannelOrderIndex(ModifySysPaymentChannelOrderIndexRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var paymentChannel = (from item in db.t_payment_channel
                                      where item.ChannelCode == request.ChannelCode && item.BusinessCode == request.BusinessCode
                                      select item).FirstOrDefault();
                if (paymentChannel == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                paymentChannel.OrderIndex = request.OrderIndex;
                paymentChannel.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改支付渠道状态
        /// </summary>
        public void ModifySysPaymentChannelStatus(ModifySysPaymentChannelStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var paymentChannel = (from item in db.t_payment_channel
                                      where item.ChannelCode == request.ChannelCode && item.BusinessCode == request.BusinessCode
                                      select item).FirstOrDefault();
                if (paymentChannel == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                paymentChannel.Status = request.Status;
                paymentChannel.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 绑定支付渠道账户
        /// </summary>
        /// <param name="request"></param>
        public void BindSysPaymentChannelAccount(BindSysPaymentChannelAccountRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var rel = (from item in db.t_payment_channel_account_rel
                               where item.ChannelCode == request.ChannelCode && item.BusinessCode == request.BusinessCode
                               select item).ToList();
                    if (rel.Count() > 0)
                    {
                        db.t_payment_channel_account_rel.RemoveRange(rel);
                        db.SaveChanges();
                    }

                    foreach (var accountId in request.PaymentAccountIdList)
                    {
                        db.t_payment_channel_account_rel.Add(new t_payment_channel_account_rel
                        {
                            BusinessCode = request.BusinessCode,
                            ChannelCode = request.ChannelCode,
                            PaymentAccountId = accountId
                        });
                    }
                    if (request.PaymentAccountIdList.Count() > 0)
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
        /// 查询打款渠道列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SysPaymentCashChannelInfo> GetSysPaymentCashChannelList(GetSysPaymentCashChannelListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var paymentCashChannel = from item in db.t_payment_cash_channel
                                         select item;
                if (!string.IsNullOrEmpty(request.ChannelCode))
                {
                    paymentCashChannel = from item in paymentCashChannel
                                         where item.ChannelCode == request.ChannelCode
                                         select item;
                }
                if (request.Status != 0)
                {
                    paymentCashChannel = from item in paymentCashChannel
                                         where item.Status == request.Status
                                         select item;
                }
                int totalCount = paymentCashChannel.Count();
                return new PageRes<SysPaymentCashChannelInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in paymentCashChannel
                            orderby item.CreateTime descending
                            select new SysPaymentCashChannelInfo
                            {
                                BusinessCode = item.BusinessCode,
                                Status = item.Status,
                                BusinessName = item.BusinessName,
                                ChannelCode = item.ChannelCode,
                                ChannelDescription = item.ChannelDescription,
                                ChannelName = item.ChannelName,
                                CreateTime = item.LastModified,
                                Type = item.Type,
                                PaymentCashRefundAccountCount = (from x in db.t_payment_cash_channel_refund_account_rel
                                                                 join x2 in db.t_payment_account on x.PaymentAccountId equals x2.Id
                                                                 where x.ChannelCode == item.ChannelCode && x.BusinessCode == item.BusinessCode
                                                                 select x).Count(),
                                PaymentCashTransferAccountCount = (from x in db.t_payment_cash_channel_transfer_account_rel
                                                                   join x2 in db.t_payment_account on x.PaymentAccountId equals x2.Id
                                                                   where x.ChannelCode == item.ChannelCode && x.BusinessCode == item.BusinessCode
                                                                   select x).Count()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 修改打款渠道状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySysPaymentCashChannelStatus(ModifySysPaymentCashChannelStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var paymentCashChannel = (from item in db.t_payment_cash_channel
                                          where item.ChannelCode == request.ChannelCode && item.BusinessCode == request.BusinessCode
                                          select item).FirstOrDefault();
                if (paymentCashChannel == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                paymentCashChannel.Status = request.Status;
                paymentCashChannel.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 绑定打款渠道退款账户
        /// </summary>
        /// <param name="request"></param>
        public void BindSysPaymentCashChannelRefundAccount(BindSysPaymentCashChannelRefundAccountRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var rel = (from item in db.t_payment_cash_channel_refund_account_rel
                               where item.ChannelCode == request.ChannelCode && item.BusinessCode == request.BusinessCode
                               select item).ToList();
                    if (rel.Count() > 0)
                    {
                        db.t_payment_cash_channel_refund_account_rel.RemoveRange(rel);
                        db.SaveChanges();
                    }

                    foreach (var accountId in request.PaymentAccountIdList)
                    {
                        db.t_payment_cash_channel_refund_account_rel.Add(new t_payment_cash_channel_refund_account_rel
                        {
                            BusinessCode = request.BusinessCode,
                            ChannelCode = request.ChannelCode,
                            PaymentAccountId = accountId
                        });
                    }
                    if (request.PaymentAccountIdList.Count() > 0)
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
        /// 绑定打款渠道转账账户
        /// </summary>
        /// <param name="request"></param>
        public void BindSysPaymentCashChannelTransferAccount(BindSysPaymentCashChannelTransferAccountRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var rel = (from item in db.t_payment_cash_channel_transfer_account_rel
                               where item.ChannelCode == request.ChannelCode && item.BusinessCode == request.BusinessCode
                               select item).ToList();
                    if (rel.Count() > 0)
                    {
                        db.t_payment_cash_channel_transfer_account_rel.RemoveRange(rel);
                        db.SaveChanges();
                    }

                    foreach (var accountId in request.PaymentAccountIdList)
                    {
                        db.t_payment_cash_channel_transfer_account_rel.Add(new t_payment_cash_channel_transfer_account_rel
                        {
                            BusinessCode = request.BusinessCode,
                            ChannelCode = request.ChannelCode,
                            PaymentAccountId = accountId
                        });
                    }
                    if (request.PaymentAccountIdList.Count() > 0)
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
        /// 查询支付账户列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SysPaymentAccountInfo> GetSysPaymentAccountList(GetSysPaymentAccountListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var paymentAccount = from item in db.t_payment_account
                                     select item;
                if (!string.IsNullOrEmpty(request.ChannelCode))
                {
                    paymentAccount = from item in paymentAccount
                                     where item.ChannelCode == request.ChannelCode
                                     select item;
                }
                if (request.MaxId > 0)
                {
                    paymentAccount = from item in paymentAccount
                                     where item.Id <= request.MaxId
                                     select item;
                }

                int totalCount = paymentAccount.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = paymentAccount.Max(e => e.Id);
                }

                List<long> CheckedList = new List<long>();
                if (request.Type == 1)
                {
                    CheckedList = (from item in db.t_payment_channel_account_rel
                                   where item.ChannelCode == request.ChannelCode && item.BusinessCode == request.BusinessCode
                                   select item.PaymentAccountId).ToList();
                }
                if (request.Type == 2)
                {
                    CheckedList = (from item in db.t_payment_cash_channel_refund_account_rel
                                   where item.ChannelCode == request.ChannelCode && item.BusinessCode == request.BusinessCode
                                   select item.PaymentAccountId).ToList();
                }
                if (request.Type == 3)
                {
                    CheckedList = (from item in db.t_payment_cash_channel_transfer_account_rel
                                   where item.ChannelCode == request.ChannelCode && item.BusinessCode == request.BusinessCode
                                   select item.PaymentAccountId).ToList();
                }

                return new PageRes<SysPaymentAccountInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in paymentAccount
                            orderby item.CreateTime descending
                            select new SysPaymentAccountInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ChannelCode = item.ChannelCode,
                                Name = item.Name,
                                IsChecked = CheckedList.Contains(item.Id) ? true : false
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加支付账户
        /// </summary>
        public void AddSysPaymentAccount(AddSysPaymentAccountRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var paymentAccount = new t_payment_account
                    {
                        Status = 1,
                        ChannelCode = request.ChannelCode,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        Name = request.Name
                    };
                    db.t_payment_account.Add(paymentAccount);
                    db.SaveChanges();

                    if (request.ChannelCode == "Alipay")
                    {
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "app_id",
                            SettingValue = request.AppId,
                            CreateTime = DateTime.Now,
                            Description = "app_id",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "seller_id",
                            SettingValue = request.SellerId,
                            CreateTime = DateTime.Now,
                            Description = "seller_id",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "alipay_public_key",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "disable_pay_channels",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "enable_pay_channels",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "notify_url",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "rsaprivatekey",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.SaveChanges();
                    }
                    if (request.ChannelCode == "WeChat")
                    {
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "appid",
                            SettingValue = request.AppId,
                            CreateTime = DateTime.Now,
                            Description = "appid",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "mch_id",
                            SettingValue = request.SellerId,
                            CreateTime = DateTime.Now,
                            Description = "mch_id",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "key",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "limit_pay",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "notify_url",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "SSlCertPassword",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "SSlCertPath",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.SaveChanges();
                    }
                    if (request.ChannelCode == "BankCard")
                    {
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "AccountName",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "账户名",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "Bank",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "开户行",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "CardNumber",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "卡号",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "Mobile",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "手机号",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
                        db.t_payment_account_settings.Add(new t_payment_account_settings
                        {
                            SettingKey = "Other",
                            SettingValue = "",
                            CreateTime = DateTime.Now,
                            Description = "其他说明",
                            PaymentAccountId = paymentAccount.Id,
                            LastModified = DateTime.Now
                        });
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
        /// 编辑支付账户
        /// </summary>
        public void ModifySysPaymentAccount(ModifySysPaymentAccountRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var paymentAccount = (from item in db.t_payment_account
                                      where item.Id == request.Id
                                      select item).FirstOrDefault();
                if (paymentAccount == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                paymentAccount.Name = request.Name;
                paymentAccount.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改支付账户状态
        /// </summary>
        public void ModifySysPaymentAccountStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var paymentAccount = (from item in db.t_payment_account
                                      where item.Id == request.Id
                                      select item).FirstOrDefault();
                if (paymentAccount == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                paymentAccount.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除支付账户
        /// </summary>
        public void DeleteSysPaymentAccount(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var paymentAccount = (from item in db.t_payment_account
                                      where item.Id == request.Id
                                      select item).FirstOrDefault();
                if (paymentAccount == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_payment_account.Remove(paymentAccount);

                var setting = (from item in db.t_payment_account_settings
                               where item.PaymentAccountId == request.Id
                               select item).ToList();
                db.t_payment_account_settings.RemoveRange(setting);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询支付参数信息
        /// </summary>
        /// <returns></returns>
        public List<SysPaymentParInfo> GetSysPaymentParList(GetSysPaymentParListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var par = (from item in db.t_payment_account_settings
                           where item.PaymentAccountId == request.PaymentAccountId
                           orderby item.SettingKey
                           select new SysPaymentParInfo
                           {
                               ParDescription = item.Description,
                               LastModified = item.LastModified,
                               ParName = item.SettingKey,
                               ParValue = item.SettingValue
                           }).ToList();
                return par;
            }
        }

        /// <summary>
        /// 修改支付参数信息
        /// </summary>
        /// <param name="request"></param>
        public void ModifySysPaymentPar(ModifySysPaymentParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询渠道信息
                var paymentAccount = (from item in db.t_payment_account
                                      where item.Id == request.PaymentAccountId
                                      select item).FirstOrDefault();
                if (paymentAccount == null)
                {
                    throw new WebApiException(400, "支付账户信息有误");
                }
                if (paymentAccount.Status == 1)
                {
                    throw new WebApiException(400, "修改参数必须先关闭该支付账户");
                }

                var par = (from item in db.t_payment_account_settings
                           where item.PaymentAccountId == request.PaymentAccountId && item.SettingKey == request.ParName
                           select item).FirstOrDefault();
                if (par == null)
                {
                    throw new WebApiException(400, "参数不存在");
                }
                if (paymentAccount.ChannelCode == "Alipay")
                {
                    if (request.ParName == "app_id")
                    {
                        throw new WebApiException(400, "app_id不能修改");
                    }
                    if (request.ParName == "seller_id")
                    {
                        throw new WebApiException(400, "seller_id不能修改");
                    }
                }
                if (paymentAccount.ChannelCode == "WeChat")
                {
                    if (request.ParName == "appid")
                    {
                        throw new WebApiException(400, "appid不能修改");
                    }
                    if (request.ParName == "mch_id")
                    {
                        throw new WebApiException(400, "mch_id不能修改");
                    }
                }
                par.SettingValue = request.ParValue;
                par.Description = request.ParDescription;
                par.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询商品列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SysGoodsInfo> GetSysGoodsList(GetSysGoodsListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var goods = from item in db.t_goods
                            select item;
                if (request.GoodsType != 0)
                {
                    goods = from item in goods
                            where item.GoodsType == request.GoodsType
                            select item;
                }

                int totalCount = goods.Count();

                return new PageRes<SysGoodsInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in goods
                            orderby item.CreateTime descending
                            select new SysGoodsInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                GoodsDescription = item.GoodsDescription,
                                GoodsDetails = item.GoodsDetails,
                                GoodsName = item.GoodsName,
                                GoodsType = item.GoodsType,
                                Id = item.GoodsId,
                                OrderIndex = item.OrderIndex,
                                ImgUrl=item.ImgUrl
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="request"></param>
        public void AddSysGoods(AddSysGoodsRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_goods.Add(new t_goods 
                {
                    Status=1,
                    CreateTime=DateTime.Now,
                    GoodsDescription=request.GoodsDescription,
                    GoodsDetails=request.GoodsDetails,
                    GoodsName=request.GoodsName,
                    GoodsType=request.GoodsType,
                    LastModified=DateTime.Now,
                    ImgUrl=request.ImgUrl,
                    OrderIndex=request.OrderIndex
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑商品
        /// </summary>
        /// <param name="request"></param>
        public void ModifySysGoods(ModifySysGoodsRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var goods = (from item in db.t_goods
                             where item.GoodsId == request.Id
                             select item).FirstOrDefault();
                if (goods == null)
                {
                    throw new WebApiException(400,"商品不存在");
                }
                goods.GoodsDescription = request.GoodsDescription;
                goods.GoodsDetails = request.GoodsDetails;
                goods.GoodsName = request.GoodsName;
                goods.GoodsType = request.GoodsType;
                goods.LastModified = DateTime.Now;
                goods.ImgUrl = request.ImgUrl;
                goods.OrderIndex = request.OrderIndex;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改商品状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySysGoodsStatus(ModifyStatusRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var goods = (from item in db.t_goods
                             where item.GoodsId == request.Id
                             select item).FirstOrDefault();
                if (goods == null)
                {
                    throw new WebApiException(400, "商品不存在");
                }
                goods.LastModified = DateTime.Now;
                goods.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSysGoods(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var goods = (from item in db.t_goods
                             where item.GoodsId == request.Id
                             select item).FirstOrDefault();
                if (goods == null)
                {
                    throw new WebApiException(400, "商品不存在");
                }
                db.t_goods.Remove(goods);
                db.SaveChanges();
            }
        }
        #endregion

        #region====短信管理====
        /// <summary>
        /// 查询短信渠道列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SmsChannelInfo> GetSmsChannelList(GetSmsChannelListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var channelList = from item in db.t_sms_channel
                                  select item;
                if (request.Status != 0)
                {
                    channelList = from item in channelList
                                  where item.Status == request.Status
                                  select item;
                }

                int totalCount = channelList.Count();

                return new PageRes<SmsChannelInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in channelList
                            orderby item.CreateTime descending
                            select new SmsChannelInfo
                            {
                                ChannelCode = item.ChannelCode,
                                ChannelName = item.ChannelName,
                                Status = item.Status,
                                CreateTime = item.LastModified,
                                AppCount = (from x in db.t_sms_channel_app
                                            where x.ChannelCode == item.ChannelCode
                                            select x).Count()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 修改短信渠道状态
        /// </summary>
        public void ModifySmsChannelStatus(ModifySmsChannelStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var channel = (from item in db.t_sms_channel
                               where item.ChannelCode == request.ChannelCode
                               select item).FirstOrDefault();
                if (channel == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                channel.Status = request.Status;
                channel.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询短信渠道app列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SmsChannelAppInfo> GetSmsChannelAppList(GetSmsChannelAppListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var app = from item in db.t_sms_channel_app
                          where item.ChannelCode == request.ChannelCode
                          select item;
                if (request.Status != 0)
                {
                    app = from item in app
                          where item.Status == request.Status
                          select item;
                }
                int totalCount = app.Count();

                return new PageRes<SmsChannelAppInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in app
                            orderby item.CreateTime descending
                            select new SmsChannelAppInfo
                            {
                                Id = item.Id,
                                AppKey = item.AppKey,
                                AppSecret = item.AppSecret,
                                AppName = item.AppName,
                                Status = item.Status,
                                CreateTime = item.CreateTime
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加短信渠道app
        /// </summary>
        /// <param name="request"></param>
        public void AddSmsChannelApp(AddSmsChannelAppRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断渠道是否存在
                var channel = (from item in db.t_sms_channel
                               where item.ChannelCode == request.ChannelCode && item.Status == 1
                               select item).FirstOrDefault();
                if (channel == null)
                {
                    throw new WebApiException(400, "无效的渠道");
                }

                //判断appKey是否存在
                var app = (from item in db.t_sms_channel_app
                           where item.ChannelCode == request.ChannelCode && item.AppName == request.AppName
                           select item).FirstOrDefault();
                if (app != null)
                {
                    throw new WebApiException(400, "应用名称已存在");
                }
                app = (from item in db.t_sms_channel_app
                       where item.ChannelCode == request.ChannelCode && item.AppKey == request.AppKey
                       select item).FirstOrDefault();
                if (app != null)
                {
                    throw new WebApiException(400, "AppKey已存在");
                }

                db.t_sms_channel_app.Add(new t_sms_channel_app
                {
                    Status = 1,
                    AppSecret = request.AppSecret,
                    AppKey = request.AppKey,
                    AppName = request.AppName,
                    ChannelCode = request.ChannelCode,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑短信渠道app
        /// </summary>
        /// <param name="request"></param>
        public void ModifySmsChannelApp(ModifySmsChannelAppRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var app = (from item in db.t_sms_channel_app
                           where item.Id == request.Id
                           select item).FirstOrDefault();
                if (app == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                //判断appKey是否存在
                var appTemp = (from item in db.t_sms_channel_app
                               where item.Id != request.Id && item.AppName == request.AppName
                               select item).FirstOrDefault();
                if (appTemp != null)
                {
                    throw new WebApiException(400, "应用名称已存在");
                }
                appTemp = (from item in db.t_sms_channel_app
                           where item.Id != request.Id && item.AppKey == request.AppKey
                           select item).FirstOrDefault();
                if (appTemp != null)
                {
                    throw new WebApiException(400, "AppKey已存在");
                }

                app.LastModified = DateTime.Now;
                app.AppKey = request.AppKey;
                app.AppSecret = request.AppSecret;
                app.AppName = request.AppName;
                db.SaveChanges();

            }
        }

        /// <summary>
        /// 修改短信渠道app状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySmsChannelAppStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var app = (from item in db.t_sms_channel_app
                           where item.Id == request.Id
                           select item).FirstOrDefault();
                if (app == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                app.LastModified = DateTime.Now;
                app.Status = request.Status;
                db.SaveChanges();

            }
        }

        /// <summary>
        /// 删除短信渠道app
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSmsChannelApp(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var app = (from item in db.t_sms_channel_app
                           where item.Id == request.Id
                           select item).FirstOrDefault();
                if (app == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sms_channel_app.Remove(app);
                db.SaveChanges();

            }
        }

        /// <summary>
        /// 查询短信签名列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SmsSignInfo> GetSmsSignList(GetSmsSignListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var sign = from item in db.t_sms_sign
                           select item;
                if (!string.IsNullOrEmpty(request.ChannelCode))
                {
                    sign = from item in sign
                           where item.ChannelCode == request.ChannelCode
                           select item;
                }
                if (!string.IsNullOrEmpty(request.AppKey))
                {
                    sign = from item in sign
                           where item.AppKey == request.AppKey
                           select item;
                }
                if (request.Status != 0)
                {
                    sign = from item in sign
                           where item.Status == request.Status
                           select item;
                }
                if (request.ExamineStatus != 0)
                {
                    sign = from item in sign
                           where item.ExamineStatus == request.ExamineStatus
                           select item;
                }

                int totalCount = sign.Count();

                return new PageRes<SmsSignInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in sign
                            join item2 in db.t_sms_channel on item.ChannelCode equals item2.ChannelCode into a
                            from ai in a.DefaultIfEmpty()
                            join item3 in db.t_sms_channel_app on new { item.ChannelCode, item.AppKey } equals new { item3.ChannelCode, item3.AppKey } into b
                            from bi in b.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new SmsSignInfo
                            {
                                SignName = item.SignName,
                                Status = item.Status,
                                LicenceUrl = item.LicenceUrl,
                                ThirdSignId = item.ThirdSignId,
                                ExamineStatus = item.ExamineStatus,
                                ChannelCode = item.ChannelCode,
                                Id = item.Id,
                                CreateTime = item.CreateTime,
                                ExamineFailReason = item.ExamineFailReason,
                                AppName = bi == null ? "" : bi.AppName,
                                ChannelName = ai == null ? "" : ai.ChannelName,
                                AppKey = item.AppKey,
                                ApplyTime = item.ApplyTime,
                                ExamineTime = item.ExamineTime
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加短信签名
        /// </summary>
        /// <param name="request"></param>
        public void AddSmsSign(AddSmsSignRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断渠道是否存在
                var channel = (from item in db.t_sms_channel
                               where item.ChannelCode == request.ChannelCode && item.Status == 1
                               select item).FirstOrDefault();
                if (channel == null)
                {
                    throw new WebApiException(400, "无效的渠道");
                }
                //判断应用是否存在
                var app = (from item in db.t_sms_channel_app
                           where item.ChannelCode == request.ChannelCode && item.AppKey == request.AppKey && item.Status == 1
                           select item).FirstOrDefault();
                if (app == null)
                {
                    throw new WebApiException(400, "无效的应用");
                }
                //判断签名是否存在
                var sign = (from item in db.t_sms_sign
                            where item.ChannelCode == request.ChannelCode && item.AppKey == request.AppKey && item.SignName == request.SignName
                            select item).FirstOrDefault();
                if (sign != null)
                {
                    throw new WebApiException(400, "签名已存在");
                }

                db.t_sms_sign.Add(new t_sms_sign
                {
                    SignName = request.SignName,
                    Status = 1,
                    ExamineStatus = 1,
                    ThirdSignId = "",
                    AppKey = request.AppKey,
                    ChannelCode = request.ChannelCode,
                    CreateTime = DateTime.Now,
                    ExamineFailReason = "",
                    LastModified = DateTime.Now,
                    LicenceUrl = request.LicenceUrl,
                    ApplyTime = null,
                    ExamineTime = null
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑短信签名
        /// </summary>
        /// <param name="request"></param>
        public void ModifySmsSign(ModifySmsSignRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var sign = (from item in db.t_sms_sign
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                    if (sign == null)
                    {
                        throw new WebApiException(400, "签名不存在");
                    }
                    if (sign.ExamineStatus != 1 && sign.ExamineStatus != 4)
                    {
                        throw new WebApiException(400, "当前审核状态无法修改");
                    }
                    //判断渠道是否存在
                    var channel = (from item in db.t_sms_channel
                                   where item.ChannelCode == request.ChannelCode && item.Status == 1
                                   select item).FirstOrDefault();
                    if (channel == null)
                    {
                        throw new WebApiException(400, "无效的渠道");
                    }
                    //判断应用是否存在
                    var app = (from item in db.t_sms_channel_app
                               where item.ChannelCode == request.ChannelCode && item.AppKey == request.AppKey && item.Status == 1
                               select item).FirstOrDefault();
                    if (app == null)
                    {
                        throw new WebApiException(400, "无效的应用");
                    }
                    //判断签名是否存在
                    var signTemp = (from item in db.t_sms_sign
                                    where item.ChannelCode == request.ChannelCode && item.AppKey == request.AppKey && item.SignName == request.SignName && item.Id != request.Id
                                    select item).FirstOrDefault();
                    if (signTemp != null)
                    {
                        throw new WebApiException(400, "签名已存在");
                    }
                    string sourceChannelCode = sign.ChannelCode;
                    string sourceAppKey = sign.AppKey;
                    string sourceAppSecret = string.Empty;
                    var tempApp = (from item in db.t_sms_channel_app
                                   where item.ChannelCode == sourceChannelCode && item.AppKey == sourceAppKey
                                   select item).FirstOrDefault();
                    if (tempApp != null)
                    {
                        sourceAppSecret = tempApp.AppSecret;
                    }
                    string sourceThirdSignId = sign.ThirdSignId;

                    sign.AppKey = request.AppKey;
                    sign.ChannelCode = request.ChannelCode;
                    sign.ExamineFailReason = "";
                    sign.ExamineStatus = 1;
                    sign.LastModified = DateTime.Now;
                    sign.LicenceUrl = request.LicenceUrl;
                    sign.SignName = request.SignName;

                    if (sourceChannelCode == request.ChannelCode && sourceAppKey == request.AppKey)
                    {
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(sourceThirdSignId) && !string.IsNullOrEmpty(sourceAppSecret))
                        {
                            //删除原渠道签名
                            ThirdSmsBase smsObj = new ThirdSmsBase(sourceChannelCode, sourceAppKey, sourceAppSecret);
                            var thirdSms = smsObj.GetThirdSmsObj();
                            thirdSms.DeleteSmsSign(sourceThirdSignId);
                        }
                        sign.ThirdSignId = "";
                    }
                    db.SaveChanges();

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
        /// 修改短信签名状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySmsSignStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var sign = (from item in db.t_sms_sign
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (sign == null)
                {
                    throw new WebApiException(400, "签名不存在");
                }
                sign.Status = request.Status;
                sign.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除短信签名
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSmsSign(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var sign = (from item in db.t_sms_sign
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (sign == null)
                {
                    throw new WebApiException(400, "签名不存在");
                }
                db.t_sms_sign.Remove(sign);
                db.SaveChanges();

                if (!string.IsNullOrEmpty(sign.ThirdSignId))
                {
                    string appSecret = string.Empty;
                    var tempApp = (from item in db.t_sms_channel_app
                                   where item.ChannelCode == sign.ChannelCode && item.AppKey == sign.AppKey
                                   select item).FirstOrDefault();
                    if (tempApp != null)
                    {
                        appSecret = tempApp.AppSecret;
                    }
                    if (!string.IsNullOrEmpty(appSecret))
                    {
                        //删除原渠道签名
                        ThirdSmsBase smsObj = new ThirdSmsBase(sign.ChannelCode, sign.AppKey, appSecret);
                        var thirdSms = smsObj.GetThirdSmsObj();
                        thirdSms.DeleteSmsSign(sign.ThirdSignId);
                    }
                }
            }
        }

        /// <summary>
        /// 短信签名提交审核
        /// </summary>
        /// <param name="request"></param>
        public void SmsSignApplyExamine(DetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var sign = (from item in db.t_sms_sign
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                    if (sign == null)
                    {
                        throw new WebApiException(400, "签名不存在");
                    }
                    if (sign.ExamineStatus != 1)
                    {
                        throw new WebApiException(400, "当前状态无法提交审核");
                    }
                    if (string.IsNullOrEmpty(sign.LicenceUrl))
                    {
                        throw new WebApiException(400, "请上传营业执照");
                    }
                    sign.ExamineStatus = 2;
                    sign.ApplyTime = DateTime.Now;
                    db.SaveChanges();

                    string channelCode = sign.ChannelCode;
                    string appKey = sign.AppKey;
                    string appSecret = string.Empty;
                    var tempApp = (from item in db.t_sms_channel_app
                                   where item.ChannelCode == channelCode && item.AppKey == appKey
                                   select item).FirstOrDefault();
                    if (tempApp == null)
                    {
                        throw new WebApiException(400, "无效的应用");
                    }
                    appSecret = tempApp.AppSecret;

                    var imgList = JsonConvert.DeserializeObject<List<ImageObj>>(sign.LicenceUrl);
                    List<Stream> ImgStreamList = new List<Stream>();
                    foreach (var img in imgList)
                    {
                        string WebAddress = img.url;
                        WebRequest imgRequest = WebRequest.Create(WebAddress);
                        WebResponse imgResponse = imgRequest.GetResponse();
                        Stream responseStream = imgResponse.GetResponseStream();
                        ImgStreamList.Add(responseStream);
                    }


                    ThirdSmsBase smsObj = new ThirdSmsBase(channelCode, appKey, appSecret);
                    var thirdSms = smsObj.GetThirdSmsObj();
                    HttpResponse res;
                    if (string.IsNullOrEmpty(sign.ThirdSignId))
                    {
                        res = thirdSms.CreateSmsSign(new SignModel
                        {
                            Sign = sign.SignName,
                            Image0 = ImgStreamList
                        });
                    }
                    else
                    {
                        res = thirdSms.UpdateSmsSign(int.Parse(sign.ThirdSignId), new SignModel
                        {
                            Sign = sign.SignName,
                            Image0 = ImgStreamList
                        });
                    }
                    var resObj = JsonConvert.DeserializeObject<dynamic>(res.Content);
                    if (resObj.error != null)
                    {
                        string errorMessage = resObj.error.code;
                        errorMessage = errorMessage.JGErrorParse();
                        throw new WebApiException(400, errorMessage);
                    }
                    sign.ThirdSignId = resObj.sign_id;
                    db.SaveChanges();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }
        #endregion

        #region====推送管理====
        /// <summary>
        /// 查询短信渠道列表
        /// </summary>
        /// <returns></returns>
        public PageRes<PushChannelInfo> GetPushChannelList(GetPushChannelListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var channelList = from item in db.t_push_channel
                                  select item;
                if (request.Status != 0)
                {
                    channelList = from item in channelList
                                  where item.Status == request.Status
                                  select item;
                }

                int totalCount = channelList.Count();

                return new PageRes<PushChannelInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in channelList
                            orderby item.CreateTime descending
                            select new PushChannelInfo
                            {
                                ChannelCode = item.ChannelCode,
                                ChannelName = item.ChannelName,
                                Status = item.Status,
                                CreateTime = item.LastModified,
                                AppCount = (from x in db.t_push_channel_app
                                            where x.ChannelCode == item.ChannelCode
                                            select x).Count()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 修改短信渠道状态
        /// </summary>
        public void ModifyPushChannelStatus(ModifyPushChannelStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var channel = (from item in db.t_push_channel
                               where item.ChannelCode == request.ChannelCode
                               select item).FirstOrDefault();
                if (channel == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                channel.Status = request.Status;
                channel.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询短信渠道app列表
        /// </summary>
        /// <returns></returns>
        public PageRes<PushChannelAppInfo> GetPushChannelAppList(GetPushChannelAppListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var app = from item in db.t_push_channel_app
                          where item.ChannelCode == request.ChannelCode
                          select item;
                if (request.Status != 0)
                {
                    app = from item in app
                          where item.Status == request.Status
                          select item;
                }
                int totalCount = app.Count();

                return new PageRes<PushChannelAppInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in app
                            orderby item.CreateTime descending
                            select new PushChannelAppInfo
                            {
                                Id = item.Id,
                                AppKey = item.AppKey,
                                AppSecret = item.AppSecret,
                                AppName = item.AppName,
                                Status = item.Status,
                                CreateTime = item.CreateTime
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加短信渠道app
        /// </summary>
        /// <param name="request"></param>
        public void AddPushChannelApp(AddPushChannelAppRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断渠道是否存在
                var channel = (from item in db.t_push_channel
                               where item.ChannelCode == request.ChannelCode && item.Status == 1
                               select item).FirstOrDefault();
                if (channel == null)
                {
                    throw new WebApiException(400, "无效的渠道");
                }

                //判断appKey是否存在
                var app = (from item in db.t_push_channel_app
                           where item.ChannelCode == request.ChannelCode && item.AppName == request.AppName
                           select item).FirstOrDefault();
                if (app != null)
                {
                    throw new WebApiException(400, "应用名称已存在");
                }
                app = (from item in db.t_push_channel_app
                       where item.ChannelCode == request.ChannelCode && item.AppKey == request.AppKey
                       select item).FirstOrDefault();
                if (app != null)
                {
                    throw new WebApiException(400, "AppKey已存在");
                }

                db.t_push_channel_app.Add(new t_push_channel_app
                {
                    Status = 1,
                    AppSecret = request.AppSecret,
                    AppKey = request.AppKey,
                    AppName = request.AppName,
                    ChannelCode = request.ChannelCode,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑短信渠道app
        /// </summary>
        /// <param name="request"></param>
        public void ModifyPushChannelApp(ModifyPushChannelAppRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var app = (from item in db.t_push_channel_app
                           where item.Id == request.Id
                           select item).FirstOrDefault();
                if (app == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                //判断appKey是否存在
                var appTemp = (from item in db.t_push_channel_app
                               where item.Id != request.Id && item.AppName == request.AppName
                               select item).FirstOrDefault();
                if (appTemp != null)
                {
                    throw new WebApiException(400, "应用名称已存在");
                }
                appTemp = (from item in db.t_push_channel_app
                           where item.Id != request.Id && item.AppKey == request.AppKey
                           select item).FirstOrDefault();
                if (appTemp != null)
                {
                    throw new WebApiException(400, "AppKey已存在");
                }

                app.LastModified = DateTime.Now;
                app.AppKey = request.AppKey;
                app.AppSecret = request.AppSecret;
                app.AppName = request.AppName;
                db.SaveChanges();

            }
        }

        /// <summary>
        /// 修改短信渠道app状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyPushChannelAppStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var app = (from item in db.t_push_channel_app
                           where item.Id == request.Id
                           select item).FirstOrDefault();
                if (app == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                app.LastModified = DateTime.Now;
                app.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除短信渠道app
        /// </summary>
        /// <param name="request"></param>
        public void DeletePushChannelApp(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var app = (from item in db.t_push_channel_app
                           where item.Id == request.Id
                           select item).FirstOrDefault();
                if (app == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_push_channel_app.Remove(app);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询推送分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<PushTagInfo> GetPushTagList(GetPushTagListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tag = from item in db.t_push_tag
                          select item;
                if (request.Status != 0)
                {
                    tag = from item in tag
                          where item.Status == request.Status
                          select item;
                }

                int totalCount = tag.Count();

                return new PageRes<PushTagInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in tag
                            join item2 in db.t_account_api on item.TriApiUrl equals item2.Url into a
                            from ai in a.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new PushTagInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Description = item.Description,
                                Id = item.Id,
                                Name = item.Name,
                                Tag = item.Tag,
                                TriApiUrl = item.TriApiUrl,
                                TriApiDes = ai == null ? "" : ai.Description
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加推送分组
        /// </summary>
        /// <param name="request"></param>
        public void AddPushTag(AddPushTagRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断标签是否存在
                var tag = (from item in db.t_push_tag
                           where item.Name == request.Name
                           select item).FirstOrDefault();
                if (tag != null)
                {
                    throw new WebApiException(400, "分组名称已存在");
                }
                tag = (from item in db.t_push_tag
                       where item.Tag == request.Tag
                       select item).FirstOrDefault();
                if (tag != null)
                {
                    throw new WebApiException(400, "分组标识已存在");
                }

                db.t_push_tag.Add(new t_push_tag
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    Description = request.Description,
                    LastModified = DateTime.Now,
                    Name = request.Name,
                    Tag = request.Tag,
                    TriApiUrl = request.TriApiUrl
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑推送分组
        /// </summary>
        /// <param name="request"></param>
        public void ModifyPushTag(ModifyPushTagRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tag = (from item in db.t_push_tag
                           where item.Id == request.Id
                           select item).FirstOrDefault();
                if (tag == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }

                var temptag = (from item in db.t_push_tag
                               where item.Name == request.Name && item.Id != request.Id
                               select item).FirstOrDefault();
                if (temptag != null)
                {
                    throw new WebApiException(400, "分组名称已存在");
                }
                temptag = (from item in db.t_push_tag
                           where item.Tag == request.Tag && item.Id != request.Id
                           select item).FirstOrDefault();
                if (temptag != null)
                {
                    throw new WebApiException(400, "分组标识已存在");
                }
                tag.Name = request.Name;
                tag.Description = request.Description;
                tag.LastModified = DateTime.Now;
                tag.Tag = request.Tag;
                tag.TriApiUrl = request.TriApiUrl;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改推送分组状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyPushTagStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tag = (from item in db.t_push_tag
                           where item.Id == request.Id
                           select item).FirstOrDefault();
                if (tag == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }

                tag.LastModified = DateTime.Now;
                tag.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除推送分组
        /// </summary>
        /// <param name="request"></param>
        public void DeletePushTag(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tag = (from item in db.t_push_tag
                           where item.Id == request.Id
                           select item).FirstOrDefault();
                if (tag == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }

                db.t_push_tag.Remove(tag);
                db.SaveChanges();
            }
        }
        #endregion
    }
}
