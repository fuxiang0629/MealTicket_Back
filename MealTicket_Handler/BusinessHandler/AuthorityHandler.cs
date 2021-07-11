using MealTicket_Handler.Model;
using MealTicket_Handler;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MealTicket_DBCommon;

namespace MealTicket_Handler
{
    public class AuthorityHandler
    {
        /// <summary>
        /// 添加访问日志
        /// </summary>
        /// <param name="log"></param>
        public void AddVisitLog(VisitLog log)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_authority_account_visitlog.Add(new t_authority_account_visitlog
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
                    AppVersion = log.AppVersion,
                    UserToken = log.UserToken
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 检查用户是否登录
        /// </summary>
        /// <param name="userToken"></param>
        /// <param name="baseData"></param>
        public void CheckAccountLogin(string userToken, HeadBase baseData)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter accountIdDb = new ObjectParameter("accountId", 0);
                    db.P_CheckAccountLogin(userToken, errorCodeDb, errorMessageDb, accountIdDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    long accountId = (long)accountIdDb.Value;
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                    baseData.AccountId = accountId;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 判断用户是否实名认证
        /// </summary>
        /// <param name="accountId"></param>
        public void CheckRealName(long accountId)
        {
            using (var db = new meal_ticketEntities())
            {
                var realname = (from item in db.t_account_realname
                                where item.AccountId == accountId
                                select item).FirstOrDefault();
                if (realname == null)
                {
                    throw new WebApiException(305,"请先实名认证");
                }
                if (realname.ExamineStatus != 2)
                {
                    throw new WebApiException(305, "请先实名认证");
                }
            }
        }

        /// <summary>
        /// 更新Api
        /// </summary>
        /// <param name="list"></param>
        public void InitActionList(List<ActionList> list)
        {
            using (var db = new meal_ticketEntities())
            {
                var actionNow = (from item in db.t_account_api
                                 select item.Url).ToList();


                var actionList = (from item in list
                                  where !actionNow.Contains(item.Url)
                                  select new t_account_api
                                  {
                                      CreateTime = DateTime.Now,
                                      Description = item.Description,
                                      Name = item.Description,
                                      Url = item.Url
                                  }).ToList();
                foreach (var item in actionList)
                {
                    if (item.Url == "NoticeSend.OverCordon")//到达警戒线通知
                    {
                        item.Para = JsonConvert.SerializeObject(new 
                        {
                            sharescode="股票代码",
                            sharesname="股票名称",
                            time="时间",
                            adddeposit = "建议追加保证金",
                            accountmobile ="用户手机号"
                        });
                    }
                    if (item.Url == "NoticeSend.ClosingForceSoon")//即将强制平仓通知
                    {
                        item.Para = JsonConvert.SerializeObject(new
                        {
                            sharescode = "股票代码",
                            sharesname = "股票名称",
                            time = "时间",
                            times="强平时间",
                            closingprice="强平价格",
                            adddeposit="建议追加保证金",
                            accountmobile = "用户手机号"
                        });
                    }
                    if (item.Url == "NoticeSend.ClosingForce")//发生强制平仓通知
                    {
                        item.Para = JsonConvert.SerializeObject(new
                        {
                            sharescode = "股票代码",
                            sharesname = "股票名称",
                            time = "时间",
                            dealcount="成交数量",
                            dealstatus="委托状态",
                            accountmobile = "用户手机号"
                        });
                    }
                    if (item.Url == "NoticeSend.ClosingAuto")//发生自动平仓通知
                    {
                        item.Para = JsonConvert.SerializeObject(new
                        {
                            sharescode = "股票代码",
                            sharesname = "股票名称",
                            time = "时间",
                            dealcount = "成交数量",
                            dealstatus = "委托状态",
                            accountmobile = "用户手机号"
                        });
                    }
                    if (item.Url == "NoticeSend.BuySharesFinish")//股票买入委托结束通知
                    {
                        item.Para = JsonConvert.SerializeObject(new
                        {
                            sharescode = "股票代码",
                            sharesname = "股票名称",
                            time = "时间",
                            dealcount = "成交数量",
                            dealstatus = "委托状态",
                            accountmobile = "用户手机号"
                        });
                    }
                    if (item.Url == "NoticeSend.SellSharesFinish")//股票卖出委托结束通知
                    {
                        item.Para = JsonConvert.SerializeObject(new
                        {
                            sharescode = "股票代码",
                            sharesname = "股票名称",
                            time = "时间",
                            dealcount = "成交数量",
                            dealstatus = "委托状态",
                            accountmobile = "用户手机号"
                        });
                    }
                    if (item.Url == "NoticeSend.CashSuccess")//提现成功通知
                    {
                        item.Para = JsonConvert.SerializeObject(new
                        {
                            time = "时间",
                            amount="已提现金额",
                            accountmobile = "用户手机号"
                        });
                    }
                    if (item.Url == "NoticeSend.CashFail")//提现失败通知
                    {
                        item.Para = JsonConvert.SerializeObject(new
                        {
                            time = "时间",
                            amount = "申请提现金额",
                            accountmobile = "用户手机号"
                        });
                    }
                    if (item.Url == "NoticeSend.CashReject")//拒绝提现通知
                    {
                        item.Para = JsonConvert.SerializeObject(new
                        {
                            time = "时间",
                            amount = "申请提现金额",
                            accountmobile = "用户手机号"
                        });
                    }
                    if (item.Url == "Account.CashApply")//申请提现
                    {
                        item.Para = JsonConvert.SerializeObject(new
                        {
                            time = "时间",
                            amount = "申请提现金额",
                            accountmobile = "用户手机号"
                        });
                    }
                    if (item.Url == "Account.AccountModifyTransactionPassword")//修改交易密码
                    {
                        item.Para = JsonConvert.SerializeObject(new
                        {
                            time = "时间",
                            accountmobile = "用户手机号"
                        });
                    }
                }

                db.t_account_api.AddRange(actionList);
                db.SaveChanges();
            }
        }
    }
}
