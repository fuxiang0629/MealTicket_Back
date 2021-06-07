using FXCommon.Common;
using MealTicket_Admin_Handler.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler
{
    public class FrontAccountHandler
    {
        #region====账户管理====
        /// <summary>
        /// 查询前端账户列表
        /// </summary>
        /// <returns></returns>
        public PageRes<FrontAccountInfo> GetFrontAccountList(GetFrontAccountListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var account = from item in db.t_account_baseinfo
                              join item2 in db.t_account_baseinfo on item.ReferId equals item2.Id into a
                              from ai in a.DefaultIfEmpty()
                              join item3 in db.t_account_wallet on item.Id equals item3.AccountId
                              select new { item, ai, item3 };
                if (!string.IsNullOrEmpty(request.AccountInfo))
                {
                    account = from item in account
                              where item.item.Mobile.Contains(request.AccountInfo) || item.item.NickName.Contains(request.AccountInfo) || item.item.RecommandCode.Contains(request.AccountInfo)
                              select item;
                }
                if (!string.IsNullOrEmpty(request.ReferInfo))
                {
                    account = from item in account
                              where item.ai != null && (item.ai.NickName.Contains(request.ReferInfo) || item.ai.RecommandCode.Contains(request.ReferInfo) || item.ai.Mobile.Contains(request.ReferInfo))
                              select item;
                }
                if (request.StartTime != null && request.EndTime != null)
                {
                    account = from item in account
                              where item.item.CreateTime >= request.StartTime && item.item.CreateTime < request.EndTime
                              select item;
                }
                if (request.MaxId > 0)
                {
                    account = from item in account
                              where item.item.Id <= request.MaxId
                              select item;
                }
                int totalCount = account.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = account.Max(e => e.item.Id);
                }
                var list = (from item in account
                            orderby item.item.CreateTime descending
                            select new FrontAccountInfo
                            {
                                Sex = item.item.Sex,
                                Status = item.item.Status,
                                ForbidStatus = item.item.ForbidStatus,
                                MonitorStatus = item.item.MonitorStatus,
                                MultipleChangeStatus = item.item.MultipleChangeStatus,
                                CashStatus = item.item.CashStatus,
                                AccountId = item.item.Id,
                                BirthDay = item.item.BirthDay,
                                CreateTime = item.item.CreateTime,
                                HeadUrl = item.item.HeadUrl,
                                Mobile = item.item.Mobile,
                                NickName = item.item.NickName,
                                RecommandCode = item.item.RecommandCode,
                                ReferNickName = item.ai == null ? "" : item.ai.NickName,
                                ReferRecommandCode = item.ai == null ? "" : item.ai.RecommandCode,
                                WalletStatus = item.item3.Status,
                                Deposit = item.item3.Deposit,
                                BankCardCount = (from x in db.t_account_bank_card
                                                 where x.AccountId == item.item.Id
                                                 select x).Count()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                foreach (var item in list)
                {
                    var temp = (from x in db.t_account_shares_hold
                                join x2 in db.t_shares_quotes on new { x.Market, x.SharesCode } equals new { x2.Market, x2.SharesCode }
                                where x.AccountId == item.AccountId && x.RemainCount > 0 && x2.ClosedPrice>0
                                select new { x, x2 }).ToList();
                    if (temp.Count() > 0)
                    {
                        item.UseDeposit = temp.Sum(e => e.x.RemainDeposit);
                        item.TotalFundAmount = temp.Sum(e => e.x.FundAmount);
                        item.TotalMarketValue = (temp.Sum(e => e.x.RemainCount * (e.x2.PresentPrice <= 0 ? e.x2.ClosedPrice : e.x2.PresentPrice))) / 100 * 100;
                    }
                    var realName = (from x in db.t_account_realname
                                    where x.AccountId == item.AccountId && x.ExamineStatus == 2
                                    select x).FirstOrDefault();
                    if (realName != null)
                    {
                        item.IsRealName = true;
                    }

                    item.FollowAccountCount = (from x in db.t_account_follow_rel
                                               where x.AccountId == item.AccountId
                                               select x).Count();

                    var followTemp = (from x in db.t_account_follow_rel
                                      join x2 in db.t_account_baseinfo on x.AccountId equals x2.Id
                                      where x.FollowAccountId == item.AccountId
                                      select x2).FirstOrDefault();
                    if (followTemp == null)
                    {
                        item.MainAccountId = 0;
                        item.MainAccountInfo = "";
                    }
                    else
                    {
                        item.MainAccountId = followTemp.Id;
                        item.MainAccountInfo = followTemp.Mobile + (string.IsNullOrEmpty(followTemp.NickName) ? "" : ("(" + followTemp.NickName + ")"));
                    }
                }

                return new PageRes<FrontAccountInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = list
                };
            }
        }

        /// <summary>
        /// 查询前端账户个人信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public FrontAccountInfo GetFrontAccountInfo(DetailsRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var accountInfo = (from item in db.t_account_baseinfo
                                   where item.Id == request.Id
                                   select new FrontAccountInfo
                                   {
                                       MultipleChangeStatus = item.MultipleChangeStatus
                                   }).FirstOrDefault();
                return accountInfo;
            }
        }

        /// <summary>
        /// 编辑前端账户基础信息
        /// </summary>
        public void ModifyFrontAccountBaseInfo(ModifyFrontAccountBaseInfoRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var account = (from item in db.t_account_baseinfo
                               where item.Id == request.AccountId
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400, "账户不存在");
                }
                account.HeadUrl = request.HeadUrl;
                account.BirthDay = request.BirthDay;
                account.LastModified = DateTime.Now;
                account.NickName = request.NickName;
                account.Sex = request.Sex;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改前端账户推荐人
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountRefer(ModifyFrontAccountReferRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_AccountSetRefer(request.AccountId, request.ReferRecommandCode, errorCodeDb, errorMessageDb);
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
        /// 修改前端账户状态
        /// </summary>
        public void ModifyFrontAccountStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var account = (from item in db.t_account_baseinfo
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400, "账户不存在");
                }
                account.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改前端账户交易状态
        /// </summary>
        public void ModifyFrontAccountTradeStatus(ModifyFrontAccountTradeStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var account = (from item in db.t_account_baseinfo
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400, "账户不存在");
                }
                account.ForbidStatus = request.ForbidStatus;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改前端账户监控状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountMonitorStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var account = (from item in db.t_account_baseinfo
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400, "账户不存在");
                }
                account.MonitorStatus = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改前端账户提现状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountCashStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var account = (from item in db.t_account_baseinfo
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400, "账户不存在");
                }
                account.CashStatus = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改前端账户倍数修改状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountMultipleChangeStatus(ModifyStatusRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var account = (from item in db.t_account_baseinfo
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400, "账户不存在");
                }
                account.MultipleChangeStatus = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改前端账户密码
        /// </summary>
        public void ModifyFrontAccountLoginPassword(ModifyFrontAccountLoginPasswordRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var account = (from item in db.t_account_baseinfo
                               where item.Id == request.AccountId
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400, "账户不存在");
                }
                string newPassword = request.NewPassword.ToMD5();
                //if (account.TransactionPassword == newPassword)
                //{
                //    throw new WebApiException(400,"登录密码不能与交易密码一致");
                //}
                account.LoginPassword = newPassword;

                var loginToken = (from item in db.t_account_login_token
                                  where item.AccountId == request.AccountId
                                  select item).FirstOrDefault();
                if (loginToken != null)
                {
                    //生成loginuuid
                    string loginUUid = Guid.NewGuid().ToString().ToUpper();
                    loginToken.Login_Uuid = loginUUid;
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改前端账户交易密码
        /// </summary>
        public void ModifyFrontAccountTransactionPassword(ModifyFrontAccountTransactionPasswordRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var account = (from item in db.t_account_baseinfo
                               where item.Id == request.AccountId
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400, "账户不存在");
                }
                string transactionPassword = request.TransactionPassword.ToMD5();
                //if (account.LoginPassword == transactionPassword)
                //{
                //    throw new WebApiException(400, "交易密码不能与登录密码一致");
                //}
                account.TransactionPassword = transactionPassword;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改前端账户钱包状态
        /// </summary>
        public void ModifyFrontAccountWalletStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var accountWallet = (from item in db.t_account_wallet
                                     where item.AccountId == request.Id
                                     select item).FirstOrDefault();
                if (accountWallet == null)
                {
                    throw new WebApiException(400, "账户钱包不存在");
                }
                accountWallet.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改前端账户余额
        /// </summary>
        public void ModifyFrontAccountWallet(ModifyFrontAccountWalletRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_Admin_ModifyFrontAccountWallet(request.AccountId, request.AddDeposit, request.Remark, errorCodeDb, errorMessageDb);
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
        /// 查询前端账户余额变更记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountWalletChangeRecordInfo> GetFrontAccountWalletChangeRecord(GetFrontAccountWalletChangeRecordRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var record = from item in db.t_account_wallet_change_record
                             where item.AccountId == request.AccountId
                             select item;
                if (request.StartTime != null && request.EndTime != null)
                {
                    record = from item in record
                             where item.CreateTime >= request.StartTime && item.CreateTime < request.EndTime
                             select item;
                }
                int totalCount = record.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = record.Max(e => e.Id);
                }
                return new PageRes<FrontAccountWalletChangeRecordInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in record
                            orderby item.CreateTime descending
                            select new FrontAccountWalletChangeRecordInfo
                            {
                                ChangeAmount = item.ChangeAmount,
                                CreateTime = item.CreateTime,
                                CurAmount = item.CurAmount,
                                Description = item.Description,
                                Id = item.Id,
                                PreAmount = item.PreAmount
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询前端账户银行卡列表
        /// </summary>
        /// <returns></returns>
        public PageRes<FrontAccountBankCardInfo> GetFrontAccountBankCardList(GetFrontAccountBankCardListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var bankCard = from item in db.t_account_bank_card
                               join item2 in db.t_bank on item.BankCode equals item2.BankCode
                               where item.AccountId == request.AccountId
                               select new { item, item2 };
                if (!string.IsNullOrEmpty(request.CardNumber))
                {
                    bankCard = from item in bankCard
                               where item.item.CardNumber.Contains(request.CardNumber)
                               select item;
                }
                if (request.MaxId > 0)
                {
                    bankCard = from item in bankCard
                               where item.item.Id <= request.MaxId
                               select item;
                }
                int totalCount = bankCard.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = bankCard.Max(e => e.item.Id);
                }

                return new PageRes<FrontAccountBankCardInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in bankCard
                            orderby item.item.CreateTime descending
                            select new FrontAccountBankCardInfo
                            {
                                BankName = item.item2.BankName,
                                CardBreed = item.item.CardBreed,
                                CardNumber = item.item.CardNumber,
                                CreateTime = item.item.CreateTime,
                                Id = item.item.Id,
                                Mobile = item.item.Mobile,
                                RealName = item.item.RealName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询前端账户推荐奖励类别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountRecommendPrizeTypeInfo> GetFrontAccountRecommendPrizeType(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var result = from item in db.t_account_recommend_prize_type
                                 where item.AccountId == request.Id
                                 select item;
                    int totalCount = result.Count();
                    if (totalCount <= 0)
                    {
                        db.t_account_recommend_prize_type.Add(new t_account_recommend_prize_type
                        {
                            Status = 1,
                            AccountId = request.Id,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            Type = 1
                        });
                        db.t_account_recommend_prize_type.Add(new t_account_recommend_prize_type
                        {
                            Status = 1,
                            AccountId = request.Id,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            Type = 2
                        });
                        db.t_account_recommend_prize_type.Add(new t_account_recommend_prize_type
                        {
                            Status = 1,
                            AccountId = request.Id,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            Type = 3
                        });
                        db.t_account_recommend_prize_type.Add(new t_account_recommend_prize_type
                        {
                            Status = 1,
                            AccountId = request.Id,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            Type = 4
                        });
                        db.t_account_recommend_prize_type.Add(new t_account_recommend_prize_type
                        {
                            Status = 1,
                            AccountId = request.Id,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            Type = 5
                        });
                        db.t_account_recommend_prize_type.Add(new t_account_recommend_prize_type
                        {
                            Status = 1,
                            AccountId = request.Id,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            Type = 6
                        });
                        db.SaveChanges();
                    }
                    result = from item in db.t_account_recommend_prize_type
                             where item.AccountId == request.Id
                             select item;
                    totalCount = result.Count();
                    var res = new PageRes<FrontAccountRecommendPrizeTypeInfo>
                    {
                        MaxId = 0,
                        TotalCount = totalCount,
                        List = (from item in result
                                orderby item.Type
                                select new FrontAccountRecommendPrizeTypeInfo
                                {
                                    Status = item.Status,
                                    Id = item.Id,
                                    LevelCount = (from x in db.t_account_recommend_prize_type_level
                                                  where x.TypeId == item.Id
                                                  select x).Count(),
                                    Type = item.Type
                                }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                    };
                    tran.Commit();
                    return res;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return new PageRes<FrontAccountRecommendPrizeTypeInfo>();
                }
            }
        }

        /// <summary>
        /// 修改前端账户推荐奖励类别状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountRecommendPrizeTypeStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_account_recommend_prize_type
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (result == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                result.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询前端账户推荐奖励分类级别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountRecommendPrizeTypeLevelInfo> GetFrontAccountRecommendPrizeTypeLevel(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var levelList = from item in db.t_account_recommend_prize_type_level
                                where item.TypeId == request.Id
                                select item;
                int totalCount = levelList.Count();
                return new PageRes<FrontAccountRecommendPrizeTypeLevelInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in levelList
                            orderby item.LevelValue
                            select new FrontAccountRecommendPrizeTypeLevelInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                LevelValue = item.LevelValue,
                                PrizeRate = item.PrizeRate
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加前端账户推荐奖励分类级别
        /// </summary>
        /// <param name="request"></param>
        public void AddFrontAccountRecommendPrizeTypeLevel(AddFrontAccountRecommendPrizeTypeLevelRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断typeId是否存在
                var typeInfo = (from item in db.t_account_recommend_prize_type
                                where item.Id == request.TypeId
                                select item).FirstOrDefault();
                if (typeInfo == null)
                {
                    throw new WebApiException(400, "类别不存在");
                }
                //判断级别是否存在
                var level = (from item in db.t_account_recommend_prize_type_level
                             where item.TypeId == request.TypeId && item.LevelValue == request.LevelValue
                             select item).FirstOrDefault();
                if (level != null)
                {
                    throw new WebApiException(400, "级别已存在");
                }
                db.t_account_recommend_prize_type_level.Add(new t_account_recommend_prize_type_level
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    LevelValue = request.LevelValue,
                    PrizeRate = request.PrizeRate,
                    TypeId = request.TypeId
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑前端账户推荐奖励分类级别
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountRecommendPrizeTypeLevel(ModifyFrontAccountRecommendPrizeTypeLevelRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断Id是否存在
                var level = (from item in db.t_account_recommend_prize_type_level
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (level == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                //判断级别是否存在
                var temp = (from item in db.t_account_recommend_prize_type_level
                            where item.TypeId == level.TypeId && item.LevelValue == request.LevelValue && item.Id != request.Id
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "级别已存在");
                }
                level.LastModified = DateTime.Now;
                level.LevelValue = request.LevelValue;
                level.PrizeRate = request.PrizeRate;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改前端账户推荐奖励分类级别状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountRecommendPrizeTypeLevelStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断Id是否存在
                var level = (from item in db.t_account_recommend_prize_type_level
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (level == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                level.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除前端账户推荐奖励分类级别
        /// </summary>
        /// <param name="request"></param>
        public void DeleteFrontAccountRecommendPrizeTypeLevel(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断Id是否存在
                var level = (from item in db.t_account_recommend_prize_type_level
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (level == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_account_recommend_prize_type_level.Remove(level);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询前端账户交易费用配置列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountTradeParSettingInfo> GetFrontAccountTradeParSettingList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var parSetting = from item in db.t_account_par_setting
                                 where item.AccountId == request.Id
                                 select item;
                int totalCount = parSetting.Count();

                return new PageRes<FrontAccountTradeParSettingInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in parSetting
                            orderby item.CreateTime descending
                            select new FrontAccountTradeParSettingInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamName = item.ParamName,
                                ParamValue = item.ParamValue,
                                Status = item.Status
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加前端账户交易费用配置
        /// </summary>
        /// <param name="request"></param>
        public void AddFrontAccountTradeParSetting(AddFrontAccountTradeParSettingRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断参数名是否存在
                var parSetting = (from item in db.t_account_par_setting
                                  where item.AccountId == request.AccountId && item.ParamName == request.ParamName
                                  select item).FirstOrDefault();
                if (parSetting != null)
                {
                    throw new WebApiException(400, "参数名已存在");
                }
                db.t_account_par_setting.Add(new t_account_par_setting
                {
                    Status = 1,
                    AccountId = request.AccountId,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    ParamName = request.ParamName,
                    ParamValue = request.ParamValue
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑前端账户交易费用配置
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountTradeParSetting(ModifyFrontAccountTradeParSettingRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断数据是否存在
                var parSetting = (from item in db.t_account_par_setting
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (parSetting == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                //判断参数名是否存在
                var temp = (from item in db.t_account_par_setting
                            where item.AccountId == parSetting.AccountId && item.ParamName == request.ParamName && item.Id != request.Id
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "参数名已存在");
                }
                parSetting.LastModified = DateTime.Now;
                parSetting.ParamName = request.ParamName;
                parSetting.ParamValue = request.ParamValue;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改前端账户交易费用配置状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountTradeParSettingStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断数据是否存在
                var parSetting = (from item in db.t_account_par_setting
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (parSetting == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                parSetting.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除前端账户交易费用配置
        /// </summary>
        /// <param name="request"></param>
        public void DeleteFrontAccountTradeParSetting(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断数据是否存在
                var parSetting = (from item in db.t_account_par_setting
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (parSetting == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                db.t_account_par_setting.Remove(parSetting);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询前端账户实名认证列表
        /// </summary>
        /// <returns></returns>
        public PageRes<FrontAccountRealNameInfo> GetFrontAccountRealNameList(GetFrontAccountRealNameListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var realname = from item in db.t_account_realname
                               join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id
                               select new { item, item2 };
                if (!string.IsNullOrEmpty(request.AccountInfo))
                {
                    realname = from item in realname
                               where item.item2.NickName.Contains(request.AccountInfo) || item.item2.Mobile.Contains(request.AccountInfo)
                               select item;
                }
                if (request.ExamineStatus != 0)
                {
                    realname = from item in realname
                               where item.item.ExamineStatus == request.ExamineStatus
                               select item;
                }
                if (request.StartTime != null && request.EndTime != null)
                {
                    realname = from item in realname
                               where item.item.CreateTime >= request.StartTime && item.item.CreateTime < request.EndTime
                               select item;
                }

                int totalCount = realname.Count();
                return new PageRes<FrontAccountRealNameInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in realname
                            orderby item.item.CreateTime descending
                            select new FrontAccountRealNameInfo
                            {
                                Sex = item.item.Sex,
                                ExamineStatusDes = item.item.ExamineStatusDes,
                                ImgUrlBack = item.item.ImgUrlBack,
                                ExamineStatus = item.item.ExamineStatus,
                                AccountId = item.item.AccountId,
                                AccountMobile = item.item2.Mobile,
                                AccountName = item.item2.NickName,
                                Address = item.item.Address,
                                BirthDay = item.item.BirthDay,
                                CardNo = item.item.CardNo,
                                ExamineFinishTime = item.item.ExamineFinishTime,
                                CreateTime = item.item.CreateTime,
                                ImgUrlFront = item.item.ImgUrlFront,
                                RealName = item.item.RealName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询前端账户实名认证信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public FrontAccountRealNameInfo GetFrontAccountRealNameInfo(DetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var realname = (from item in db.t_account_realname
                                join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id
                                where item.AccountId == request.Id
                                select new FrontAccountRealNameInfo
                                {
                                    Sex = item.Sex,
                                    ExamineStatusDes = item.ExamineStatusDes,
                                    ImgUrlBack = item.ImgUrlBack,
                                    ExamineStatus = item.ExamineStatus,
                                    AccountId = item.AccountId,
                                    AccountMobile = item2.Mobile,
                                    AccountName = item2.NickName,
                                    Address = item.Address,
                                    BirthDay = item.BirthDay,
                                    CardNo = item.CardNo,
                                    ExamineFinishTime = item.ExamineFinishTime,
                                    CreateTime = item.CreateTime,
                                    ImgUrlFront = item.ImgUrlFront,
                                    RealName = item.RealName
                                }).FirstOrDefault();
                return realname;
            }
        }

        /// <summary>
        /// 审核前端账户实名认证
        /// </summary>
        public void ExamineFrontAccountRealName(ExamineFrontAccountRealNameRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var realname = (from item in db.t_account_realname
                                where item.AccountId == request.AccountId
                                select item).FirstOrDefault();
                if (realname == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                if (realname.ExamineStatus != 1)
                {
                    throw new WebApiException(400, "已处理");
                }
                realname.ExamineStatus = request.ExamineStatus;
                realname.ExamineStatusDes = request.ExamineStatusDes;
                realname.ExamineFinishTime = DateTime.Now;

                if (request.ExamineStatus == 2)
                {
                    var accountInfo = (from item in db.t_account_baseinfo
                                       where item.Id == request.AccountId
                                       select item).FirstOrDefault();
                    if (accountInfo == null)
                    {
                        throw new WebApiException(400, "用户不存在");
                    }
                    accountInfo.BirthDay = realname.BirthDay;
                    accountInfo.Sex = realname.Sex;
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询前端账户注册日志
        /// </summary>
        /// <returns></returns>
        public PageRes<FrontAccountRegisterLogInfo> GetFrontAccountRegisterLog(GetFrontAccountRegisterLogRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var registerLog = from item in db.t_account_registerLog
                                  join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id
                                  select new { item, item2 };
                if (!string.IsNullOrEmpty(request.AccountInfo))
                {
                    registerLog = from item in registerLog
                                  where item.item2.Mobile.Contains(request.AccountInfo) || item.item2.NickName.Contains(request.AccountInfo)
                                  select item;
                }
                if (request.StartTime != null && request.EndTime != null)
                {
                    registerLog = from item in registerLog
                                  where item.item.CreateTime >= request.StartTime && item.item.CreateTime < request.EndTime
                                  select item;
                }
                if (request.MaxId > 0)
                {
                    registerLog = from item in registerLog
                                  where item.item.AccountId <= request.MaxId
                                  select item;
                }
                int totalCount = registerLog.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = registerLog.Max(e => e.item.AccountId);
                }

                return new PageRes<FrontAccountRegisterLogInfo>
                {
                    TotalCount = totalCount,
                    MaxId = maxId,
                    List = (from item in registerLog
                            orderby item.item.CreateTime descending
                            select new FrontAccountRegisterLogInfo
                            {
                                AccountId = item.item.AccountId,
                                CreateTime = item.item.CreateTime,
                                DeviceUA = item.item.DeviceUA,
                                Mobile = item.item2.Mobile,
                                NickName = item.item2.NickName,
                                RegIp = item.item.RegIp,
                                RegisterMobile = item.item.AccountName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询后台用户登录日志列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountLoginLogInfo> GetFrontAccountLoginLog(GetFrontAccountLoginLogRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var loginLog = from item in db.t_account_loginlog
                               select item;
                if (!string.IsNullOrEmpty(request.AccountInfo))
                {
                    loginLog = from item in loginLog
                               where item.AccountName.Contains(request.AccountInfo)
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
                int totalCount = loginLog.Count();
                return new PageRes<FrontAccountLoginLogInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in loginLog
                            orderby item.LoginTime descending
                            select new FrontAccountLoginLogInfo
                            {
                                AccountName = item.AccountName,
                                DeviceUA = item.DeviceUA,
                                LogId = item.Id,
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
        /// 查询用户监控席位列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSeatInfo> GetFrontAccountSeatList(GetFrontAccountSeatListRequest request)
        {
            DateTime maxTime = DateTime.Parse("9999-01-01");
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var seat = from item in db.t_account_shares_seat
                           where item.AccountId == request.Id
                           select item;
                if (request.ExpireStatus == 1)
                {
                    seat = from item in seat
                           where item.ValidEndTime > timeNow
                           select item;
                }
                if (request.ExpireStatus == 2)
                {
                    seat = from item in seat
                           where item.ValidEndTime <= timeNow
                           select item;
                }
                if (request.Status != 0)
                {
                    seat = from item in seat
                           where item.Status == request.Status
                           select item;
                }
                if (!string.IsNullOrEmpty(request.Name))
                {
                    seat = from item in seat
                           where item.Name.Contains(request.Name)
                           select item;
                }

                int totalCount = seat.Count();

                return new PageRes<FrontAccountSeatInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in seat
                            join item2 in db.t_account_shares_optional_seat_rel on item.Id equals item2.SeatId into a
                            from ai in a.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new FrontAccountSeatInfo
                            {
                                Description = item.Description,
                                Id = item.Id,
                                Name = item.Name,
                                OptionalId = ai == null ? 0 : ai.OptionalId,
                                ValidEndTimeStr = item.ValidEndTime >= maxTime ? "永久" : item.ValidEndTime <= timeNow ? "已过期" : ("剩余" + SqlFunctions.DateDiff("DAY", timeNow, item.ValidEndTime) + "天"),
                                ValidEndTime = item.ValidEndTime,
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                SourceFrom = item.SourceForm,
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加用户监控席位
        /// </summary>
        /// <param name="request"></param>
        public void AddFrontAccountSeat(AddFrontAccountSeatRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断席位名称是否存在
                var seat = (from item in db.t_account_shares_seat
                            where item.AccountId == request.AccountId && item.Name == request.Name
                            select item).FirstOrDefault();
                if (seat != null)
                {
                    throw new WebApiException(400, "席位名称已存在");
                }
                db.t_account_shares_seat.Add(new t_account_shares_seat
                {
                    Status = 1,
                    AccountId = request.AccountId,
                    CreateTime = DateTime.Now,
                    Description = request.Description,
                    LastModified = DateTime.Now,
                    Name = request.Name,
                    SourceForm = 1,
                    ValidEndTime = request.ValidEndTime,
                });
                db.SaveChanges();

            }
        }

        /// <summary>
        /// 批量添加用户监控席位
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public void BatchAddFrontAccountSeat(BatchAddFrontAccountSeatRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                int SeatNumber = 4001;
                var seat = (from item in db.t_account_shares_seat
                            where item.AccountId == request.AccountId && item.SourceForm == 2
                            orderby item.Name descending
                            select item).FirstOrDefault();
                if (seat != null)
                {
                    SeatNumber = int.Parse(seat.Name) + 1;
                }

                for (int i = 0; i < request.Count; i++)
                {
                    db.t_account_shares_seat.Add(new t_account_shares_seat
                    {
                        Status = 1,
                        AccountId = request.AccountId,
                        CreateTime = DateTime.Now,
                        Description = SeatNumber.ToString(),
                        LastModified = DateTime.Now,
                        Name = SeatNumber.ToString(),
                        SourceForm = 2,
                        ValidEndTime = request.ValidEndTime,
                    });
                    SeatNumber++;
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑用户监控席位
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountSeat(ModifyFrontAccountSeatRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var seat = (from item in db.t_account_shares_seat
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (seat == null)
                {
                    throw new WebApiException(400, "席位不存在");
                }

                var tempSeat = (from item in db.t_account_shares_seat
                                where item.Id != request.Id && item.AccountId == seat.AccountId && item.Name == request.Name
                                select item).FirstOrDefault();
                if (tempSeat != null)
                {
                    throw new WebApiException(400, "席位名称已存在");
                }

                seat.Name = request.Name;
                seat.Description = request.Description;
                seat.LastModified = DateTime.Now;
                seat.ValidEndTime = request.ValidEndTime;
                db.SaveChanges();

            }
        }

        /// <summary>
        /// 修改用户监控席位状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountSeatStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var seat = (from item in db.t_account_shares_seat
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (seat == null)
                {
                    throw new WebApiException(400, "席位不存在");
                }
                seat.Status = request.Status;
                seat.LastModified = DateTime.Now;
                db.SaveChanges();

            }
        }

        /// <summary>
        /// 删除用户监控席位
        /// </summary>
        /// <param name="request"></param>
        public void DeleteFrontAccountSeat(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var seat = (from item in db.t_account_shares_seat
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (seat == null)
                {
                    throw new WebApiException(400, "席位不存在");
                }

                db.t_account_shares_seat.Remove(seat);


                var seatRel = (from item in db.t_account_shares_optional_seat_rel
                               where item.SeatId == request.Id
                               select item).ToList();
                db.t_account_shares_optional_seat_rel.RemoveRange(seatRel);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询用户自选股列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountOptionalInfo> GetFrontAccountOptionalList(GetFrontAccountOptionalListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //自选股
                var optional = from item in db.t_account_shares_optional.AsNoTracking()
                               join item2 in db.t_shares_all.AsNoTracking() on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into c
                               from ci in c.DefaultIfEmpty()
                               join item3 in db.t_shares_quotes.AsNoTracking() on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode } into a
                               from ai in a.DefaultIfEmpty()
                               join item4 in db.t_shares_monitor.AsNoTracking() on new { item.Market, item.SharesCode } equals new { item4.Market, item4.SharesCode } into b
                               from bi in b.DefaultIfEmpty()
                               where item.AccountId == request.Id
                               select new { item, ci, ai, bi };
                if (!string.IsNullOrEmpty(request.SharesInfo))
                {
                    optional = from item in optional
                               where item.item.SharesCode.Contains(request.SharesInfo) || (item.ci != null && (item.ci.SharesName.Contains(request.SharesInfo) || item.ci.SharesPyjc.StartsWith(request.SharesInfo)))
                               select item;
                }

                int totalCount = optional.Count();

                var seatList = (from item in db.t_account_shares_seat
                                join item2 in db.t_account_shares_optional_seat_rel on item.Id equals item2.SeatId
                                where item.AccountId == request.Id && item.Status == 1
                                select new { item, item2 }).ToList();

                DateTime maxTime = DateTime.Parse("9999-01-01");
                DateTime timeNow = DateTime.Now;
                return new PageRes<FrontAccountOptionalInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in optional.ToList()
                            join item2 in seatList on item.item.Id equals item2.item2.OptionalId into a
                            from ai in a.DefaultIfEmpty()
                            orderby item.item.CreateTime descending
                            let currPrice = item.ai == null ? 0 : item.ai.PresentPrice <= 0 ? item.ai.ClosedPrice : item.ai.PresentPrice
                            select new FrontAccountOptionalInfo
                            {
                                IsValid = (item.ai == null || item.bi == null || item.ci == null) ? false : true,
                                SharesCode = item.item.SharesCode,
                                SharesName = item.ci == null ? "" : item.ci.SharesName,
                                CreateTime = item.item.CreateTime,
                                CurrPrice = currPrice,
                                Id = item.item.Id,
                                Market = item.item.Market,
                                RisePrice = item.ai == null ? 0 : (currPrice - item.ai.ClosedPrice),
                                RiseRate = (item.ai == null || item.ai.ClosedPrice <= 0) ? 0 : (int)((currPrice - item.ai.ClosedPrice) * 1.0 / item.ai.ClosedPrice * 10000),
                                SeatId = ai == null ? 0 : ai.item.Id,
                                SeatName = ai == null ? "" : ai.item.Name,
                                ValidTime = ai == null ? "" : (ai.item.ValidEndTime >= maxTime ? "永久" : ai.item.ValidEndTime < timeNow ? "已过期" : ("剩余" + (int)(ai.item.ValidEndTime.Date - timeNow.Date).TotalDays) + "天")
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };

            }
        }

        /// <summary>
        /// 添加用户自选股
        /// </summary>
        /// <param name="request"></param>
        public void AddFrontAccountOptional(AddFrontAccountOptionalRequest request)
        {
            BatchAddSharesMonitor(request.AccountId, new List<MarketTimeInfo>
            {
                new MarketTimeInfo
                {
                    SharesCode=request.SharesCode,
                    Market=request.Market
                }
            });
        }

        /// <summary>
        /// 删除用户自选股
        /// </summary>
        /// <param name="request"></param>
        public void DeleteFrontAccountOptional(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var optional = (from item in db.t_account_shares_optional
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                    if (optional == null)
                    {
                        throw new WebApiException(400, "自选股不存在");
                    }
                    long optionalId = optional.Id;

                    var optionalTrend = (from item in db.t_account_shares_optional_trend_rel
                                         where item.OptionalId == optionalId
                                         select item).ToList();
                    List<long> relIdList = optionalTrend.Select(e => e.Id).ToList();

                    var trendPar = (from item in db.t_account_shares_optional_trend_rel_par
                                    where relIdList.Contains(item.RelId)
                                    select item).ToList();

                    db.t_account_shares_optional.Remove(optional);
                    if (optionalTrend.Count() > 0)
                    {
                        db.t_account_shares_optional_trend_rel.RemoveRange(optionalTrend);
                    }
                    if (trendPar.Count() > 0)
                    {
                        db.t_account_shares_optional_trend_rel_par.RemoveRange(trendPar);
                    }
                    var seatRel = (from item in db.t_account_shares_optional_seat_rel
                                   where item.OptionalId == optionalId
                                   select item).ToList();
                    db.t_account_shares_optional_seat_rel.RemoveRange(seatRel);
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
        /// 批量导入用户自选股
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddSharesMonitor(long accountId, List<MarketTimeInfo> list)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //判断是否存在
                    var optional = (from item in db.t_account_shares_optional
                                    where item.AccountId == accountId
                                    select item).ToList();
                    //判断是否在监控股票中
                    var monitor = (from item in db.t_shares_monitor
                                   where item.Status == 1
                                   select item).ToList();
                    //查询参数
                    var parList = (from x in db.t_shares_monitor_trend_rel
                                   join x2 in db.t_shares_monitor_trend_rel_par on x.Id equals x2.RelId
                                   select new { x, x2 }).ToList();
                    List<MarketTimeInfo> tempList = new List<MarketTimeInfo>();
                    foreach (var item in list)
                    {
                        if (optional.Where(e => e.Market == item.Market && e.SharesCode == item.SharesCode).FirstOrDefault() != null)
                        {
                            continue;
                        }
                        var temp = monitor.Where(e => e.Market == item.Market && e.SharesCode == item.SharesCode).FirstOrDefault();
                        if (temp == null)
                        {
                            continue;
                        }
                        item.MonitorId = temp.Id;
                        tempList.Add(item);
                    }

                    foreach (var item in tempList)
                    {
                        //查询参数
                        var par = (from x in parList
                                   where x.x.MonitorId == item.MonitorId
                                   select x).ToList();

                        var addOptional = new t_account_shares_optional
                        {
                            SharesCode = item.SharesCode,
                            AccountId = accountId,
                            CreateTime = DateTime.Now,
                            IsTrendClose = false,
                            Market = item.Market,
                            TriCountToday = 0
                        };
                        db.t_account_shares_optional.Add(addOptional);
                        db.SaveChanges();

                        t_account_shares_optional_trend_rel trend1 = new t_account_shares_optional_trend_rel
                        {
                            Status = 1,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            OptionalId = addOptional.Id,
                            TrendId = 1
                        };
                        db.t_account_shares_optional_trend_rel.Add(trend1);
                        db.SaveChanges();

                        List<t_account_shares_optional_trend_rel_par> parList1 = (from x in par
                                                                                  where x.x.TrendId == 1
                                                                                  select new t_account_shares_optional_trend_rel_par
                                                                                  {
                                                                                      CreateTime = DateTime.Now,
                                                                                      LastModified = DateTime.Now,
                                                                                      ParamsInfo = x.x2.ParamsInfo,
                                                                                      RelId = trend1.Id
                                                                                  }).ToList();
                        db.t_account_shares_optional_trend_rel_par.AddRange(parList1);
                        db.SaveChanges();

                        db.t_account_shares_optional_trend_rel_tri.Add(new t_account_shares_optional_trend_rel_tri
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            RelId = trend1.Id,
                            LastPushPrice = null,
                            LastPushRiseRate = null,
                            LastPushTime = null,
                            MinPushRateInterval = Singleton.Instance.MinPushRateInterval,
                            MinPushTimeInterval = Singleton.Instance.MinPushTimeInterval,
                            TriCountToday = 0,
                            MinTodayPrice = -1
                        });
                        db.SaveChanges();


                        t_account_shares_optional_trend_rel trend2 = new t_account_shares_optional_trend_rel
                        {
                            Status = 1,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            OptionalId = addOptional.Id,
                            TrendId = 2
                        };
                        db.t_account_shares_optional_trend_rel.Add(trend2);
                        db.SaveChanges();

                        List<t_account_shares_optional_trend_rel_par> parList2 = (from x in par
                                                                                  where x.x.TrendId == 2
                                                                                  select new t_account_shares_optional_trend_rel_par
                                                                                  {
                                                                                      CreateTime = DateTime.Now,
                                                                                      LastModified = DateTime.Now,
                                                                                      ParamsInfo = x.x2.ParamsInfo,
                                                                                      RelId = trend2.Id
                                                                                  }).ToList();
                        db.t_account_shares_optional_trend_rel_par.AddRange(parList2);
                        db.SaveChanges();

                        db.t_account_shares_optional_trend_rel_tri.Add(new t_account_shares_optional_trend_rel_tri
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            RelId = trend2.Id,
                            LastPushPrice = null,
                            LastPushRiseRate = null,
                            LastPushTime = null,
                            MinPushRateInterval = Singleton.Instance.MinPushRateInterval,
                            MinPushTimeInterval = Singleton.Instance.MinPushTimeInterval,
                            TriCountToday = 0,
                            MinTodayPrice = -1
                        });
                        db.SaveChanges();

                        t_account_shares_optional_trend_rel trend3 = new t_account_shares_optional_trend_rel
                        {
                            Status = 1,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            OptionalId = addOptional.Id,
                            TrendId = 3
                        };
                        db.t_account_shares_optional_trend_rel.Add(trend3);
                        db.SaveChanges();

                        List<t_account_shares_optional_trend_rel_par> parList3 = (from x in par
                                                                                  where x.x.TrendId == 3
                                                                                  select new t_account_shares_optional_trend_rel_par
                                                                                  {
                                                                                      CreateTime = DateTime.Now,
                                                                                      LastModified = DateTime.Now,
                                                                                      ParamsInfo = x.x2.ParamsInfo,
                                                                                      RelId = trend3.Id
                                                                                  }).ToList();
                        db.t_account_shares_optional_trend_rel_par.AddRange(parList3);
                        db.SaveChanges();

                        db.t_account_shares_optional_trend_rel_tri.Add(new t_account_shares_optional_trend_rel_tri
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            RelId = trend3.Id,
                            LastPushPrice = null,
                            LastPushRiseRate = null,
                            LastPushTime = null,
                            MinPushRateInterval = Singleton.Instance.MinPushRateInterval,
                            MinPushTimeInterval = Singleton.Instance.MinPushTimeInterval,
                            TriCountToday = 0,
                            MinTodayPrice = -1
                        });
                        db.SaveChanges();
                    }

                    tran.Commit();
                    return tempList.Count();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 绑定自选股席位
        /// </summary>
        /// <param name="request"></param>
        public void BindFrontAccountOptionalSeat(BindFrontAccountOptionalSeatRequest request)
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                //判断自选股是否存在
                var optional = (from item in db.t_account_shares_optional
                                where item.Id == request.OptionalId
                                select item).FirstOrDefault();
                if (optional == null)
                {
                    throw new WebApiException(400, "自选股不存在");
                }

                var seatRel = (from item in db.t_account_shares_optional_seat_rel
                               where item.OptionalId == request.OptionalId
                               select item).FirstOrDefault();
                if (request.SeatId != -1)
                {
                    //判断席位是否有效
                    var seat = (from item in db.t_account_shares_seat
                                where item.Id == request.SeatId && item.Status == 1 && item.AccountId == optional.AccountId
                                select item).FirstOrDefault();
                    if (seat == null)
                    {
                        throw new WebApiException(400, "无效的席位");
                    }
                    if (seat.ValidEndTime < timeNow)
                    {
                        if (seatRel == null || seatRel.SeatId != request.SeatId)
                        {
                            throw new WebApiException(400, "席位已过期");
                        }
                    }

                    //判断席位是否已被绑定
                    var rel = (from item in db.t_account_shares_optional_seat_rel
                               where item.SeatId == request.SeatId && item.OptionalId != request.OptionalId
                               select item).FirstOrDefault();
                    if (rel != null)
                    {
                        throw new WebApiException(400, "该席位已被其他股票绑定");
                    }

                    if (seatRel == null)
                    {
                        db.t_account_shares_optional_seat_rel.Add(new t_account_shares_optional_seat_rel
                        {
                            SeatId = request.SeatId,
                            OptionalId = request.OptionalId
                        });
                        db.SaveChanges();
                    }
                    else
                    {
                        seatRel.SeatId = request.SeatId;
                        db.SaveChanges();
                    }
                }
                else
                {
                    if (seatRel != null)
                    {
                        db.t_account_shares_optional_seat_rel.Remove(seatRel);
                        db.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// 批量绑定自选股席位
        /// </summary>
        /// <param name="request"></param>
        public int BatchBindFrontAccountOptionalSeat(DetailsRequest request)
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                //查询未绑定自选股
                var optionalList = (from item in db.t_account_shares_optional
                                    join item2 in db.t_account_shares_optional_seat_rel on item.Id equals item2.OptionalId into a from ai in a.DefaultIfEmpty()
                                    where item.AccountId == request.Id && ai == null
                                    select item).ToList();
                //查询未绑定的席位
                var seatList = (from item in db.t_account_shares_seat
                                join item2 in db.t_account_shares_optional_seat_rel on item.Id equals item2.SeatId into a
                                from ai in a.DefaultIfEmpty()
                                where item.AccountId == request.Id && ai == null && item.Status == 1 && item.ValidEndTime >= timeNow
                                select item).ToList();
                int i = 0;
                foreach (var optional in optionalList)
                {
                    if (seatList.Count() > i)
                    {
                        db.t_account_shares_optional_seat_rel.Add(new t_account_shares_optional_seat_rel
                        {
                            SeatId = seatList[i].Id,
                            OptionalId = optional.Id
                        });
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
                db.SaveChanges();
                return i;
            }
        }

        /// <summary>
        /// 查询自选股监控走势
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountOptionalTrendInfo> GetFrontAccountOptionalTrend(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var optionalTrend = from item in db.t_account_shares_optional_trend_rel
                                    join item2 in db.t_shares_monitor_trend on item.TrendId equals item2.Id
                                    join item3 in db.t_account_shares_optional on item.OptionalId equals item3.Id
                                    where item2.Status == 1 && item.OptionalId == request.Id
                                    select new { item, item2 };
                int totalCount = optionalTrend.Count();

                return new PageRes<FrontAccountOptionalTrendInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in optionalTrend
                            orderby item.item.TrendId
                            select new FrontAccountOptionalTrendInfo
                            {
                                Status = item.item.Status,
                                CreateTime = item.item.LastModified,
                                RelId = item.item.Id,
                                TrendDescription = item.item2.Description,
                                TrendId = item.item.TrendId,
                                TrendName = item.item2.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 修改自选股监控走势状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountOptionalTrendStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var optionalTrend = (from item in db.t_account_shares_optional_trend_rel
                                     join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                     where item.Id == request.Id
                                     select item).FirstOrDefault();
                if (optionalTrend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                optionalTrend.Status = request.Status;
                optionalTrend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountOptionalTrendParInfo> GetFrontAccountOptionalTrendPar(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_account_shares_optional_trend_rel_par
                               join item2 in db.t_account_shares_optional_trend_rel on item.RelId equals item2.Id
                               join item3 in db.t_account_shares_optional on item2.OptionalId equals item3.Id
                               where item.RelId == request.Id
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<FrontAccountOptionalTrendParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new FrontAccountOptionalTrendParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        public void AddFrontAccountOptionalTrendPar(AddFrontAccountOptionalTrendParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var optionalTrend = (from item in db.t_account_shares_optional_trend_rel
                                     join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                     where item.Id == request.RelId
                                     select item).FirstOrDefault();
                if (optionalTrend == null)
                {
                    throw new WebApiException(400, "自选股不存在");
                }
                if (optionalTrend.TrendId != 1)
                {
                    var par = (from item in db.t_account_shares_optional_trend_rel_par
                               where item.RelId == request.RelId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_account_shares_optional_trend_rel_par.Add(new t_account_shares_optional_trend_rel_par
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = request.ParamsInfo,
                            RelId = request.RelId
                        });
                    }
                }
                else
                {
                    db.t_account_shares_optional_trend_rel_par.Add(new t_account_shares_optional_trend_rel_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        ParamsInfo = request.ParamsInfo,
                        RelId = request.RelId
                    });
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountOptionalTrendPar(ModifyFrontAccountOptionalTrendParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_account_shares_optional_trend_rel_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                var optional = (from item in db.t_account_shares_optional_trend_rel
                                join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                where item.Id == trendPar.RelId && item.TrendId == 1
                                select item2).FirstOrDefault();
                if (optional == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trendPar.ParamsInfo = request.ParamsInfo;
                trendPar.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        public void DeleteFrontAccountOptionalTrendPar(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_account_shares_optional_trend_rel_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                var optional = (from item in db.t_account_shares_optional_trend_rel
                                join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                where item.Id == trendPar.RelId && item.TrendId == 1
                                select item2).FirstOrDefault();
                if (optional == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_account_shares_optional_trend_rel_par.Remove(trendPar);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询自选股走势再触发配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public FrontAccountOptionalTrendTriInfo GetFrontAccountOptionalTrendTri(DetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var optional = (from item in db.t_account_shares_optional_trend_rel
                                join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                where item.Id == request.Id
                                select item2).FirstOrDefault();
                if (optional == null)
                {
                    return null;
                }

                var relTri = (from item in db.t_account_shares_optional_trend_rel_tri
                              where item.RelId == request.Id
                              select new FrontAccountOptionalTrendTriInfo
                              {
                                  MinPushRateInterval = item.MinPushRateInterval,
                                  MinPushTimeInterval = item.MinPushTimeInterval,
                                  MinTodayPrice = item.MinTodayPrice
                              }).FirstOrDefault();
                return relTri;
            }
        }

        /// <summary>
        /// 设置自选股走势再触发配置
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountOptionalTrendTri(ModifyFrontAccountOptionalTrendTriRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var optional = (from item in db.t_account_shares_optional_trend_rel
                                join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                where item.Id == request.RelId
                                select item2).FirstOrDefault();
                if (optional == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                var relTri = (from item in db.t_account_shares_optional_trend_rel_tri
                              where item.RelId == request.RelId
                              select item).FirstOrDefault();
                if (relTri == null)
                {
                    db.t_account_shares_optional_trend_rel_tri.Add(new t_account_shares_optional_trend_rel_tri
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        LastPushPrice = null,
                        LastPushRiseRate = null,
                        LastPushTime = null,
                        MinPushRateInterval = request.MinPushRateInterval,
                        MinPushTimeInterval = request.MinPushTimeInterval,
                        MinTodayPrice = request.MinTodayPrice,
                        RelId = request.RelId
                    });
                }
                else
                {
                    relTri.MinPushRateInterval = request.MinPushRateInterval;
                    relTri.MinPushTimeInterval = request.MinPushTimeInterval;
                    if (request.MinTodayPrice != -1)
                    {
                        relTri.MinTodayPrice = request.MinTodayPrice;
                    }
                    relTri.LastModified = DateTime.Now;
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量更新监控参数
        /// </summary>
        /// <param name="request"></param>
        public int BatchUpdateOptionalMonitorTrendPar(BatchUpdateOptionalMonitorTrendParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询需要更新的监控股票
                var optional = (from item in db.t_account_shares_optional
                                where item.AccountId == request.AccountId
                                select item).ToList();
                if (!string.IsNullOrEmpty(request.SharesCode))
                {
                    optional = (from item in optional
                                where item.SharesCode.StartsWith(request.SharesCode)
                                select item).ToList();
                }
                int i = 0;
                foreach (var item in optional)
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            //查询参数
                            var result = (from x in db.t_account_shares_optional_trend_rel_par
                                          join x2 in db.t_account_shares_optional_trend_rel on x.RelId equals x2.Id
                                          where x2.OptionalId == item.Id && request.TrendIdList.Contains(x2.TrendId)
                                          select new { x, x2 }).ToList();
                            var par = result.Select(e => e.x).ToList();
                            db.t_account_shares_optional_trend_rel_par.RemoveRange(par);
                            db.SaveChanges();

                            //查询新参数
                            var monitorPar = (from x in db.t_shares_monitor_trend_rel
                                              join x2 in db.t_shares_monitor_trend_rel_par on x.Id equals x2.RelId
                                              join x3 in db.t_shares_monitor on x.MonitorId equals x3.Id
                                              where x3.SharesCode == item.SharesCode && x3.Market == item.Market && request.TrendIdList.Contains(x.TrendId)
                                              select new { x, x2 }).ToList();

                            var trend = result.GroupBy(e => e.x2).Select(e => e.Key).ToList();
                            foreach (var x in trend)
                            {
                                var temp = (from y in monitorPar
                                            where y.x.TrendId == x.TrendId
                                            select new t_account_shares_optional_trend_rel_par
                                            {
                                                CreateTime = DateTime.Now,
                                                LastModified = DateTime.Now,
                                                ParamsInfo = y.x2.ParamsInfo,
                                                RelId = x.Id
                                            }).ToList();
                                db.t_account_shares_optional_trend_rel_par.AddRange(temp);
                            }
                            db.SaveChanges();

                            tran.Commit();
                            i++;
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                        }
                    }
                }
                return i;
            }
        }

        /// <summary>
        /// 查询跟投用户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountInfo> GetFrontAccountFollowList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var account = from item in db.t_account_baseinfo
                              join item2 in db.t_account_follow_rel on item.Id equals item2.FollowAccountId
                              where item2.AccountId == request.Id
                              select new { item, item2 };
                int totalCount = account.Count();

                return new PageRes<FrontAccountInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in account
                            orderby item.item2.CreateTime descending
                            select new FrontAccountInfo
                            {
                                AccountId = item.item.Id,
                                NickName = item.item.NickName,
                                Mobile = item.item.Mobile,
                                OrderIndex=item.item2.OrderIndex,
                                CreateTime = item.item2.CreateTime
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加跟投用户
        /// </summary>
        /// <param name="request"></param>
        public void AddFrontAccountFollow(AddFrontAccountFollowRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    if (request.AccountId == request.FollowAccountId)
                    {
                        throw new WebApiException(400, "不能添加本人为跟投人");
                    }
                    //判断用户是否存在
                    var account = (from item in db.t_account_baseinfo
                                   where item.Id == request.AccountId
                                   select item).FirstOrDefault();
                    if (account == null)
                    {
                        throw new WebApiException(400, "用户不存在");
                    }
                    //判断跟投用户是否存在
                    var followAccount = (from item in db.t_account_baseinfo
                                         join item2 in db.t_account_follow_rel on item.Id equals item2.FollowAccountId into a
                                         from ai in a.DefaultIfEmpty()
                                         where item.Id == request.FollowAccountId
                                         select new { item, ai }).FirstOrDefault();
                    if (followAccount == null)
                    {
                        throw new WebApiException(400, "跟投用户不存在");
                    }
                    followAccount.item.CashStatus = request.CashStatus;
                    followAccount.item.ForbidStatus = request.ForbidStatus;
                    followAccount.item.MultipleChangeStatus = request.MultipleChangeStatus;
                    db.SaveChanges();
                    if (followAccount.ai != null)
                    {
                        if (followAccount.ai.AccountId == request.AccountId)
                        {
                            throw new WebApiException(400, "已绑定该跟投用户");
                        }
                        else
                        {
                            throw new WebApiException(400, "跟投用户已绑定其他操作用户");
                        }
                    }

                    db.t_account_follow_rel.Add(new t_account_follow_rel
                    {
                        AccountId = request.AccountId,
                        FollowAccountId = request.FollowAccountId,
                        CreateTime = DateTime.Now
                    });

                    db.t_account_follow_apply_record.Add(new t_account_follow_apply_record
                    {
                        Status = 3,
                        StatusDes = "",
                        AccountId = request.AccountId,
                        CreateTime = DateTime.Now,
                        FinishTime = DateTime.Now,
                        FollowAccountId = request.FollowAccountId,
                        HandlerTime = DateTime.Now,
                        Type = 2
                    });
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
        /// 编辑跟投用户排序值
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountFollowOrderIndex(ModifyFrontAccountFollowOrderIndexRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var rel = (from item in db.t_account_follow_rel
                           where item.AccountId == request.AccountId && item.FollowAccountId == request.FollowAccountId
                           select item).FirstOrDefault();
                if (rel == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                rel.OrderIndex = request.OrderIndex;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除跟投用户
        /// </summary>
        /// <param name="request"></param>
        public void DeleteFrontAccountFollow(DeleteFrontAccountFollowRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var followAccount = (from item in db.t_account_follow_rel
                                     where item.AccountId == request.AccountId && item.FollowAccountId == request.FollowAccountId
                                     select item).FirstOrDefault();
                if (followAccount == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                db.t_account_follow_rel.Remove(followAccount);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询操盘用户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountInfo> GetFrontAccountMainList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var account = from item in db.t_account_baseinfo
                              join item2 in db.t_account_follow_rel on item.Id equals item2.AccountId
                              where item2.FollowAccountId == request.Id
                              select new { item, item2 };
                int totalCount = account.Count();

                return new PageRes<FrontAccountInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in account
                            orderby item.item2.CreateTime descending
                            select new FrontAccountInfo
                            {
                                AccountId = item.item.Id,
                                NickName = item.item.NickName,
                                Mobile = item.item.Mobile,
                                CreateTime = item.item2.CreateTime
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加操盘用户
        /// </summary>
        /// <param name="request"></param>
        public void AddFrontAccountMain(AddFrontAccountMainRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    //判断用户是否存在
                    var account = (from item in db.t_account_baseinfo
                                   where item.Id == request.AccountId
                                   select item).FirstOrDefault();
                    if (account == null)
                    {
                        throw new WebApiException(400, "用户不存在");
                    }
                    account.CashStatus = request.CashStatus;
                    account.ForbidStatus = request.ForbidStatus;
                    account.MultipleChangeStatus = request.MultipleChangeStatus;
                    db.SaveChanges();
                    if (request.MainAccountId > 0)
                    {
                        if (request.AccountId == request.MainAccountId)
                        {
                            throw new WebApiException(400, "不能添加本人为操盘人");
                        }
                        //判断操盘用户是否存在
                        var mainAccount = (from item in db.t_account_baseinfo
                                           where item.Id == request.MainAccountId
                                           select item).FirstOrDefault();
                        if (mainAccount == null)
                        {
                            throw new WebApiException(400, "操盘用户不存在");
                        }
                    }

                    var follow = (from item in db.t_account_follow_rel
                                  where item.FollowAccountId == request.AccountId
                                  select item).ToList();
                    db.t_account_follow_rel.RemoveRange(follow);

                    if (request.MainAccountId > 0)
                    {
                        db.t_account_follow_rel.Add(new t_account_follow_rel
                        {
                            AccountId = request.MainAccountId,
                            FollowAccountId = request.AccountId,
                            CreateTime = DateTime.Now
                        });

                        db.t_account_follow_apply_record.Add(new t_account_follow_apply_record
                        {
                            Status = 3,
                            StatusDes = "",
                            AccountId = request.MainAccountId,
                            CreateTime = DateTime.Now,
                            FinishTime = DateTime.Now,
                            FollowAccountId = request.AccountId,
                            HandlerTime = DateTime.Now,
                            Type = 2
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
        /// 查询监控股票分组列表
        /// </summary>
        /// <returns></returns>
        public List<AccountOptionalGroupInfo> GetAccountOptionalGroup(DetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var groupList = (from item in db.t_account_shares_optional_group
                                 where item.AccountId == request.Id
                                 orderby item.OrderIndex, item.CreateTime descending
                                 select new AccountOptionalGroupInfo
                                 {
                                     GroupId = item.Id,
                                     GroupName = item.Name,
                                     CreateTime = item.CreateTime,
                                     GroupDescription = item.Description,
                                     OrderIndex = item.OrderIndex,
                                     SharesCount = (from x in db.t_account_shares_optional_group_rel
                                                    join x2 in db.t_account_shares_optional on x.OptionalId equals x2.Id
                                                    join x3 in db.t_shares_monitor on new { x2.Market, x2.SharesCode } equals new { x3.Market, x3.SharesCode }
                                                    join x4 in db.t_shares_all on new { x2.Market, x2.SharesCode } equals new { x4.Market, x4.SharesCode }
                                                    where x.GroupId == item.Id && x2.AccountId == request.Id
                                                    select x2).Count()
                                 }).ToList();
                return groupList;
            }
        }

        /// <summary>
        /// 添加监控股票分组
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountOptionalGroup(AddAccountOptionalGroupRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //判断名称是否存在
                    var groupInfo = (from item in db.t_account_shares_optional_group
                                     where item.AccountId == request.Id && item.Name == request.GroupName
                                     select item).FirstOrDefault();
                    if (groupInfo != null)
                    {
                        throw new WebApiException(400, "分组名称已存在");
                    }
                    t_account_shares_optional_group info = new t_account_shares_optional_group
                    {
                        AccountId = request.Id,
                        CreateTime = DateTime.Now,
                        Description = request.GroupDescription,
                        LastModified = DateTime.Now,
                        Name = request.GroupName,
                        OrderIndex = request.OrderIndex
                    };
                    db.t_account_shares_optional_group.Add(info);
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
        /// 编辑监控股票分组
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountOptionalGroup(ModifyAccountOptionalGroupRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断分组是否存在
                var groupInfo = (from item in db.t_account_shares_optional_group
                                 where item.Id == request.Id
                                 select item).FirstOrDefault();
                if (groupInfo == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }
                //判断名称是否存在
                var groupTemp = (from item in db.t_account_shares_optional_group
                                 where item.AccountId == groupInfo.AccountId && item.Name == request.GroupName && item.Id != request.Id
                                 select item).FirstOrDefault();
                if (groupTemp != null)
                {
                    throw new WebApiException(400, "分组名称已存在");
                }

                groupInfo.Name = request.GroupName;
                groupInfo.Description = request.GroupDescription;
                groupInfo.OrderIndex = request.OrderIndex;
                groupInfo.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除监控股票分组
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountOptionalGroup(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断分组是否存在
                var groupInfo = (from item in db.t_account_shares_optional_group
                                 where item.Id == request.Id
                                 select item).FirstOrDefault();
                if (groupInfo == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }

                db.t_account_shares_optional_group.Remove(groupInfo);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountOptionalGroupRelInfo> GetAccountOptionalGroupRelList(GetAccountOptionalGroupRelListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var groupShares = from item in db.t_account_shares_optional_group_rel
                                  join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                  join item3 in db.t_shares_monitor on new { item2.Market, item2.SharesCode } equals new { item3.Market, item3.SharesCode }
                                  join item4 in db.t_shares_all on new { item2.Market, item2.SharesCode } equals new { item4.Market, item4.SharesCode }
                                  where item.GroupId == request.Id
                                  select new { item, item4 };
                if (!string.IsNullOrEmpty(request.SharesInfo))
                {
                    groupShares = from item in groupShares
                                  where item.item4.SharesName.Contains(request.SharesInfo) || item.item4.SharesCode.Contains(request.SharesInfo) || item.item4.SharesPyjc.StartsWith(request.SharesInfo)
                                  select item;
                }

                int totalCount = groupShares.Count();
                return new PageRes<AccountOptionalGroupRelInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in groupShares
                            orderby item.item.Id descending
                            select new AccountOptionalGroupRelInfo
                            {
                                Id = item.item.Id,
                                SharesCode = item.item4.SharesCode,
                                SharesName = item.item4.SharesName,
                                ValidStartTimeDt = item.item.ValidStartTime,
                                ValidEndTimeDt = item.item.ValidEndTime,
                                GroupIsContinue = item.item.GroupIsContinue
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountOptionalGroupRel(AddAccountOptionalGroupRelRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断自选股是否存在
                var optional = (from item in db.t_account_shares_optional
                                where item.Id == request.OptionalId
                                select item).FirstOrDefault();
                if (optional == null)
                {
                    throw new WebApiException(400, "股票不存在");
                }
                //判断分组是否存在
                var groupInfo = (from item in db.t_account_shares_optional_group
                                 where item.Id == request.GroupId
                                 select item).FirstOrDefault();
                if (groupInfo == null)
                {
                    throw new WebApiException(400, "监控分组不存在");
                }
                //判断是否已经添加
                var groupRel = (from item in db.t_account_shares_optional_group_rel
                                where item.GroupId == request.GroupId && item.OptionalId == request.OptionalId
                                select item).FirstOrDefault();
                if (groupRel != null)
                {
                    throw new WebApiException(400, "股票已添加");
                }

                db.t_account_shares_optional_group_rel.Add(new t_account_shares_optional_group_rel
                {
                    GroupId = request.GroupId,
                    GroupIsContinue = request.GroupIsContinue,
                    OptionalId = request.OptionalId,
                    ValidStartTime = request.ValidStartTime,
                    ValidEndTime = request.ValidEndTime
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountOptionalGroupRel(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {

                var groupRel = (from item in db.t_account_shares_optional_group_rel
                                join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (groupRel == null)
                {
                    throw new WebApiException(400, "数据不添加");
                }
                db.t_account_shares_optional_group_rel.Remove(groupRel);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改监控分组内股票是否持续
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountOptionalGroupRelGroupIsContinue(ModifyAccountOptionalGroupRelGroupIsContinueRequest request)
        {
            using (var db = new meal_ticketEntities())
            {

                var groupRel = (from item in db.t_account_shares_optional_group_rel
                                join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (groupRel == null)
                {
                    throw new WebApiException(400, "数据不添加");
                }
                groupRel.GroupIsContinue = request.GroupIsContinue;
                groupRel.ValidStartTime = request.ValidStartTime;
                groupRel.ValidEndTime = request.ValidEndTime;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量导入监控分组内股票
        /// </summary>
        public int BatchAddAccountOptionalGroupRel(long groupId, bool groupIsContinue, DateTime? ValidStartTime, DateTime? ValidEndTime, List<SharesInfo> sharesList)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断分组是否存在
                var groupInfo = (from item in db.t_account_shares_optional_group
                                 where item.Id == groupId
                                 select item).FirstOrDefault();
                if (groupInfo == null)
                {
                    return 0;
                }
                int i = 0;
                foreach (var shares in sharesList)
                {
                    //判断自选股是否存在
                    var optional = (from item in db.t_account_shares_optional
                                    where item.Market == shares.Market && item.SharesCode == shares.SharesCode && item.AccountId == groupInfo.AccountId
                                    select item).FirstOrDefault();
                    if (optional == null)
                    {
                        continue;
                    }
                    //判断是否已经添加
                    var groupRel = (from item in db.t_account_shares_optional_group_rel
                                    where item.GroupId == groupId && item.OptionalId == optional.Id
                                    select item).FirstOrDefault();
                    if (groupRel != null)
                    {
                        continue;
                    }
                    db.t_account_shares_optional_group_rel.Add(new t_account_shares_optional_group_rel
                    {
                        GroupId = groupId,
                        GroupIsContinue = groupIsContinue,
                        OptionalId = optional.Id,
                        ValidStartTime = ValidStartTime,
                        ValidEndTime = ValidEndTime
                    });
                    i++;
                }
                db.SaveChanges();
                return i;
            }
        }

        /// <summary>
        /// 查询用户杠杆配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountFundmultipleInfo> GetAccountFundmultipleList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var fundmultiple = from item in db.t_shares_limit_fundmultiple
                                   join item2 in db.t_shares_limit_fundmultiple_account on item.Id equals item2.FundmultipleId
                                   where item2.AccountId == request.Id
                                   select new { item, item2 };
                if (request.MaxId > 0)
                {
                    fundmultiple = from item in fundmultiple
                                   where item.item.Id <= request.MaxId
                                   select item;
                }

                int totalCount = fundmultiple.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = fundmultiple.Max(e => e.item.Id);
                }

                var list = (from item in fundmultiple
                            orderby item.item.CreateTime descending
                            select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                return new PageRes<AccountFundmultipleInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in list
                            select new AccountFundmultipleInfo
                            {
                                FundMultiple = item.item.FundMultiple,
                                Priority = item.item.Priority,
                                Range = item.item.Range,
                                MarketName = item.item.MarketName,
                                CreateTime = item.item2.CreateTime,
                                Id = item.item2.Id,
                                LimitKey = item.item.LimitKey,
                                LimitMarket = item.item.LimitMarket,
                                AccountFundMultiple = item.item2.FundMultiple
                            }).ToList()
                };
            }
        }

        /// <summary>
        /// 添加用户杠杆配置
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountFundmultiple(AddAccountFundmultipleRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断用户是否存在
                var account = (from item in db.t_account_baseinfo
                               where item.Id == request.AccountId
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400, "用户不存在");
                }
                //判断杠杆Id是否存在
                var fundmultiple = (from item in db.t_shares_limit_fundmultiple
                                    where item.Id == request.FundmultipleId
                                    select item).FirstOrDefault();
                if (fundmultiple == null)
                {
                    throw new WebApiException(400, "杠杆配置不存在");
                }
                if (request.AccountFundMultiple > fundmultiple.FundMultiple)
                {
                    throw new WebApiException(400, "倍数不能超过系统配置最高倍数:" + fundmultiple.FundMultiple);
                }

                //判断是否已经添加
                var accountFundmultiple = (from item in db.t_shares_limit_fundmultiple_account
                                           where item.AccountId == request.AccountId && item.FundmultipleId == request.FundmultipleId
                                           select item).FirstOrDefault();
                if (accountFundmultiple != null)
                {
                    throw new WebApiException(400, "已添加");
                }

                db.t_shares_limit_fundmultiple_account.Add(new t_shares_limit_fundmultiple_account
                {
                    AccountId = request.AccountId,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    FundMultiple = request.AccountFundMultiple,
                    FundmultipleId = request.FundmultipleId
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑用户杠杆配置
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountFundmultiple(ModifyAccountFundmultipleRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断用户杠杆是否存在
                var accountFundmultiple = (from item in db.t_shares_limit_fundmultiple_account
                                           where item.Id == request.Id
                                           select item).FirstOrDefault();
                if (accountFundmultiple == null)
                {
                    throw new WebApiException(400, "用户杠杆不存在");
                }
                accountFundmultiple.FundMultiple = request.AccountFundMultiple;
                accountFundmultiple.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除用户杠杆配置
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountFundmultiple(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断用户杠杆是否存在
                var accountFundmultiple = (from item in db.t_shares_limit_fundmultiple_account
                                           where item.Id == request.Id
                                           select item).FirstOrDefault();
                if (accountFundmultiple == null)
                {
                    throw new WebApiException(400, "用户杠杆不存在");
                }
                db.t_shares_limit_fundmultiple_account.Remove(accountFundmultiple);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询用户跟投申请列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountFollowApplyInfo> GetAccountFollowApplyList(GetAccountFollowApplyListRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_account_follow_apply_record
                             where item.Type==1
                             select item;
                if (request.Status == -1)
                {
                    result = from item in result
                             where item.Status == 1 || item.Status == 2
                             select item;
                }
                if (request.Status == -2)
                {
                    result = from item in result
                             where item.Status == 3 || item.Status == 4 || item.Status == 5
                             select item;
                }
                if (request.Status != 0 && request.Status != -1 && request.Status != -2)
                {
                    result = from item in result
                             where item.Status == request.Status
                             select item;
                }

                int totalCount = result.Count();

                return new PageRes<AccountFollowApplyInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in result
                            join item2 in db.t_account_baseinfo on item.FollowAccountId equals item2.Id
                            join item3 in db.t_account_follow_rel on item.FollowAccountId equals item3.FollowAccountId into a
                            from ai in a.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new AccountFollowApplyInfo
                            {
                                Status = item.Status,
                                StatusDesc = item.StatusDes,
                                AccountId = ai == null ? 0 : ai.AccountId,
                                FollowAccountId = item.FollowAccountId,
                                CreateTime = item.CreateTime,
                                FinishTime = item.FinishTime,
                                HandlerTime = item.HandlerTime,
                                Id = item.Id,
                                AccountMobile = item2.Mobile,
                                AccountName = item2.NickName,
                                CashStatus=item2.CashStatus,
                                ForbidStatus=item2.ForbidStatus,
                                MultipleChangeStatus=item2.MultipleChangeStatus
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 设置用户跟投申请状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountFollowApplyStatus(ModifyAccountFollowApplyStatusRequest request) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    var apply_record = (from item in db.t_account_follow_apply_record
                                        where item.Id == request.Id
                                        select item).FirstOrDefault();
                    if (apply_record == null)
                    {
                        throw new WebApiException(400, "申请不存在");
                    }

                    if (request.Status == 2)
                    {
                        apply_record.Status = 2;
                        apply_record.HandlerTime = DateTime.Now;
                        db.SaveChanges();
                    }

                    if (request.Status == 4)
                    {
                        apply_record.Status = 4;
                        apply_record.StatusDes = request.StatusDesc;
                        apply_record.FinishTime = DateTime.Now;
                        db.SaveChanges();
                    }

                    if (request.Status == 3)
                    {
                        apply_record.Status = 3;
                        apply_record.FinishTime = DateTime.Now;
                        db.SaveChanges();

                        //判断用户是否存在
                        var account = (from item in db.t_account_baseinfo
                                       where item.Id == apply_record.FollowAccountId
                                       select item).FirstOrDefault();
                        if (account == null)
                        {
                            throw new WebApiException(400, "用户不存在");
                        }
                        account.CashStatus = request.CashStatus;
                        account.ForbidStatus = request.ForbidStatus;
                        db.SaveChanges();
                        if (request.MainAccountId > 0)
                        {
                            if (apply_record.FollowAccountId == request.MainAccountId)
                            {
                                throw new WebApiException(400, "不能添加本人为操盘人");
                            }
                            //判断操盘用户是否存在
                            var mainAccount = (from item in db.t_account_baseinfo
                                               where item.Id == request.MainAccountId
                                               select item).FirstOrDefault();
                            if (mainAccount == null)
                            {
                                throw new WebApiException(400, "操盘用户不存在");
                            }
                        }

                        var follow = (from item in db.t_account_follow_rel
                                      where item.FollowAccountId == apply_record.FollowAccountId
                                      select item).ToList();
                        db.t_account_follow_rel.RemoveRange(follow);

                        if (request.MainAccountId > 0)
                        {
                            db.t_account_follow_rel.Add(new t_account_follow_rel
                            {
                                AccountId = request.MainAccountId,
                                FollowAccountId = apply_record.FollowAccountId,
                                CreateTime = DateTime.Now
                            });
                        }
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
        #endregion

        #region====交易管理====
        /// <summary>
        /// 查询今日交易统计信息
        /// </summary>
        /// <returns></returns>
        public FrontAccountSharesTradeTodayStatisticsInfo GetFrontAccountSharesTradeTodayStatistics()
        {
            DateTime timeDate = DateTime.Now.Date;
            do
            {
                if (Helper.CheckTradeDate(timeDate))
                {
                    break;
                }
                timeDate = timeDate.AddDays(-1);

            } while (true);
            using (var db = new meal_ticketEntities())
            {
                var entrustList = (from item in db.t_account_shares_entrust
                                   where item.CreateTime > timeDate && item.Status == 3
                                   select item).ToList();
                var buyEntrustList = (from item in entrustList
                                      where item.TradeType == 1
                                      select item).ToList();
                var sellEntrustList = (from item in entrustList
                                       where item.TradeType == 2
                                       select item).ToList();
                int TradeBuySharesCount = 0;
                long TradeSharesCount = 0;
                long TradeSoldAmount = 0;
                long TradeAmount = 0;
                int TradeSoldSharesCount = 0;
                long TradeBuyAmount = 0;
                int TradeFinishCount = 0;
                if (entrustList.Count() > 0)
                {
                    TradeFinishCount = entrustList.Count();
                    TradeSharesCount = entrustList.Sum(e => e.DealCount);
                    TradeAmount = entrustList.Sum(e => e.DealAmount);

                }
                if (buyEntrustList.Count() > 0)
                {
                    TradeBuySharesCount = buyEntrustList.Sum(e => e.DealCount);
                    TradeBuyAmount = buyEntrustList.Sum(e => e.DealAmount);
                }
                if (sellEntrustList.Count() > 0)
                {
                    TradeSoldSharesCount = sellEntrustList.Sum(e => e.DealCount);
                    TradeSoldAmount = sellEntrustList.Sum(e => e.DealAmount);
                }
                return new FrontAccountSharesTradeTodayStatisticsInfo
                {
                    TradeBuySharesCount = TradeBuySharesCount,
                    TradeSharesCount = TradeSharesCount,
                    TradeSoldAmount = TradeSoldAmount,
                    TradeAmount = TradeAmount,
                    TradeSoldSharesCount = TradeSoldSharesCount,
                    TradeBuyAmount = TradeBuyAmount,
                    TradeFinishCount = TradeFinishCount
                };
            }
        }

        /// <summary>
        /// 查询交易排行列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSharesTradeRankInfo> GetFrontAccountSharesTradeRankList(GetFrontAccountSharesTradeRankListRequest request)
        {
            DateTime timeNow = DateTime.Now;
            if (request.TimeType == 1)
            {
                do
                {
                    if (Helper.CheckTradeDate(timeNow)) 
                    {
                        break;
                    }
                    timeNow = timeNow.AddDays(-1);

                } while (true);

                request.StartTime = timeNow.Date;
                request.EndTime = timeNow.Date.AddDays(1);
            }
            if (request.TimeType == 2)
            {
                request.StartTime = timeNow.AddDays(-7).Date;
                request.EndTime = timeNow.Date;
            }
            if (request.TimeType == 3)
            {
                request.StartTime = timeNow.AddMonths(-1).Date;
                request.EndTime = timeNow.Date;
            }
            if (request.TimeType == 4)
            {
                request.StartTime = timeNow.AddMonths(-3).Date;
                request.EndTime = timeNow.Date;
            }
            if (request.TimeType == 5)
            {
                if (request.StartTime == null || request.EndTime == null)
                {
                    throw new WebApiException(400, "时间参数错误");
                }
            }
            using (var db = new meal_ticketEntities())
            {
                var entrustList = from item in db.t_account_shares_entrust
                                  where item.Status == 3 && item.CreateTime >= request.StartTime && item.CreateTime < request.EndTime && item.DealCount > 0
                                  select item;

                var list = from item in entrustList
                           group item by item.AccountId into g
                           select new
                           {
                               AccountId = g.Key,
                               TradeAmount = g.Sum(e => e.DealAmount),
                               BuyAmount = g.Sum(e => e.TradeType == 1 ? e.DealAmount : 0),
                               SellAmount = g.Sum(e => e.TradeType == 2 ? e.DealAmount : 0),
                               TradeCount = g.Count(),
                               LastTradeTime = g.Max(e => e.CreateTime)
                           };


                int totalCount = list.Count();
                List<FrontAccountSharesTradeRankInfo> result = new List<FrontAccountSharesTradeRankInfo>();
                if (request.OrderType == 2)
                {
                    if (request.OrderMethod == "ascending")
                    {
                        result = (from item in list
                                  join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id into a
                                  from ai in a.DefaultIfEmpty()
                                  orderby item.BuyAmount
                                  select new FrontAccountSharesTradeRankInfo
                                  {
                                      SellAmount = item.SellAmount,
                                      AccountMobile = ai == null ? "" : ai.Mobile,
                                      AccountName = ai == null ? "" : ai.NickName,
                                      BuyAmount = item.BuyAmount,
                                      LastTradeTime = item.LastTradeTime,
                                      TradeAmount = item.TradeAmount,
                                      TradeCount = item.TradeCount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                    }
                    else
                    {
                        result = (from item in list
                                  join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id into a
                                  from ai in a.DefaultIfEmpty()
                                  orderby item.BuyAmount descending
                                  select new FrontAccountSharesTradeRankInfo
                                  {
                                      SellAmount = item.SellAmount,
                                      AccountMobile = ai == null ? "" : ai.Mobile,
                                      AccountName = ai == null ? "" : ai.NickName,
                                      BuyAmount = item.BuyAmount,
                                      LastTradeTime = item.LastTradeTime,
                                      TradeAmount = item.TradeAmount,
                                      TradeCount = item.TradeCount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                    }
                }
                else if (request.OrderType == 3)
                {
                    if (request.OrderMethod == "ascending")
                    {
                        result = (from item in list
                                  join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id into a
                                  from ai in a.DefaultIfEmpty()
                                  orderby item.SellAmount
                                  select new FrontAccountSharesTradeRankInfo
                                  {
                                      SellAmount = item.SellAmount,
                                      AccountMobile = ai == null ? "" : ai.Mobile,
                                      AccountName = ai == null ? "" : ai.NickName,
                                      BuyAmount = item.BuyAmount,
                                      LastTradeTime = item.LastTradeTime,
                                      TradeAmount = item.TradeAmount,
                                      TradeCount = item.TradeCount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                    }
                    else
                    {

                        result = (from item in list
                                  join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id into a
                                  from ai in a.DefaultIfEmpty()
                                  orderby item.SellAmount descending
                                  select new FrontAccountSharesTradeRankInfo
                                  {
                                      SellAmount = item.SellAmount,
                                      AccountMobile = ai == null ? "" : ai.Mobile,
                                      AccountName = ai == null ? "" : ai.NickName,
                                      BuyAmount = item.BuyAmount,
                                      LastTradeTime = item.LastTradeTime,
                                      TradeAmount = item.TradeAmount,
                                      TradeCount = item.TradeCount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                    }
                }
                else if (request.OrderType == 4)
                {
                    if (request.OrderMethod == "ascending")
                    {
                        result = (from item in list
                                  join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id into a
                                  from ai in a.DefaultIfEmpty()
                                  orderby item.TradeCount
                                  select new FrontAccountSharesTradeRankInfo
                                  {
                                      SellAmount = item.SellAmount,
                                      AccountMobile = ai == null ? "" : ai.Mobile,
                                      AccountName = ai == null ? "" : ai.NickName,
                                      BuyAmount = item.BuyAmount,
                                      LastTradeTime = item.LastTradeTime,
                                      TradeAmount = item.TradeAmount,
                                      TradeCount = item.TradeCount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                    }
                    else
                    {

                        result = (from item in list
                                  join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id into a
                                  from ai in a.DefaultIfEmpty()
                                  orderby item.TradeCount descending
                                  select new FrontAccountSharesTradeRankInfo
                                  {
                                      SellAmount = item.SellAmount,
                                      AccountMobile = ai == null ? "" : ai.Mobile,
                                      AccountName = ai == null ? "" : ai.NickName,
                                      BuyAmount = item.BuyAmount,
                                      LastTradeTime = item.LastTradeTime,
                                      TradeAmount = item.TradeAmount,
                                      TradeCount = item.TradeCount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                    }
                }
                else
                {
                    if (request.OrderMethod == "ascending")
                    {
                        result = (from item in list
                                  join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id into a
                                  from ai in a.DefaultIfEmpty()
                                  orderby item.TradeAmount
                                  select new FrontAccountSharesTradeRankInfo
                                  {
                                      SellAmount = item.SellAmount,
                                      AccountMobile = ai == null ? "" : ai.Mobile,
                                      AccountName = ai == null ? "" : ai.NickName,
                                      BuyAmount = item.BuyAmount,
                                      LastTradeTime = item.LastTradeTime,
                                      TradeAmount = item.TradeAmount,
                                      TradeCount = item.TradeCount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                    }
                    else
                    {
                        result = (from item in list
                                  join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id into a
                                  from ai in a.DefaultIfEmpty()
                                  orderby item.TradeAmount descending
                                  select new FrontAccountSharesTradeRankInfo
                                  {
                                      SellAmount = item.SellAmount,
                                      AccountMobile = ai == null ? "" : ai.Mobile,
                                      AccountName = ai == null ? "" : ai.NickName,
                                      BuyAmount = item.BuyAmount,
                                      LastTradeTime = item.LastTradeTime,
                                      TradeAmount = item.TradeAmount,
                                      TradeCount = item.TradeCount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                    }

                }
                return new PageRes<FrontAccountSharesTradeRankInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = result
                };
            }
        }

        /// <summary>
        /// 查询前端账户持仓统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSharesHoldStatisticsInfo> GetFrontAccountSharesHoldStatistics(GetFrontAccountSharesHoldStatisticsRequest request)
        {
            DateTime dateNow = DateTime.Now.Date;
            TimeSpan timeSpanNow = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
            using (var db = new meal_ticketEntities())
            {
                var hold = from item in db.t_account_shares_hold
                           join item2 in db.t_shares_quotes on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                           from ai in a.DefaultIfEmpty()
                           join item3 in db.t_account_baseinfo on item.AccountId equals item3.Id
                           where item.Status == 1 && (item.RemainCount > 0 || item.LastModified > dateNow)
                           group new { item, ai, item3 } by item3 into g
                           select g;
                if (!string.IsNullOrEmpty(request.AccountInfo))
                {
                    hold = from item in hold
                           where item.Key.NickName.Contains(request.AccountInfo) || item.Key.Mobile.Contains(request.AccountInfo)
                           select item;
                }
                int totalCount = hold.Count();
                var List = (from item in hold
                            orderby item.Key.CreateTime descending
                            select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                List<FrontAccountSharesHoldStatisticsInfo> result = new List<FrontAccountSharesHoldStatisticsInfo>();
                foreach (var item in List)
                {
                    long TotalMarketValue = 0;
                    long remainValue = 0;//剩余成本
                    long totalDeposit = 0;
                    long initialDeposit = 0;
                    long initialFundAmount = 0;
                    long totalFundAmount = 0;
                    int cordonCount = 0;
                    int closingCount = 0;
                    var temp = (from x in item
                                where x.ai != null
                                select x).ToList();
                    if (temp.Count() > 0)
                    {
                        TotalMarketValue = temp.Sum(e => (e.ai.PresentPrice <= 0 ? e.ai.ClosedPrice : e.ai.PresentPrice) * e.item.RemainCount);
                        remainValue = temp.Sum(e => e.item.BuyTotalAmount - e.item.SoldAmount);
                        totalDeposit = temp.Sum(e => e.item.RemainDeposit);
                        totalFundAmount = temp.Sum(e => e.item.FundAmount);
                        initialDeposit = temp.Sum(e => e.item.InitialDeposit);
                        initialFundAmount = temp.Sum(e => e.item.InitialFundAmount);
                    }

                    foreach (var x in temp)
                    {
                        long ClosingPrice = 0;
                        long CordonPrice = 0;
                        long AddDepositAmount = 0;
                        int Cordon = 0;
                        int Closing = 0;
                        var sharesName = (from y in db.t_shares_all
                                          where y.Market == x.item.Market && y.SharesCode == x.item.SharesCode
                                          select y.SharesName).FirstOrDefault();
                        if (sharesName == null)
                        {
                            sharesName = "";
                        }
                        long currPrice = x.ai.PresentPrice <= 0 ? x.ai.ClosedPrice : x.ai.PresentPrice;
                        GetCurrTimeClosingLine(x.item.Market, x.item.SharesCode, sharesName, out Closing, out Cordon);
                        CalculateClosingInfo(currPrice, x.item.RemainCount,x.item.RemainDeposit,x.item.FundAmount, Cordon, Closing,out ClosingPrice,out AddDepositAmount,out CordonPrice);
                        if (currPrice < ClosingPrice)
                        {
                            closingCount++;
                        }
                        else if (currPrice < CordonPrice)
                        {
                            cordonCount++;
                        }
                    }

                    result.Add(new FrontAccountSharesHoldStatisticsInfo
                    {
                        AccountId = item.Key.Id,
                        AccountMobile = item.Key.Mobile,
                        AccountName = item.Key.NickName,
                        HoldCount = item.Count(),
                        TotalMarketValue = TotalMarketValue,
                        TotalProfit = TotalMarketValue - remainValue,
                        TotalDeposit = totalDeposit,
                        TotalFundAmount = totalFundAmount,
                        InitialDeposit = initialDeposit,
                        InitialFundAmount = initialFundAmount,
                        ClosingCount= closingCount,
                        CordonCount= cordonCount
                    });
                }

                return new PageRes<FrontAccountSharesHoldStatisticsInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = result
                };
            }
        }

        /// <summary>
        /// 查询前端账户持仓列表
        /// </summary>
        /// <returns></returns>
        public PageRes<FrontAccountSharesHoldInfo> GetFrontAccountSharesHoldList(DetailsPageRequest request)
        {
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var hold = from item in db.t_account_shares_hold
                           where item.AccountId == request.Id && item.Status == 1 && (item.RemainCount > 0 || item.LastModified > dateNow)
                           select item;
                if (request.MaxId > 0)
                {
                    hold = from item in hold
                           where item.Id <= request.MaxId
                           select item;
                }
                int totalCount = hold.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = hold.Max(e => e.Id);
                }
                var List = (from item in hold
                            join item2 in db.t_shares_quotes on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                            from ai in a.DefaultIfEmpty()
                            join item3 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode }
                            orderby item.CreateTime descending
                            let PresentPrice= (ai == null ? 0 : (ai.PresentPrice <= 0 ? ai.ClosedPrice : ai.PresentPrice))
                            select new FrontAccountSharesHoldInfo
                            {
                                SharesCode = item.SharesCode,
                                CanSoldCount = item.CanSoldCount,
                                CostPrice = item.RemainCount <= 0 ? (item.BuyTotalAmount / item.BuyTotalCount) : (item.BuyTotalAmount - item.SoldAmount) / item.RemainCount,
                                Id = item.Id,
                                Market = item.Market,
                                RemainCount = item.RemainCount,
                                MarketValue = PresentPrice * item.RemainCount,
                                PresentPrice = PresentPrice,
                                ProfitAmount = (PresentPrice * item.RemainCount) + item.SoldAmount - item.BuyTotalAmount,
                                SharesName = item3.SharesName,
                                RemainDeposit = item.RemainDeposit,
                                RemainFundAmount = item.FundAmount,
                                InitialDeposit = item.InitialDeposit,
                                InitialFundAmount = item.InitialFundAmount
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                foreach (var item in List)
                {
                    int closing = 0;
                    int cordon = 0;
                    GetCurrTimeClosingLine(item.Market, item.SharesCode, item.SharesName, out closing, out cordon);
                    long closingPrice = 0;
                    long addDepositAmount = 0;
                    long cordonPrice = 0;
                    CalculateClosingInfo(item.PresentPrice, item.RemainCount, item.RemainDeposit, item.RemainFundAmount, cordon, closing, out closingPrice, out addDepositAmount, out cordonPrice);
                    item.ColsingPrice = closingPrice;
                    item.AddDepositAmount = addDepositAmount;
                }
                return new PageRes<FrontAccountSharesHoldInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List= List
                };
            }
        }

        /// <summary>
        /// 获取当前时段平仓线信息
        /// </summary>
        private static void GetCurrTimeClosingLine(int market, string sharesCode, string sharesName, out int currClosingLine, out int currCordon)
        {
            TimeSpan timeSpan = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));

            long currId = 0;
            int allDayCordon = 0;
            int allDayClosingLine = 0;
            currClosingLine = 0;
            currCordon = 0;

            using (var db = new meal_ticketEntities())
            {
                var traderulesList = (from x in db.t_shares_limit_traderules
                                      where (x.LimitMarket == -1 || x.LimitMarket == market) && ((x.LimitType == 1 && sharesCode.StartsWith(x.LimitKey)) || (x.LimitType == 2 && sharesName.StartsWith(x.LimitKey))) && x.Status == 1
                                      select x).ToList();
                foreach (var item in traderulesList)
                {
                    if (allDayClosingLine < item.ClosingLine)
                    {
                        allDayClosingLine = item.ClosingLine;
                        allDayCordon = item.Cordon;
                    }
                    //查询额外强制平仓线
                    var traderulesOther = (from x in db.t_shares_limit_traderules_other
                                           where x.RulesId == item.Id && x.Status == 1
                                           select x).ToList();
                    TimeSpan? tempCurrStartTime = null;
                    TimeSpan? tempCurrEndTime = null;
                    long tempCurrId = 0;
                    int tempCurrClosingLine = 0;
                    int tempCurrCordon = 0;
                    foreach (var other in traderulesOther)
                    {
                        try
                        {
                            TimeSpan startTime = TimeSpan.Parse(other.Times.Split('-')[0]);
                            TimeSpan endTime = TimeSpan.Parse(other.Times.Split('-')[1]);
                            if (timeSpan >= startTime && timeSpan < endTime)
                            {
                                if (tempCurrClosingLine < other.ClosingLine)
                                {
                                    tempCurrStartTime = startTime;
                                    tempCurrEndTime = endTime;
                                    tempCurrClosingLine = other.ClosingLine;
                                    tempCurrCordon = other.Cordon;
                                    tempCurrId = other.Id;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                    if (currClosingLine < tempCurrClosingLine)
                    {
                        currId = tempCurrId;
                        currClosingLine = tempCurrClosingLine;
                        currCordon = tempCurrCordon;
                    }
                }
                if (currId == 0)
                {
                    currClosingLine = allDayClosingLine;
                    currCordon = allDayCordon;
                }
            }
        }

        /// <summary>
        /// 计算平仓信息
        /// </summary>
        private void CalculateClosingInfo(long CurrPrice, int RemainCount, long RemainDeposit, long RemainFundAmount, int Cordon, int ClosingLine, out long ClosingPrice, out long AddDepositAmount, out long CordonPrice)
        {
            //计算平仓信息
            //（市值+保证金-借钱）/借钱=价格线
            ClosingPrice = RemainCount <= 0 ? 0 : (long)(ClosingLine * 1.0 / 10000 * RemainFundAmount + RemainFundAmount - RemainDeposit) / RemainCount;
            CordonPrice = RemainCount <= 0 ? 0 : (long)(Cordon * 1.0 / 10000 * RemainFundAmount + RemainFundAmount - RemainDeposit) / RemainCount;
            long needDepositAmount = (long)(Cordon * 1.0 / 10000 * RemainFundAmount + RemainFundAmount - CurrPrice * RemainCount);
            AddDepositAmount = needDepositAmount - RemainDeposit < 0 ? 0 : needDepositAmount - RemainDeposit;
        }

        /// <summary>
        /// 前端账户持仓卖出
        /// </summary>
        public void SellFrontAccountSharesHold(SellFrontAccountSharesHoldRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                var hold = (from item in db.t_account_shares_hold
                            where item.Id == request.HoldId
                            select item).FirstOrDefault();
                if (hold == null)
                {
                    throw new WebApiException(400, "持仓不存在");
                }
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter sellIdDb = new ObjectParameter("sellId", 0);
                    db.P_ApplyTradeSell(hold.AccountId, request.HoldId, request.SellCount, request.SellType, request.SellPrice, 3,false, errorCodeDb, errorMessageDb, sellIdDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    long sellId = (long)sellIdDb.Value;

                    var entrustManager = (from item in db.t_account_shares_entrust_manager
                                          join item2 in db.t_broker_account_info on item.TradeAccountCode equals item2.AccountCode
                                          where item.BuyId == sellId && item.TradeType == 2 && item.Status == 1
                                          select new { item, item2 }).ToList();
                    if (entrustManager.Count() <= 0)
                    {
                        throw new WebApiException(400, "内部错误");
                    }
                    foreach (var item in entrustManager)
                    {
                        var sendData = new
                        {
                            SellManagerId = item.item.Id,
                            SellTime = DateTime.Now.ToString("yyyy-MM-dd")
                        };

                        var server = (from x in db.t_server_broker_account_rel
                                      join x2 in db.t_server on x.ServerId equals x2.ServerId
                                      where x.BrokerAccountId == item.item2.Id
                                      select x).FirstOrDefault();
                        if (server == null)
                        {
                            throw new WebApiException(400, "服务器配置有误");
                        }

                        bool isSendSuccess = MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesSell", server.ServerId);
                        if (!isSendSuccess)
                        {
                            throw new WebApiException(400, "操作超时，请重新操作");
                        }
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
        /// 查询前端账户交易记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSharesTradeRecordInfo> GetFrontAccountSharesTradeRecordList(GetFrontAccountSharesTradeRecordListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var entrustList = from item in db.t_account_shares_entrust
                                  join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id
                                  join item3 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode }
                                  select new { item, item2, item3 };
                if (!string.IsNullOrEmpty(request.AccountInfo))
                {
                    entrustList = from item in entrustList
                                  where item.item2.NickName.Contains(request.AccountInfo) || item.item2.Mobile.Contains(request.AccountInfo)
                                  select item;
                }
                if (request.TradeType != 0)
                {
                    entrustList = from item in entrustList
                                  where item.item.TradeType == request.TradeType
                                  select item;
                }
                if (request.EntrustStatus != 0)
                {
                    if (request.EntrustStatus == 1)
                    {
                        entrustList = from item in entrustList
                                      where item.item.Status == 1
                                      select item;
                    }
                    if (request.EntrustStatus == 2)
                    {
                        entrustList = from item in entrustList
                                      where (item.item.Status == 2 || item.item.Status == 4) && item.item.DealCount <= 0
                                      select item;
                    }
                    if (request.EntrustStatus == 21)
                    {
                        entrustList = from item in entrustList
                                      where (item.item.Status == 2 || item.item.Status == 4) && item.item.DealCount > 0
                                      select item;
                    }
                    if (request.EntrustStatus == 3)
                    {
                        entrustList = from item in entrustList
                                      where item.item.Status == 3 && item.item.DealCount >= item.item.EntrustCount
                                      select item;
                    }
                    if (request.EntrustStatus == 31)
                    {
                        entrustList = from item in entrustList
                                      where item.item.Status == 3 && item.item.DealCount < item.item.EntrustCount && item.item.DealCount > 0
                                      select item;
                    }
                    if (request.EntrustStatus == 32)
                    {
                        entrustList = from item in entrustList
                                      where item.item.Status == 3 && item.item.DealCount <= 0
                                      select item;
                    }
                }

                if (!string.IsNullOrEmpty(request.SharesInfo))
                {
                    entrustList = from item in entrustList
                                  where item.item3.SharesCode.Contains(request.SharesInfo) || item.item3.SharesName.Contains(request.SharesInfo)
                                  select item;
                }
                if (request.StartTime != null && request.EndTime != null)
                {
                    entrustList = from item in entrustList
                                  where item.item.CreateTime >= request.StartTime && item.item.CreateTime < request.EndTime
                                  select item;
                }

                int totalCount = entrustList.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = entrustList.Max(e => e.item.Id);
                }

                return new PageRes<FrontAccountSharesTradeRecordInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in entrustList
                            orderby item.item.CreateTime descending
                            select new FrontAccountSharesTradeRecordInfo
                            {
                                SharesName = item.item3.SharesName,
                                AccountMobile = item.item2.Mobile,
                                AccountName = item.item2.NickName,
                                CreateTime = item.item.CreateTime,
                                DealAmount = item.item.DealAmount,
                                DealCount = item.item.DealCount,
                                DealPriceAvg = item.item.DealPrice,
                                EntrustCount = item.item.EntrustCount,
                                EntrustType=item.item.EntrustType,
                                EntrustPrice=item.item.EntrustPrice,
                                Id = item.item.Id,
                                Remark = item.item.StatusDes,
                                TradeType = item.item.TradeType,
                                SharesCode = item.item.SharesCode,
                                FinishTime=item.item.LastModified,
                                EntrustStatus = item.item.Status == 1 ? 1 : (item.item.Status == 2 || item.item.Status == 4 || item.item.Status == 5) ? (item.item.DealCount <= 0 ? 2 : 21) : (item.item.DealCount <= 0 ? 32 : item.item.DealCount >= item.item.EntrustCount ? 3 : 31)
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 前端账户交易撤单
        /// </summary>
        /// <param name="request"></param>
        public void CancelFrontAccountSharesTrade(CancelFrontAccountSharesTradeRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                var entrust = (from item in db.t_account_shares_entrust
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (entrust == null)
                {
                    throw new WebApiException(400, "委托Id不存在");
                }
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_ApplyTradeCancel(entrust.AccountId, request.Id, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }

                    var entrustManager = (from item in db.t_account_shares_entrust_manager
                                          join item2 in db.t_broker_account_info on item.TradeAccountCode equals item2.AccountCode
                                          where item.BuyId == request.Id
                                          select new { item, item2 }).ToList();
                    if (entrustManager.Count() <= 0)
                    {
                        var sendData = new
                        {
                            TradeManagerId = -1,
                            EntrustId = request.Id,
                            CancelTime = DateTime.Now.ToString("yyyy-MM-dd")
                        };
                        bool isSendSuccess = MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesCancel", "s1");
                    }
                    else
                    {
                        foreach (var item in entrustManager)
                        {
                            var sendData = new
                            {
                                TradeManagerId = item.item.Id,
                                EntrustId = request.Id,
                                CancelTime = DateTime.Now.ToString("yyyy-MM-dd")
                            };

                            var server = (from x in db.t_server_broker_account_rel
                                          join x2 in db.t_server on x.ServerId equals x2.ServerId
                                          where x.BrokerAccountId == item.item2.Id
                                          select x).FirstOrDefault();
                            if (server == null)
                            {
                                throw new WebApiException(400, "服务器配置有误");
                            }

                            bool isSendSuccess = MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesCancel", server.ServerId);
                            if (!isSendSuccess)
                            {
                                throw new WebApiException(400, "操作超时，请重新操作");
                            }
                        }
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
        /// 查询前端账户交易成交记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSharesTradeDealRecordInfo> GetFrontAccountSharesTradeDealRecordList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var dealList = from item in db.t_account_shares_entrust
                               join item2 in db.t_account_shares_entrust_manager on item.Id equals item2.BuyId
                               join item3 in db.t_account_shares_entrust_manager_dealdetails on item2.Id equals item3.EntrustManagerId
                               where item.Id == request.Id
                               select new { item, item2, item3 };
                if (request.MaxId > 0)
                {
                    dealList = from item in dealList
                               where item.item3.Id <= request.MaxId
                               select item;
                }
                int totalCount = dealList.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = dealList.Max(e => e.item3.Id);
                }

                return new PageRes<FrontAccountSharesTradeDealRecordInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in dealList
                            orderby item.item3.DealTime descending
                            select new FrontAccountSharesTradeDealRecordInfo
                            {
                                DealAmount = item.item3.DealAmount,
                                DealCount = item.item3.DealCount,
                                DealPrice=item.item3.DealPrice,
                                DealTime = item.item3.DealTime,
                                EntrustCount = item.item2.EntrustCount,
                                EntrustId = item.item2.EntrustId,
                                TradeAccountCode = item.item2.TradeAccountCode,
                                Id = item.item3.Id
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询前端账户盈亏统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSharesProfitStatisticsInfo> GetFrontAccountSharesProfitStatistics(GetFrontAccountSharesProfitStatisticsRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var hold = from item in db.t_account_shares_hold
                           join item2 in db.t_shares_quotes on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                           from ai in a.DefaultIfEmpty()
                           join item3 in db.t_account_baseinfo on item.AccountId equals item3.Id
                           where item.Status > 0
                           group new { item, ai, item3 } by item3 into g
                           select g;
                if (!string.IsNullOrEmpty(request.AccountInfo))
                {
                    hold = from item in hold
                           where item.Key.NickName.Contains(request.AccountInfo) || item.Key.Mobile.Contains(request.AccountInfo)
                           select item;
                }
                int totalCount = hold.Count();
                var List = (from item in hold
                            orderby item.Key.CreateTime descending
                            select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                List<FrontAccountSharesProfitStatisticsInfo> result = new List<FrontAccountSharesProfitStatisticsInfo>();
                foreach (var item in List)
                {
                    long TotalMarketValue = 0;
                    long remainValue = 0;//剩余成本
                    long initialDeposit = 0;
                    long initialFundAmount = 0;
                    long totalDeposit = 0;
                    long totalFundAmount = 0;

                    long serviceFee = 0;
                    long settlementFee = 0;
                    long buyService = 0;
                    long stampFee = 0;
                    long administrationFee = 0;
                    long commission = 0;
                    long profitService = 0;
                    long handlingFee = 0;
                    long transferFee = 0;

                    List<long> holdIdList = new List<long>();
                    var temp = (from x in item
                                where x.ai != null
                                select x).ToList();
                    if (temp.Count() > 0)
                    {
                        TotalMarketValue = temp.Sum(e => (e.ai.PresentPrice <= 0 ? e.ai.ClosedPrice : e.ai.PresentPrice) * e.item.RemainCount);
                        remainValue = temp.Sum(e => e.item.BuyTotalAmount - e.item.SoldAmount);
                        holdIdList = temp.Select(e => e.item.Id).Distinct().ToList();
                        totalDeposit = temp.Sum(e => e.item.RemainDeposit);
                        totalFundAmount = temp.Sum(e => e.item.FundAmount);
                        initialDeposit = temp.Sum(e => e.item.InitialDeposit);
                        initialFundAmount = temp.Sum(e => e.item.InitialFundAmount);
                        serviceFee = temp.Sum(e => e.item.ServiceFee);
                    }

                    DateTime? lastTradeTime = null;
                    int lastTradeType = 0;
                    var entrustList = (from x in db.t_account_shares_entrust
                                       where x.Status == 3 && x.DealCount > 0 && holdIdList.Contains(x.HoldId)
                                       select x).ToList();
                    if (entrustList.Count() > 0)
                    {
                        var lastEntrust = entrustList.OrderByDescending(e => e.CreateTime).First();
                        lastTradeTime = lastEntrust.CreateTime;
                        lastTradeType = lastEntrust.TradeType;

                        settlementFee = entrustList.Sum(e => e.SettlementFee);
                        buyService = entrustList.Sum(e => e.BuyService);
                        stampFee = entrustList.Sum(e => e.StampFee);
                        administrationFee = entrustList.Sum(e => e.AdministrationFee);
                        commission = entrustList.Sum(e => e.Commission);
                        profitService = entrustList.Sum(e => e.ProfitService);
                        handlingFee = entrustList.Sum(e => e.HandlingFee);
                        transferFee = entrustList.Sum(e => e.TransferFee);
                    }

                    result.Add(new FrontAccountSharesProfitStatisticsInfo
                    {
                        AccountId = item.Key.Id,
                        AccountMobile = item.Key.Mobile,
                        AccountName = item.Key.NickName,
                        HoldCount = item.Count(),
                        TotalProfit = TotalMarketValue - remainValue,
                        LastTradeTime = lastTradeTime,
                        LastTradeType = lastTradeType,
                        TotalDeposit = totalDeposit,
                        TotalFundAmount = totalFundAmount,
                        InitialDeposit = initialDeposit,
                        InitialFundAmount = initialFundAmount,
                        ServiceFee = serviceFee,
                        SettlementFee = settlementFee,
                        BuyService = buyService,
                        StampFee = stampFee,
                        AdministrationFee = administrationFee,
                        Commission = commission,
                        ProfitService = profitService,
                        HandlingFee = handlingFee,
                        TransferFee = transferFee,
                    });
                }

                return new PageRes<FrontAccountSharesProfitStatisticsInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = result
                };
            }
        }

        /// <summary>
        /// 查询前端账户盈亏记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSharesProfitRecordInfo> GetFrontAccountSharesProfitRecord(GetFrontAccountSharesProfitRecordRequest request)
        {
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var hold = from item in db.t_account_shares_hold
                           where item.AccountId == request.Id && item.Status > 0
                           select item;
                if (!string.IsNullOrEmpty(request.SharesInfo))
                {
                    hold = from item in hold
                           join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                           where item.SharesCode.Contains(request.SharesInfo) || item2.SharesName.Contains(request.SharesInfo)
                           select item;
                }
                if (request.MaxId > 0)
                {
                    hold = from item in hold
                           where item.Id <= request.MaxId
                           select item;
                }
                int totalCount = hold.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = hold.Max(e => e.Id);
                }
                var list = (from item in hold
                            join item2 in db.t_shares_quotes on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                            from ai in a.DefaultIfEmpty()
                            join item3 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode }
                            orderby item.Status, item.CreateTime descending
                            select new FrontAccountSharesProfitRecordInfo
                            {
                                SharesCode = item.SharesCode,
                                Id = item.Id,
                                Market = item.Market,
                                ProfitAmount = (ai == null ? 0 : (ai.PresentPrice <= 0 ? ai.ClosedPrice : ai.PresentPrice) * item.RemainCount) + item.SoldAmount - item.BuyTotalAmount,
                                SharesName = item3.SharesName,
                                Status = item.Status == 1 && (item.RemainCount > 0 || item.LastModified > dateNow) ? 1 : 2,
                                LastTradeTime = item.LastModified,
                                RemainDeposit = item.RemainDeposit,
                                RemainFundAmount = item.FundAmount,
                                InitialDeposit = item.InitialDeposit,
                                InitialFundAmount = item.InitialFundAmount,
                                ServiceFee = item.ServiceFee
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                foreach (var item in list)
                {
                    var entrustList = (from x in db.t_account_shares_entrust
                                       where x.Status == 3 && x.DealCount > 0 && x.HoldId == item.Id
                                       select x).ToList();
                    if (entrustList.Count() > 0)
                    {
                        item.SettlementFee = entrustList.Sum(e => e.SettlementFee);
                        item.BuyService = entrustList.Sum(e => e.BuyService);
                        item.StampFee = entrustList.Sum(e => e.StampFee);
                        item.AdministrationFee = entrustList.Sum(e => e.AdministrationFee);
                        item.Commission = entrustList.Sum(e => e.Commission);
                        item.ProfitService = entrustList.Sum(e => e.ProfitService);
                        item.HandlingFee = entrustList.Sum(e => e.HandlingFee);
                        item.TransferFee = entrustList.Sum(e => e.TransferFee);
                    }
                }

                return new PageRes<FrontAccountSharesProfitRecordInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = list
                };
            }
        }

        /// <summary>
        /// 查询保证金变更记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSharesDepositChangeRecordInfo> GetFrontAccountSharesDepositChangeRecord(DetailsPageRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var list = from item in db.t_account_shares_hold_deposit_changelog
                           where item.HoldId == request.Id
                           select item;
                int totalCount = list.Count();
                return new PageRes<FrontAccountSharesDepositChangeRecordInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.Id descending
                            select new FrontAccountSharesDepositChangeRecordInfo
                            {
                                ChangeAmount = item.ChangeAmount,
                                CreateTime = item.CreateTime,
                                CurAmount = item.CurAmount,
                                Description = item.Description,
                                Id = item.Id,
                                PreAmount = item.PreAmount
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询借款变更记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSharesFundAmountChangeRecordInfo> GetFrontAccountSharesFundAmountChangeRecord(DetailsPageRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var list = from item in db.t_account_shares_hold_fundamount_changelog
                           where item.HoldId == request.Id
                           select item;
                int totalCount = list.Count();
                return new PageRes<FrontAccountSharesFundAmountChangeRecordInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new FrontAccountSharesFundAmountChangeRecordInfo
                            {
                                ChangeAmount = item.ChangeAmount,
                                CreateTime = item.CreateTime,
                                CurAmount = item.CurAmount,
                                Description = item.Description,
                                Id = item.Id,
                                PreAmount = item.PreAmount
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询前端账户交易异常记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSharesTradeAbnormalRecordInfo> GetFrontAccountSharesTradeAbnormalRecordList(GetFrontAccountSharesTradeAbnormalRecordListRequest request)
        {
            DateTime timeNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var entrust = from item in db.t_account_shares_entrust
                              where item.Status != 3
                              select item;
                if (request.HandlerType == 1)
                {
                    entrust = from item in entrust
                              where item.HandlerType == request.HandlerType && item.CreateTime>= timeNow
                              select item;
                }
                else
                {
                    entrust = from item in entrust
                              where item.CreateTime < timeNow
                              select item;
                }

                if (request.MaxId > 0)
                {
                    entrust = from item in entrust
                              where item.Id <= request.MaxId
                              select item;
                }
                int totalCount = entrust.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = entrust.Max(e => e.Id);
                }
                var list = (from item in entrust
                            join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                            join item3 in db.t_account_baseinfo on item.AccountId equals item3.Id
                            orderby item.CreateTime descending
                            select new FrontAccountSharesTradeAbnormalRecordInfo
                            {
                                SharesCode = item.SharesCode,
                                SharesName = item2.SharesName,
                                AccountMobile = item3.Mobile,
                                AccountName = item3.NickName,
                                CreateTime = item.CreateTime,
                                EntrustCount = item.EntrustCount,
                                EntrustPrice=item.EntrustPrice,
                                EntrustType=item.EntrustType,
                                Id = item.Id,
                                TradeType = item.TradeType
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                foreach (var item in list)
                {
                    var manager = (from x in db.t_account_shares_entrust_manager
                                   where x.BuyId == item.Id
                                   select x).ToList();
                    item.TradeAccountCount = manager.Count();
                    if (item.TradeAccountCount > 0)
                    {
                        item.DealCount = manager.Sum(e => e.DealCount);
                    }
                }

                return new PageRes<FrontAccountSharesTradeAbnormalRecordInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = list
                };
            }
        }

        /// <summary>
        /// 前端账户交易当日队列委托消费
        /// </summary>
        public void ConsumeFrontAccountSharesTradeQueue()
        {
            var buyList = MQHandler.instance.GetMessage("SharesBuy_s1");
            List<long> entrustIdList = new List<long>();
            foreach (var item in buyList)
            {
                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<dynamic>(item.payload);
                long entrustId = dataJson.BuyId;
                entrustIdList.Add(entrustId);
            }

            var sellList = MQHandler.instance.GetMessage("SharesSell_s1");

            List<long> managerIdList = new List<long>();
            foreach (var item in sellList)
            {
                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<dynamic>(item.payload);
                long managerId = dataJson.SellManagerId;
                managerIdList.Add(managerId);
            }
            using (var db = new meal_ticketEntities())
            {
                var manager = (from item in db.t_account_shares_entrust_manager
                               where managerIdList.Contains(item.Id)
                               select item.BuyId).ToList();
                entrustIdList.AddRange(manager);

                var entrust = (from x in db.t_account_shares_entrust
                               where entrustIdList.Contains(x.Id) && x.Status != 3
                               select x).ToList();
                foreach (var item in entrust)
                {
                    item.HandlerType = 1;
                }
                db.SaveChanges();
            }

            var cancelList = MQHandler.instance.GetMessage("SharesCancel_s1");
            foreach (var item in cancelList)
            {
                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<dynamic>(item.payload);
                long tradeManagerId = dataJson.TradeManagerId;
                long entrustId = dataJson.EntrustId;
                string cancelTime = dataJson.CancelTime; 
                if (DateTime.Now.Date != DateTime.Parse(cancelTime))//委托过期
                {
                    //确认队列
                    continue;
                }

                using (var db = new meal_ticketEntities())
                {
                    var tempEntrust = (from x in db.t_account_shares_entrust
                                       where x.Id == entrustId && x.HandlerType == 1
                                       select x).FirstOrDefault();
                    if (tempEntrust == null)
                    {
                        MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(item.payload)), "SharesCancel_s1", "s1");
                        continue;
                    }
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {

                            if (tradeManagerId == -1)
                            {
                                db.P_TryToCancelEntrust(entrustId);
                            }
                            else
                            {
                                //查询委托数据
                                var entrustManager = (from x in db.t_account_shares_entrust_manager
                                                      where x.Id == tradeManagerId
                                                      select x).FirstOrDefault();
                                if (entrustManager == null)
                                {
                                    throw new Exception();
                                }
                                var entrust = (from x in db.t_account_shares_entrust
                                               where x.Id == entrustManager.BuyId
                                               select x).FirstOrDefault();
                                if (entrust == null)
                                {
                                    throw new Exception();
                                }
                                if (string.IsNullOrEmpty(entrustManager.EntrustId))
                                {
                                    db.P_TradeFinish(tradeManagerId, 0, 0);
                                }
                            }
                            tran.Commit();
                        }

                        catch (Exception ex)
                        {
                            tran.Rollback();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查询前端账户交易异常记录详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSharesTradeAbnormalRecordDetails> GetFrontAccountSharesTradeAbnormalRecordDetails(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var entrustManager = from item in db.t_account_shares_entrust_manager
                                     where item.BuyId == request.Id
                                     select item;
                if (request.MaxId > 0)
                {
                    entrustManager = from item in entrustManager
                                     where item.Id <= request.MaxId
                                     select item;
                }
                int totalCount = entrustManager.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = entrustManager.Max(e => e.Id);
                }

                return new PageRes<FrontAccountSharesTradeAbnormalRecordDetails>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in entrustManager
                            orderby item.CreateTime descending
                            select new FrontAccountSharesTradeAbnormalRecordDetails
                            {
                                Status = item.Status,
                                StatusDes = item.StatusDes,
                                DealCount = item.DealCount,
                                EntrustCount = item.EntrustCount,
                                EntrustId = item.EntrustId,
                                EntrustTime = item.EntrustTime,
                                Id = item.Id,
                                SimulateDealCount = item.SimulateDealCount,
                                DealPrice = item.DealPrice,
                                EntrustPrice = item.EntrustPrice,
                                EntrustType = item.EntrustType,
                                RealEntrustCount = item.RealEntrustCount,
                                TradeAccountCode = item.TradeAccountCode
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };

            }
        }

        /// <summary>
        /// 添加前端账户交易异常账号委托
        /// </summary>
        /// <param name="request"></param>
        public void AddFrontAccountSharesTradeAbnormalRecordDetails(AddFrontAccountSharesTradeAbnormalRecordDetailsRequest request)
        {
            if (request.DealCount > request.EntrustCount)
            {
                throw new WebApiException(400, "成交数量不能大于委托数量");
            }
            DateTime timeNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var entrust = (from item in db.t_account_shares_entrust
                                   where item.Id == request.BuyId
                                   select item).FirstOrDefault();
                    if (entrust == null)
                    {
                        throw new WebApiException(400, "委托不存在");
                    }
                    if (entrust.Status == 3)
                    {
                        throw new WebApiException(400, "当前委托无法添加");
                    }
                    if (entrust.TradeType != 1)
                    {
                        throw new WebApiException(400, "非买入委托无法添加");
                    }
                    //判断数量是否正确
                    int currCount = 0;
                    var manager = (from item in db.t_account_shares_entrust_manager
                                   where item.BuyId == request.BuyId
                                   select item).ToList();
                    if (manager.Count > 0)
                    {
                        currCount = manager.Sum(e => e.EntrustCount);
                    }
                    if (currCount + request.EntrustCount > entrust.EntrustCount)
                    {
                        throw new WebApiException(400, "超出总委托数量");
                    }
                    db.t_account_shares_entrust_manager.Add(new t_account_shares_entrust_manager
                    {
                        SimulateDealCount = 0,
                        Status = 2,
                        StatusDes = "",
                        BuyId = request.BuyId,
                        CancelCount = 0,
                        CanClear = false,
                        CreateTime = DateTime.Now,
                        DealAmount = 0,
                        DealCount = request.DealCount,
                        DealPrice = request.DealPrice,
                        EntrustCount = request.EntrustCount,
                        EntrustId = request.EntrustId,
                        EntrustPrice = request.EntrustType == 1 ? 0 : request.EntrustPrice,
                        EntrustTime = DateTime.Now,
                        EntrustType = request.EntrustType,
                        LastModified = DateTime.Now,
                        RealEntrustCount = request.EntrustCount,
                        TradeAccountCode = request.TradeAccountCode,
                        TradeType = entrust.TradeType
                    });
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
        /// 编辑前端账户交易异常账号委托
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountSharesTradeAbnormalRecordDetails(ModifyFrontAccountSharesTradeAbnormalRecordDetailsRequest request)
        {
            if (request.DealCount > request.RealEntrustCount)
            {
                throw new WebApiException(400, "成交数量不能大于实际委托数量");
            }
            if (string.IsNullOrEmpty(request.EntrustId))
            {
                throw new WebApiException(400, "请输入委托编号");
            }
            DateTime timeNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var manager = (from item in db.t_account_shares_entrust_manager
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                    if (manager == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    if (manager.Status == 3)
                    {
                        throw new WebApiException(400, "当前委托已完成,无法编辑");
                    }
                    if (manager.EntrustCount < request.RealEntrustCount)
                    {
                        throw new WebApiException(400, "实际委托数量不能大于总委托数量");
                    }
                    var entrust = (from item in db.t_account_shares_entrust
                                   where item.Id == manager.BuyId
                                   select item).FirstOrDefault();
                    if (entrust == null)
                    {
                        throw new WebApiException(400, "委托不存在");
                    }
                    if (entrust.Status == 3)
                    {
                        throw new WebApiException(400, "当前委托无法编辑");
                    }

                    if (string.IsNullOrEmpty(manager.EntrustId) && entrust.TradeType == 2)
                    {
                        //查询账户持仓情况
                        var accountShares = (from item in db.t_broker_account_shares_rel
                                             where item.TradeAccountCode == manager.TradeAccountCode && item.Market == entrust.Market && item.SharesCode == entrust.SharesCode
                                             select item).FirstOrDefault();
                        if (accountShares != null)
                        {
                            accountShares.AccountCanSoldCount = accountShares.AccountCanSoldCount - manager.EntrustCount;
                            accountShares.AccountSellingCount = accountShares.AccountSellingCount + manager.EntrustCount;
                            db.SaveChanges();
                        }
                    }

                    manager.EntrustId = request.EntrustId;
                    manager.RealEntrustCount = request.RealEntrustCount;
                    manager.DealCount = request.DealCount;
                    manager.DealPrice = request.DealPrice;

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
        /// 删除前端账户交易异常账号委托
        /// </summary>
        /// <param name="request"></param>
        public void DeleteFrontAccountSharesTradeAbnormalRecordDetails(DeleteRequest request)
        {
            DateTime timeNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var manager = (from item in db.t_account_shares_entrust_manager
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                    if (manager == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    if (manager.Status == 3)
                    {
                        throw new WebApiException(400, "当前委托已完成,无法删除");
                    }
                    var entrust = (from item in db.t_account_shares_entrust
                                   where item.Id == manager.BuyId
                                   select item).FirstOrDefault();
                    if (entrust == null)
                    {
                        throw new WebApiException(400, "委托不存在");
                    }
                    if (entrust.Status == 3)
                    {
                        throw new WebApiException(400, "当前委托无法删除");
                    }

                    var dealdetails = (from item in db.t_account_shares_entrust_manager_dealdetails
                                       where item.EntrustManagerId == manager.Id
                                       select item).ToList();
                    if (dealdetails.Count() > 0)
                    {
                        db.t_account_shares_entrust_manager_dealdetails.RemoveRange(dealdetails);
                        db.SaveChanges();
                    }

                    db.t_account_shares_entrust_manager.Remove(manager);
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
        /// 结束异常交易
        /// </summary>
        public void OverFrontAccountSharesTradeAbnormalRecord(DetailsRequest request)
        {
            DateTime timeNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var entrust = (from item in db.t_account_shares_entrust
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (entrust == null)
                {
                    throw new WebApiException(400, "委托不存在");
                }
                if (entrust.Status == 3)
                {
                    throw new WebApiException(400, "当前委托无法结束交易");
                }
                var manager = (from item in db.t_account_shares_entrust_manager
                               where item.BuyId == entrust.Id
                               select item).ToList();
                if (manager.Count() <= 0)
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            db.P_TryToCancelEntrust(entrust.Id);

                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            Logger.WriteFileLog("结束交易清理出错", ex);
                        }
                    }
                }
                else
                {
                    manager = (from item in manager
                               where item.Status != 3
                               select item).ToList();
                    foreach (var item in manager)
                    {
                        using (var tran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var dealdetails = (from x in db.t_account_shares_entrust_manager_dealdetails
                                                   where x.EntrustManagerId == item.Id
                                                   select x).ToList();
                                if (dealdetails.Count() > 0)
                                {
                                    db.t_account_shares_entrust_manager_dealdetails.RemoveRange(dealdetails);
                                    db.SaveChanges();
                                }

                                db.t_account_shares_entrust_manager_dealdetails.Add(new t_account_shares_entrust_manager_dealdetails
                                {
                                    DealCount = item.DealCount,
                                    DealAmount = item.DealCount * item.DealPrice,
                                    DealPrice = item.DealPrice,
                                    DealTime = DateTime.Now,
                                    EntrustManagerId = item.Id,
                                    DealId = Guid.NewGuid().ToString("N")
                                });
                                db.SaveChanges();

                                db.P_TradeFinish(item.Id, item.DealPrice, item.DealCount);

                                tran.Commit();
                            }
                            catch (Exception ex)
                            {
                                tran.Rollback();
                                Logger.WriteFileLog("结束交易出错-" + item.Id, ex);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查询股票派股派息记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSharesAllotRecordInfo> GetFrontAccountSharesAllotRecord(PageRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var list = from item in db.t_shares_allot
                           select item;
                int totalCount = list.Count();

                var result = (from item in list
                              join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                              from ai in a.DefaultIfEmpty()
                              orderby item.CreateTime descending
                              select new FrontAccountSharesAllotRecordInfo
                              {
                                  SharesCode = item.SharesCode,
                                  SharesName = ai == null ? "" : ai.SharesName,
                                  AllotSharesInfo = item.AllotSharesInfo,
                                  AllotBonusInfo = item.AllotBonusInfo,
                                  Id = item.Id,
                                  Market = item.Market,
                                  CreateTime = item.CreateTime,
                                  HandleStatus=item.HandleStatus,
                                  AllotDate=item.AllotDate,
                                  HandleTime=item.HandleTime
                              }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                return new PageRes<FrontAccountSharesAllotRecordInfo>
                {
                    MaxId=0,
                    TotalCount=totalCount,
                    List=result
                };
            }
        }

        /// <summary>
        /// 添加股票派股派息
        /// </summary>
        /// <param name="request"></param>
        public void AddFrontAccountSharesAllot(AddFrontAccountSharesAllotRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                //判断股票是否存在
                var shares = (from item in db.t_shares_all
                              where item.Market == request.Market && item.SharesCode == request.SharesCode
                              select item).FirstOrDefault();
                if (shares == null)
                {
                    throw new WebApiException(400, "股票不存在");
                }
                //判断派息日期
                if (request.AllotDate.Date <= DateTime.Now.Date)
                {
                    throw new WebApiException(400, "派息日期最小为明日");
                }
                //判断派息信息是否存在
                var allot = (from item in db.t_shares_allot
                             where item.Market == request.Market && item.SharesCode == request.SharesCode && item.AllotDate == request.AllotDate
                             select item).FirstOrDefault();
                if (allot != null)
                {
                    throw new WebApiException(400,"该股票当天派息数据已存在");
                }
                //判断派息派股信息
                if (string.IsNullOrEmpty(request.AllotBonusInfo) && string.IsNullOrEmpty(request.AllotSharesInfo))
                {
                    throw new WebApiException(400,"必须配置派息或派股其中一项");
                }
                if (!string.IsNullOrEmpty(request.AllotSharesInfo))
                {
                    try
                    {
                        var temp = JsonConvert.DeserializeObject<dynamic>(request.AllotSharesInfo);
                        int baseCount = temp.BaseCount;
                        int allotCount = temp.AllotCount;
                        if (baseCount <= 0 || allotCount <= 0)
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception)
                    {
                        throw new WebApiException(400,"派股数据参数有误");
                    }
                }
                if (!string.IsNullOrEmpty(request.AllotBonusInfo))
                {
                    try
                    {
                        var temp = JsonConvert.DeserializeObject<dynamic>(request.AllotBonusInfo);
                        int baseCount = temp.BaseCount;
                        int allotCount = temp.AllotCount;
                        if (baseCount <= 0 || allotCount <= 0)
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception)
                    {
                        throw new WebApiException(400, "派息数据参数有误");
                    }
                }

                db.t_shares_allot.Add(new t_shares_allot 
                { 
                    SharesCode=request.SharesCode,
                    HandleStatus=1,
                    AllotDate=request.AllotDate,
                    CreateTime=DateTime.Now,
                    HandleTime=null,
                    LastModified=DateTime.Now,
                    Market=request.Market,
                    AllotSharesInfo=request.AllotSharesInfo,
                    AllotBonusInfo=request.AllotBonusInfo
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量导入股票派股派息
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddFrontAccountSharesAllot(List<AddFrontAccountSharesAllotRequest> list) 
        {
            int result = 0;
            using (var db = new meal_ticketEntities())
            {
                foreach (var request in list)
                {
                    //判断股票是否存在
                    var shares = (from item in db.t_shares_all
                                  where item.Market == request.Market && item.SharesCode == request.SharesCode
                                  select item).FirstOrDefault();
                    if (shares == null)
                    {
                        continue;
                    }
                    //判断派息日期
                    if (request.AllotDate.Date <= DateTime.Now.Date)
                    {
                        continue;
                    }
                    //判断派息信息是否存在
                    var allot = (from item in db.t_shares_allot
                                 where item.Market == request.Market && item.SharesCode == request.SharesCode && item.AllotDate == request.AllotDate
                                 select item).FirstOrDefault();
                    if (allot != null)
                    {
                        continue;
                    }
                    //判断派息派股信息
                    if (string.IsNullOrEmpty(request.AllotBonusInfo) && string.IsNullOrEmpty(request.AllotSharesInfo))
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(request.AllotSharesInfo))
                    {
                        try
                        {
                            var temp = JsonConvert.DeserializeObject<dynamic>(request.AllotSharesInfo);
                            int baseCount = temp.BaseCount;
                            int allotCount = temp.AllotCount;
                            if (baseCount <= 0 || allotCount <= 0)
                            {
                                continue;
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    if (!string.IsNullOrEmpty(request.AllotBonusInfo))
                    {
                        try
                        {
                            var temp = JsonConvert.DeserializeObject<dynamic>(request.AllotBonusInfo);
                            int baseCount = temp.BaseCount;
                            int allotCount = temp.AllotCount;
                            if (baseCount <= 0 || allotCount <= 0)
                            {
                                continue;
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    db.t_shares_allot.Add(new t_shares_allot
                    {
                        SharesCode = request.SharesCode,
                        HandleStatus = 1,
                        AllotDate = request.AllotDate,
                        CreateTime = DateTime.Now,
                        HandleTime = null,
                        LastModified = DateTime.Now,
                        Market = request.Market,
                        AllotSharesInfo = request.AllotSharesInfo,
                        AllotBonusInfo = request.AllotBonusInfo
                    });
                    result++;
                }
                db.SaveChanges();
                return result;
            }
        }

        /// <summary>
        /// 编辑股票派股派息
        /// </summary>
        /// <param name="request"></param>
        public void ModifyFrontAccountSharesAllot(ModifyFrontAccountSharesAllotRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_allot
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (result == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                if (result.HandleStatus != 1)
                {
                    throw new WebApiException(400,"已处理数据不能编辑");
                }

                //判断股票是否存在
                var shares = (from item in db.t_shares_all
                              where item.Market == request.Market && item.SharesCode == request.SharesCode
                              select item).FirstOrDefault();
                if (shares == null)
                {
                    throw new WebApiException(400, "股票不存在");
                }
                //判断派息日期
                if (request.AllotDate.Date <= DateTime.Now.Date)
                {
                    throw new WebApiException(400, "派息日期最小为明日");
                }
                //判断派息信息是否存在
                var allot = (from item in db.t_shares_allot
                             where item.Market == request.Market && item.SharesCode == request.SharesCode && item.AllotDate == request.AllotDate && item.Id!=request.Id
                             select item).FirstOrDefault();
                if (allot != null)
                {
                    throw new WebApiException(400, "该股票当天派息数据已存在");
                }
                //判断派息派股信息
                if (string.IsNullOrEmpty(request.AllotBonusInfo) && string.IsNullOrEmpty(request.AllotSharesInfo))
                {
                    throw new WebApiException(400, "必须配置派息或派股其中一项");
                }
                if (!string.IsNullOrEmpty(request.AllotSharesInfo))
                {
                    try
                    {
                        var temp = JsonConvert.DeserializeObject<dynamic>(request.AllotSharesInfo);
                        int baseCount = temp.BaseCount;
                        int allotCount = temp.AllotCount;
                        if (baseCount <= 0 || allotCount <= 0)
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception)
                    {
                        throw new WebApiException(400, "派股数据参数有误");
                    }
                }
                if (!string.IsNullOrEmpty(request.AllotBonusInfo))
                {
                    try
                    {
                        var temp = JsonConvert.DeserializeObject<dynamic>(request.AllotBonusInfo);
                        int baseCount = temp.BaseCount;
                        int allotCount = temp.AllotCount;
                        if (baseCount <= 0 || allotCount <= 0)
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception)
                    {
                        throw new WebApiException(400, "派息数据参数有误");
                    }
                }
                result.SharesCode = request.SharesCode;
                result.AllotDate = request.AllotDate;
                result.LastModified = DateTime.Now;
                result.Market = request.Market;
                result.AllotSharesInfo = request.AllotSharesInfo;
                result.AllotBonusInfo = request.AllotBonusInfo;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除股票派股派息
        /// </summary>
        /// <param name="request"></param>
        public void DeleteFrontAccountSharesAllot(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_allot
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (result == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                if (result.HandleStatus != 1)
                {
                    throw new WebApiException(400, "已处理数据不能删除");
                }
                db.t_shares_allot.Remove(result);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询股票派股派息详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<FrontAccountSharesAllotDetails> GetFrontAccountSharesAllotDetails(DetailsPageRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var details = from item in db.t_shares_allot_account
                              where item.AllotId == request.Id
                              select item;
                int totalCount = details.Count();

                return new PageRes<FrontAccountSharesAllotDetails>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in details
                            join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id into a from ai in a.DefaultIfEmpty()
                            orderby item.Id
                            select new FrontAccountSharesAllotDetails
                            {
                                AccountId = item.AccountId,
                                BonusAmount = item.BonusAmount,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                InitialCount = item.InitialCount,
                                InitialCostPrice = item.InitialCostPrice,
                                LatestCostPrice = item.LatestCostPrice,
                                LatestCount = item.LatestCount,
                                AccountMobile = ai == null ? "" : ai.Mobile,
                                AccountName = ai == null ? "" : ai.NickName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }
        #endregion
    }
}
