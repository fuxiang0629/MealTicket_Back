using FXCommon.Common;
using FXCommon.FileUpload;
using MealTicket_DBCommon;
using MealTicket_Handler.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MealTicket_Handler
{
    public class AccountHandler
    {
        /// <summary>
        /// 用户登入
        /// </summary>
        /// <returns></returns>
        public AccountLoginInfo AccountLogin(AccountLoginRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    string errorMessage = "";
                    long accountId = 0;
                    try
                    {
                        ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                        ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                        ObjectParameter tokenDb = new ObjectParameter("token", "");
                        ObjectParameter accountIdDb = new ObjectParameter("accountId", 0);
                        db.P_AccountLogin(request.Mobile, request.LoginPassword.ToMD5(), errorCodeDb, errorMessageDb, tokenDb, accountIdDb);
                        int errorCode = (int)errorCodeDb.Value;
                        errorMessage = errorMessageDb.Value.ToString();
                        accountId = (long)accountIdDb.Value;
                        string token = tokenDb.Value.ToString();
                        if (errorCode != 0 && errorCode != 304)
                        {
                            throw new WebApiException(errorCode, errorMessage);
                        }
                        tran.Commit();

                        return new AccountLoginInfo
                        {
                            Token = token,
                            Status = errorCode == 304 ? 3 : 1
                        };
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                    finally 
                    {
                        try
                        {
                            db.t_account_loginlog.Add(new t_account_loginlog 
                            {
                                AccountId= accountId,
                                AccountName=request.Mobile,
                                CreateTime=DateTime.Now,
                                DeviceUA= basedata.DeviceNo,
                                LoginError= errorMessage,
                                LoginIp=basedata.Ip,
                                LoginoutError="",
                                LoginTime=DateTime.Now,
                                LoginType=1,
                                LogoutTime=null
                            });
                            db.SaveChanges();
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        /// <summary>
        /// 用户首次设置交易密码
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public AccountLoginInfo AccountSetTransactionPassword(AccountSetTransactionPasswordRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter tokenDb = new ObjectParameter("token", "");
                    db.P_AccountSetTransactionPassword(request.TransactionPassword.ToMD5(), basedata.UserToken, errorCodeDb, errorMessageDb, tokenDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    string token = tokenDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();

                    return new AccountLoginInfo
                    {
                        Token = token,
                        Status = 1
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
        /// 检查手机号是否已经注册
        /// </summary>
        public bool CheckAccountRegister(CheckAccountRegisterRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var accountInfo = (from item in db.t_account_baseinfo
                                   where item.Mobile == request.Mobile
                                   select item).FirstOrDefault();
                return accountInfo != null;
            }
        }

        /// <summary>
        /// 生成图片验证码
        /// </summary>
        public void CreatePicVerifyCode(string mobile, int businessType, string code, string filePath)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_CreatePicVerifyCode(mobile, businessType, code, filePath, errorCodeDb, errorMessageDb);
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
        /// 检查图片验证码是否正确
        /// </summary>
        public void CheckPicVerifyCode(CheckPicVerifyCodeRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var verifycode = (from item in db.t_account_verifyCode
                                  where item.BusinessType == request.BusinessType && item.Mobile == request.Mobile
                                  select item).FirstOrDefault();
                if (verifycode == null)
                {
                    throw new WebApiException(400, "图片验证码不存在");
                }
                if (verifycode.PicVerifyCode.ToUpper() != request.PicVerifycode.ToUpper())
                {
                    throw new WebApiException(400, "图片验证码错误");
                }
                return;
            }
        }

        /// <summary>
        /// 生成短信验证码
        /// </summary>
        /// <returns></returns>
        public int CreateSmsVerifyCode(SendVerifyCodeRequest request, string verifyCode)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter operationCodeDb = new ObjectParameter("operationCode", 0);
                    db.P_CreateSmsVerifyCode(request.Mobile, request.BusinessType, verifyCode, request.PicVerifycode, operationCodeDb, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                    return (int)operationCodeDb.Value;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <returns></returns>
        public void AccountRegister(AccountRegisterRequest request, HeadBase basedata)
        {
            int ActionCode;
            if (!int.TryParse(request.ActionCode.Replace("S1_", ""), out ActionCode))
            {
                throw new WebApiException(400, "无效验证码");
            }
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter accountIdDb = new ObjectParameter("accountId", 0);
                    db.P_AccountRegister(request.Mobile, request.LoginPassword.ToMD5(), ActionCode, request.Verifycode, request.ReferRecommandCode,basedata.Ip,basedata.DeviceNo, errorCodeDb, errorMessageDb, accountIdDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    //生成二维码及专属海报
                    long accountId = (long)accountIdDb.Value;
                    var accountInfo = (from item in db.t_account_baseinfo
                                       where item.Id == accountId
                                       select item).FirstOrDefault();
                    if (accountInfo == null)
                    {
                        throw new WebApiException(400, "注册失败,服务器内部错误");
                    }
                    //生成二维码图片
                    string qrCodePar = accountInfo.RecommandCode;

                    var par = (from item in db.t_system_param
                               where item.ParamName == "RegisterPageUrl"
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        try
                        {
                            accountInfo.RegisterPageUrl = par.ParamValue;
                            qrCodePar = string.Format("{0}?referRecommandCode={1}", par.ParamValue, qrCodePar);
                            byte[] qrCodeBytes = FXCommon.Common.Utils.CreateQrCode(qrCodePar);
                            QiNiuHelper qiNiu = new QiNiuHelper();
                            string path = qiNiu.Upload("png", qrCodeBytes, "Other.Img", out errorMessage);
                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                throw new Exception(errorMessage);
                            }
                            accountInfo.QrCodeUrl = path;
                            //获取海报地址
                            var poster = (from item in db.t_page_setting
                                          where item.Status == 1 && item.PageCode == "ShareFriends"
                                          select item).FirstOrDefault();
                            if (poster != null)
                            {
                                try
                                {
                                    var content = JsonConvert.DeserializeObject<dynamic>(poster.Content);

                                    accountInfo.PostersUrl = content.UrlPosters;
                                    int QrcodeX = content.qrcodeX;
                                    int QrcodeY = content.qrcodeY;
                                    int QrcodeWidth = content.qrcodeWidth;
                                    int QrcodeHeight = content.qrcodeHeight;

                                    string PostersComposeUrl = FXCommon.Common.Utils.CombinImage(string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], accountInfo.PostersUrl), string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], path), QrcodeX, QrcodeY, QrcodeWidth, QrcodeHeight);

                                    accountInfo.PostersComposeUrl = PostersComposeUrl;
                                }
                                catch (Exception ex)
                                { }
                            }
                        }
                        catch (Exception ex)
                        { }
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
        /// 忘记密码重置
        /// </summary>
        public void AccountForgetPassword(AccountForgetPasswordRequest request)
        {
            int ActionCode;
            if (!int.TryParse(request.ActionCode.Replace("S2_", ""), out ActionCode))
            {
                throw new WebApiException(400, "无效验证码");
            }
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_AccountForgetPassword(request.Mobile, ActionCode, request.Verifycode, request.NewLoginPassword.ToMD5(), errorCodeDb, errorMessageDb);
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
        /// 修改手机号
        /// </summary>
        public AccountLoginInfo AccountModifyMobile(AccountModifyMobileRequest request, HeadBase basedata)
        {
            int ActionCode;
            if (!int.TryParse(request.ActionCode.Replace("S3_", ""), out ActionCode))
            {
                throw new WebApiException(400, "无效验证码");
            }
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter tokenDb = new ObjectParameter("token", "");
                    db.P_AccountModifyMobile(basedata.AccountId, request.NewMobile, ActionCode, request.Verifycode, errorCodeDb, errorMessageDb, tokenDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();

                    return new AccountLoginInfo
                    {
                        Token = tokenDb.Value.ToString()
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
        /// 修改密码
        /// </summary>
        public AccountLoginInfo AccountModifyPassword(AccountModifyPasswordRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter tokenDb = new ObjectParameter("token", "");
                    db.P_AccountModifyPassword(basedata.AccountId, request.OldPassword.ToMD5(), request.NewPassword.ToMD5(), errorCodeDb, errorMessageDb, tokenDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();

                    return new AccountLoginInfo
                    {
                        Token = tokenDb.Value.ToString()
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
        /// 修改交易密码
        /// </summary>
        public void AccountModifyTransactionPassword(AccountModifyTransactionPasswordRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_AccountModifyTransactionPassword(basedata.AccountId, request.OldTransactionPassword.ToMD5(), request.NewTransactionPassword.ToMD5(), errorCodeDb, errorMessageDb);
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
        /// 修改个人信息
        /// </summary>
        public void AccountModifyBaseInfo(AccountModifyBaseInfoRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var accountInfo = (from item in db.t_account_baseinfo
                                   where item.Id == basedata.AccountId
                                   select item).FirstOrDefault();
                if (accountInfo == null)
                {
                    throw new WebApiException(400, "用户不存在");
                }
                //判断是否实名认证
                bool isRealName = false;
                var realname = (from item in db.t_account_realname
                                where item.AccountId == basedata.AccountId && item.ExamineStatus == 2
                                select item).FirstOrDefault();
                if (realname != null)
                {
                    isRealName = true;
                }

                if (request.Type == 1)//头像修改
                {
                    accountInfo.HeadUrl = request.Value;
                    db.SaveChanges();
                }
                else if (request.Type == 2)//昵称修改
                {
                    accountInfo.NickName = request.Value;
                    db.SaveChanges();
                }
                else if (request.Type == 3)//性别修改
                {
                    if (isRealName)
                    {
                        throw new WebApiException(400, "已实名认证，性别无法修改");
                    }
                    accountInfo.Sex = int.Parse(request.Value);
                    db.SaveChanges();
                }
                else if (request.Type == 4)//出生年月修改
                {
                    if (isRealName)
                    {
                        throw new WebApiException(400, "已实名认证，出生年月无法修改");
                    }
                    accountInfo.BirthDay = request.Value;
                    db.SaveChanges();
                }
                else
                {
                    throw new WebApiException(400, "参数错误");
                }
            }
        }

        /// <summary>
        /// 获取用户基本信息
        /// </summary>
        /// <returns></returns>
        public AccountBaseInfo AccountGetBaseInfo(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var accountInfo = (from item in db.t_account_baseinfo
                                   where item.Id == basedata.AccountId
                                   select new AccountBaseInfo
                                   {
                                       HeadUrl = item.HeadUrl,
                                       Sex = item.Sex,
                                       Mobile = item.Mobile,
                                       BirthDay = item.BirthDay,
                                       NickName = item.NickName,
                                       RecommandCode = item.RecommandCode,
                                   }).FirstOrDefault();
                if (accountInfo == null)
                {
                    return null;
                }
                var realname = (from item in db.t_account_realname
                                where item.AccountId == basedata.AccountId && item.ExamineStatus == 2
                                select item).FirstOrDefault();
                if (realname == null)
                {
                    accountInfo.IsRealName = false ;
                }
                else
                {
                    accountInfo.IsRealName = true;
                }
                return accountInfo;
            }
        }

        /// <summary>
        /// 搜索用户信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<AccountBaseInfo> QueryAccountBaseInfo(QueryAccountBaseInfoRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var accountInfo = (from item in db.t_account_baseinfo
                                  select new AccountBaseInfo
                                  {
                                      HeadUrl = item.HeadUrl,
                                      Sex = item.Sex,
                                      Mobile = item.Mobile,
                                      BirthDay = item.BirthDay,
                                      NickName = item.NickName,
                                      RecommandCode = item.RecommandCode,
                                  }).ToList();
                if (!string.IsNullOrEmpty(request.AccountInfo))
                {
                    accountInfo = (from item in accountInfo
                                  where item.Mobile.Contains(request.AccountInfo) || item.RecommandCode.Contains(request.AccountInfo)
                                  select item).ToList();
                }
                return accountInfo;
            }
        }

        /// <summary>
        /// 获取推荐人基本信息
        /// </summary>
        /// <returns></returns>
        public AccountBaseInfo AccountGetReferBaseInfo(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var accountInfo = (from item in db.t_account_baseinfo
                                   where item.Id == basedata.AccountId
                                   select item).FirstOrDefault();
                if (accountInfo == null)
                {
                    throw new WebApiException(400, "用户不存在");
                }
                if (accountInfo.ReferId > 0)
                {
                    var referInfo = (from item in db.t_account_baseinfo
                                     where item.Id == accountInfo.ReferId
                                     select new AccountBaseInfo
                                     {
                                         HeadUrl = item.HeadUrl,
                                         Sex = item.Sex,
                                         Mobile = item.Mobile,
                                         BirthDay = item.BirthDay,
                                         NickName = item.NickName,
                                         RecommandCode = item.RecommandCode
                                     }).FirstOrDefault();
                    return referInfo;
                }
                return null;
            }
        }

        /// <summary>
        /// 设置推荐人
        /// </summary>
        public void AccountSetRefer(AccountSetReferRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_AccountSetRefer(basedata.AccountId, request.RecommandCode, errorCodeDb, errorMessageDb);
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
        /// 查询被邀请人列表
        /// </summary>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<AccountBaseInfo> AccountGetInviteesList(PageRequest request,HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var list = from item in db.t_account_baseinfo
                           where item.ReferId == basedata.AccountId
                           select item;
                if (request.MaxId > 0)
                {
                    list = from item in list
                           where item.Id <= request.MaxId
                                   select item;
                }
                int totalCount = list.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = list.Max(e => e.Id);
                }

                return new PageRes<AccountBaseInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new AccountBaseInfo
                            {
                                Sex = item.Sex,
                                HeadUrl = item.HeadUrl,
                                CreateTime=item.CreateTime,
                                BirthDay = item.BirthDay,
                                Mobile = item.Mobile,
                                NickName = item.NickName,
                                RecommandCode = item.RecommandCode
                            }).Skip((request.PageIndex - 1) * request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 获取保证金余额
        /// </summary>
        /// <returns></returns>
        public AccountWalletInfo AccountGetWalletInfo(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var wallet = (from item in db.t_account_wallet
                              where item.AccountId == basedata.AccountId
                              select item).FirstOrDefault();
                if (wallet == null)
                {
                    return new AccountWalletInfo 
                    {
                        DepositAmount=0
                    };
                }
                return new AccountWalletInfo
                {
                    DepositAmount = wallet.Deposit/100*100
                };
            }
        }

        /// <summary>
        /// 获取保证金余额变更记录
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<AccountWalletChangeRecordInfo> AccountGetWalletChangeRecord(AccountGetWalletChangeRecordRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var changeRecord = from item in db.t_account_wallet_change_record
                                   where item.AccountId == basedata.AccountId && item.Type == 1
                                   select item;
                if (request.Year > 0)
                {
                    changeRecord = from item in changeRecord
                                   where SqlFunctions.DatePart("YEAR", item.CreateTime) == request.Year
                                     select item;
                }
                if (request.Month > 0)
                {
                    changeRecord = from item in changeRecord
                                   where SqlFunctions.DatePart("MONTH", item.CreateTime) == request.Month
                                     select item;
                }
                if (request.MaxId > 0)
                {
                    changeRecord = from item in changeRecord
                                   where item.Id <= request.MaxId
                                   select item;
                }
                int totalCount = changeRecord.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = changeRecord.Max(e => e.Id);
                }

                return new PageRes<AccountWalletChangeRecordInfo>
                {
                    TotalCount = totalCount,
                    MaxId = maxId,
                    List = (from item in changeRecord
                            orderby item.CreateTime descending
                            select new AccountWalletChangeRecordInfo
                            {
                                ChangeAmount = item.ChangeAmount,
                                CreateTime = item.CreateTime,
                                CurAmount = item.CurAmount,
                                Description = item.Description,
                                PreAmount = item.PreAmount
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 检验银行卡是否有效支持
        /// </summary>
        /// <returns></returns>
        public CheckBankCardRes CheckBankCard(CheckBankCardRequest request)
        {
            string res = HtmlSender.GetJson(string.Format("https://ccdcapi.alipay.com/validateAndCacheCardInfo.json?_input_charset=utf-8&cardNo={0}&cardBinCheck=true", request.CardNumber));
            var bankInfo = JsonConvert.DeserializeObject<dynamic>(res);
            if (bankInfo.validated == false)
            {
                throw new WebApiException(400, "银行卡号不正确");
            }
            string bankEnglishName = bankInfo.bank;
            string cardBreed = bankInfo.cardType == "DC" ? "储蓄卡" : bankInfo.cardType == "CC" ? "信用卡" : bankInfo.cardType == "SCC" ? "准贷记卡" : bankInfo.cardType == "PC" ? "预付费卡" : string.Empty;
            if (string.IsNullOrEmpty(cardBreed))
            {
                throw new WebApiException(400, "不支持的银行卡");
            }
            using (var db = new meal_ticketEntities())
            {
                var bankCard = (from item in db.t_bank
                                where item.BankEnglishName == bankEnglishName && item.Status == 1
                                select item).FirstOrDefault();
                if (bankCard == null)
                {
                    throw new WebApiException(400, "不支持的银行卡");
                }
                return new CheckBankCardRes
                {
                    BankCode = bankCard.BankCode,
                    BankName = bankCard.BankName,
                    CardBreed = cardBreed,
                    BankIcon = bankCard.Icon
                };
            }
        }

        /// <summary>
        /// 绑定银行卡
        /// </summary>
        public void BindBankCard(BindBankCardRequest request, HeadBase basedata)
        {
            var card = CheckBankCard(new CheckBankCardRequest
            {
                CardNumber = request.CardNumber
            });
            int ActionCode;
            if (!int.TryParse(request.ActionCode.Replace("S4_", ""), out ActionCode))
            {
                throw new WebApiException(400, "参数错误");
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //判断用户是否实名认证
                    var realname = (from item in db.t_account_realname
                                    where item.AccountId == basedata.AccountId && item.ExamineStatus == 2
                                    select item).FirstOrDefault();
                    if (realname == null)
                    {
                        throw new WebApiException(305, "请先实名认证");
                    }
                    if (realname.RealName != request.RealName)
                    {
                        throw new WebApiException(305, "请输入本人姓名");
                    }
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_BindBankCard(basedata.AccountId, request.Mobile,request.VerifyCode, ActionCode, request.RealName, request.CardNumber, card.BankCode, card.CardBreed, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    
                    //四要素验证
                    string uri = string.Format("https://bcard3and4.market.alicloudapi.com/bankCheck4New?accountNo={0}&idCard={1}&mobile={2}&name={3}", request.CardNumber,realname.CardNo, request.Mobile,realname.RealName);
                    string header = string.Format("Authorization:APPCODE {0}",ConfigurationManager.AppSettings["aliAppKey"]);
                    string res=HtmlSender.Get(uri, "application/json", Encoding.UTF8, header);
                    var resJson = JsonConvert.DeserializeObject<dynamic>(res);
                    if (resJson == null)
                    {
                        throw new WebApiException(400,"为了资金安全，只能绑定当前持卡人的银行卡");
                    }
                    if (resJson.status != "01")
                    {
                        throw new WebApiException(400, "为了资金安全，只能绑定当前持卡人的银行卡");
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
        /// 解绑银行卡
        /// </summary>
        public void UnBindBankCard(DetailsRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var bankCard = (from item in db.t_account_bank_card
                                where item.Id == request.Id && item.AccountId == basedata.AccountId
                                select item).FirstOrDefault();
                if (bankCard == null)
                {
                    throw new WebApiException(400, "银行卡不存在");
                }
                db.t_account_bank_card.Remove(bankCard);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取我的银行卡列表
        /// </summary>
        /// <returns></returns>
        public List<BankCardInfo> GetMyBankCardList(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var card = (from item in db.t_account_bank_card
                            join item2 in db.t_bank on item.BankCode equals item2.BankCode
                            where item.AccountId == basedata.AccountId && item2.Status == 1
                            select new BankCardInfo
                            {
                                BankCardId = item.Id,
                                BankIcon = item2.Icon,
                                BankName = item2.BankName,
                                CardBreed = item.CardBreed,
                                CardNumber = item.CardNumber,
                                Background = item2.Background
                            }).ToList();
                return card;
            }
        }

        /// <summary>
        /// 获取支持的银行列表
        /// </summary>
        /// <returns></returns>
        public List<BankInfo> GetSupportBankList()
        {
            using (var db = new meal_ticketEntities())
            {
                var bank = (from item in db.t_bank
                            where item.Status == 1
                            select new BankInfo
                            {
                                Background = item.Background,
                                BankCode = item.BankCode,
                                BankIcon = item.Icon,
                                BankName = item.BankName
                            }).ToList();
                return bank;
            }
        }

        /// <summary>
        /// 申请提现
        /// </summary>
        public void CashApply(CashApplyRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_CashApply(basedata.AccountId, request.BankCardId, request.Amount,request.CashType, request.TransactionPassword.ToMD5(), errorCodeDb, errorMessageDb);
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
        /// 撤销提现申请
        /// </summary>
        /// <param name="request"></param>
        public void CancelCashApply(DetailsRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_CashCancel(basedata.AccountId, request.Id, errorCodeDb, errorMessageDb);
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
        /// 获取我的提现记录
        /// </summary>
        /// <returns></returns>
        public PageRes<CashRecordInfo> GetMyCashRecordList(GetMyCashRecordListRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var record = from item in db.t_account_cash_record
                             where item.AccountId == basedata.AccountId
                             select item;
                if (request.Year != 0)
                {
                    record = from item in record
                             where SqlFunctions.DatePart("YEAR", item.CreateTime) == request.Year
                             select item;
                }
                if (request.Month != 0)
                {
                    record = from item in record
                             where SqlFunctions.DatePart("MONTH", item.CreateTime) == request.Month
                             select item;
                }
                if (request.MaxId > 0)
                {
                    record = from item in record
                             where item.Id <= request.MaxId
                             select item;
                }
                int totalCount = record.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = record.Max(e=>e.Id);
                }
                var list = (from item in record
                            orderby item.CreateTime descending
                            select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                var result = (from item in list
                              select new CashRecordInfo
                              {
                                  ServiceFee = item.ServiceFee,
                                  Id=item.Id,
                                  ServiceFeeRate = item.ServiceFeeRate,
                                  Status = item.Status,
                                  StatusDes = item.StatusDes,
                                  Amount = item.Amount,
                                  CashType=item.CashType,
                                  ApplyAmount = item.ApplyAmount,
                                  CreateTime = item.CreateTime,
                                  Description = "余额提现",
                                  Date = item.CreateTime.ToString("yyyy年MM月")
                              }).ToList();
                return new PageRes<CashRecordInfo> 
                {
                    List= result,
                    MaxId=maxId,
                    TotalCount=totalCount
                } ;
            }
        }

        /// <summary>
        /// 查询我的提现可退款金额
        /// </summary>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public CashRefundInfo GetMyCashRefundInfo(HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                long refundAmount = 0;
                var wallet = (from item in db.t_account_wallet
                              where item.AccountId == basedata.AccountId
                              select item).FirstOrDefault();
                if (wallet != null)
                {
                    refundAmount = wallet.Deposit;
                }

                var rechargeRecord = from item in db.t_recharge_record
                                     where item.AccountId == basedata.AccountId && item.SupportRefund == true && item.PayStatus == 4 && item.PayAmount - item.RefundedMoney > 0
                                     select item;
                if (rechargeRecord.Count() > 0)
                {
                    var temp = rechargeRecord.Sum(e => e.PayAmount - e.RefundedMoney);
                    if (refundAmount > temp)
                    {
                        refundAmount = temp;
                    }
                }
                else
                {
                    refundAmount = 0;
                }

                //查询转账费率
                var par = (from item in db.t_system_param
                           where item.ParamName == "CashRules"
                           select item).FirstOrDefault();
                if (par == null)
                {
                    refundAmount = 0;
                }

                var tempService=JsonConvert.DeserializeObject<dynamic>(par.ParamValue);
                long MaxAmount = tempService.MaxAmount;
                if (MaxAmount != -1)
                {
                    if (refundAmount > MaxAmount)
                    {
                        refundAmount = MaxAmount;
                    }
                }

                int MinServiceFee = tempService.MinServiceFee;
                int ServiceRate = tempService.ServiceRate;

                long ServiceAmount = (long)(refundAmount * (ServiceRate * 1.0 / 100000)) / 100 * 100;
                if (ServiceAmount < MinServiceFee)
                {
                    MinServiceFee = MinServiceFee;
                }

                return new CashRefundInfo
                {
                    MaxRefundAmount=refundAmount,
                    SaveAmount=MinServiceFee
                };
            }
        }

        /// <summary>
        /// 生成我的专属海报
        /// </summary>
        public object AccountCreatePosters(HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var account = (from item in db.t_account_baseinfo
                                where item.Id == basedata.AccountId
                                select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400,"账户不存在");
                }
                string PostersComposeUrl = account.PostersComposeUrl;
                string RegisterPageUrl = account.RegisterPageUrl;

                bool RegisterPageUrlChange = false;
                //获取二维码跳转地址
                var par = (from item in db.t_system_param
                           where item.ParamName == "RegisterPageUrl"
                           select item).FirstOrDefault();
                if (par != null && par.ParamValue != RegisterPageUrl)
                {
                    account.RegisterPageUrl = par.ParamValue;
                    string qrCodePar = string.Format("{0}?referRecommandCode={1}", par.ParamValue, account.RecommandCode);
                    byte[] qrCodeBytes = FXCommon.Common.Utils.CreateQrCode(qrCodePar);
                    QiNiuHelper qiNiu = new QiNiuHelper();
                    string errorMessage = "";
                    string path = qiNiu.Upload("png", qrCodeBytes, "Other.Img", out errorMessage);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        throw new Exception(errorMessage);
                    }
                    account.QrCodeUrl = path;
                    db.SaveChanges();
                    RegisterPageUrlChange = true;
                }

                //获取海报地址
                var poster = (from item in db.t_page_setting
                              where item.Status == 1 && item.PageCode == "ShareFriends"
                              select item).FirstOrDefault();
                if (poster != null)
                {
                    var content = JsonConvert.DeserializeObject<dynamic>(poster.Content);

                    string postersUrl = content.ImgUrl;
                    if (postersUrl != account.PostersUrl || RegisterPageUrlChange)
                    {
                        int QrcodeX = content.qrcodeX;
                        int QrcodeY = content.qrcodeY;
                        int QrcodeWidth = content.qrcodeWidth;
                        int QrcodeHeight = content.qrcodeHeight;

                        PostersComposeUrl = FXCommon.Common.Utils.CombinImage(postersUrl, string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], account.QrCodeUrl), QrcodeX, QrcodeY, QrcodeWidth, QrcodeHeight);

                        account.PostersUrl = postersUrl;
                        account.PostersComposeUrl = PostersComposeUrl;
                        db.SaveChanges();
                    }
                }

                return new 
                {
                    PostersComposeUrl= string.Format("{0}/{1}",ConfigurationManager.AppSettings["imgurl"],PostersComposeUrl)
                };
            }
        }

        /// <summary>
        /// 获取我的实名认证信息
        /// </summary>
        /// <returns></returns>
        public RealNameInfo GetMyRealNameInfo(HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var realname = (from item in db.t_account_realname
                                where item.AccountId == basedata.AccountId
                                select  new RealNameInfo 
                                {
                                    Sex=item.Sex,
                                    ExamineStatus=item.ExamineStatus,
                                    ImgUrlBack=item.ImgUrlBack,
                                    ExamineStatusDes=item.ExamineStatusDes,
                                    Address=item.Address,
                                    BirthDay=item.BirthDay,
                                    CheckOrg=item.CheckOrg,
                                    ValidDateFrom=item.ValidDateFrom,
                                    ValidDateTo=item.ValidDateTo,
                                    CardNo=item.CardNo,
                                    ImgUrlFront=item.ImgUrlFront,
                                    RealName=item.RealName
                                }).FirstOrDefault();
                return realname;
            }
        }

        /// <summary>
        /// 提交实名认证信息
        /// </summary>
        public void AccountApplyRealName(ApplyRealNameRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                        ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                        db.P_AccountApplyRealName(basedata.AccountId, request.ImgUrlFront, request.ImgUrlBack, request.RealName, request.Sex, request.BirthDay, request.CardNo, request.Address, request.CheckOrg, request.ValidDateFrom, request.ValidDateTo, errorCodeDb, errorMessageDb);
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

                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        //判断是否自动认证
                        bool IsAutomaticRealName = false;
                        var sysPar = (from item in db.t_system_param
                                      where item.ParamName == "IsAutomaticRealName"
                                      select item).FirstOrDefault();
                        if (sysPar != null)
                        {
                            IsAutomaticRealName = sysPar.ParamValue == "true" ? true : false;
                        }
                        if (IsAutomaticRealName)
                        {
                            //获取身份证正面图片
                            var accountRealName = (from item in db.t_account_realname
                                                   where item.AccountId == basedata.AccountId
                                                   select item).FirstOrDefault();
                            if (accountRealName == null)
                            {
                                return;
                            }
                            if (accountRealName.ExamineStatus != 1)
                            {
                                return;
                            }
                            if (string.IsNullOrEmpty(accountRealName.ImgUrlFront))
                            {
                                return;
                            }
                            if (!accountRealName.ImgUrlFront.ToUpper().Contains("HTTP"))
                            {
                                accountRealName.ImgUrlFront = string.Format("{0}/{1}",ConfigurationManager.AppSettings["imgurl"], accountRealName.ImgUrlFront);
                            }

                            //获取网络图片
                            Stream stream = WebRequest.Create(accountRealName.ImgUrlFront).GetResponse().GetResponseStream();
                            byte[] data = new byte[1024];
                            int length = 0;
                            MemoryStream ms = new MemoryStream();
                            while ((length = stream.Read(data, 0, data.Length)) > 0)
                            {
                                ms.Write(data, 0, length);
                            }
                            ms.Seek(0, SeekOrigin.Begin);

                            data = new byte[ms.Length];
                            ms.Seek(0, SeekOrigin.Begin);
                            ms.Read(data, 0, data.Length);

                            //上传正面图片
                            //识别文字
                            string uri = string.Format("https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id={0}&client_secret={1}", ConfigurationManager.AppSettings["baiduAppKey"], ConfigurationManager.AppSettings["baiduSecretKey"]);
                            string res = HtmlSender.GetJson(uri);
                            var resJson = JsonConvert.DeserializeObject<dynamic>(res);
                            string access_token = resJson.access_token;

                            if (!string.IsNullOrEmpty(access_token))
                            {
                                uri = string.Format("https://aip.baidubce.com/rest/2.0/ocr/v1/idcard?access_token={0}", access_token);
                                string content = string.Format("id_card_side=front&image={0}",HttpUtility.UrlEncode(Convert.ToBase64String(data)));
                                res = HtmlSender.Request(content, uri, "application/x-www-form-urlencoded", Encoding.UTF8, "POST");
                                resJson = JsonConvert.DeserializeObject<dynamic>(res);
                                if (resJson != null && resJson.words_result != null)
                                {
                                    string RealName = resJson.words_result.姓名.words;
                                    int Sex = resJson.words_result.性别.words == "男" ? 1 : resJson.words_result.性别.words == "女" ? 2 : 0;
                                    string BirthDay = resJson.words_result.出生.words;
                                    string CardNo = resJson.words_result.公民身份号码.words;
                                    string Address = resJson.words_result.住址.words;
                                    //实名
                                    if (accountRealName.RealName == RealName && accountRealName.Sex == Sex && accountRealName.BirthDay == BirthDay && accountRealName.CardNo == CardNo && accountRealName.Address == Address)
                                    {
                                        accountRealName.ExamineStatus = 2;
                                        accountRealName.ExamineStatusDes = "";
                                        accountRealName.ExamineFinishTime = DateTime.Now;

                                        var accountInfo = (from item in db.t_account_baseinfo
                                                           where item.Id == basedata.AccountId
                                                           select item).FirstOrDefault();
                                        if (accountInfo == null)
                                        {
                                            throw new WebApiException(400, "用户不存在");
                                        }
                                        accountInfo.BirthDay = accountRealName.BirthDay;
                                        accountInfo.Sex = accountRealName.Sex;
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Logger.WriteFileLog("自动实名出错",ex);
                    }
                }
            }
        }

        /// <summary>
        /// 获取客服信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ServiceInfo GetServiceInfo(GetServiceInfoRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var service = (from item in db.t_customerservice_setting
                               where item.PageCode == request.PageCode && item.Status == 1
                               select item).FirstOrDefault();
                if (service == null)
                {
                    return new ServiceInfo();
                }

                return new ServiceInfo 
                {
                    ImgUrl=service.ImgUrl,
                    Mobile=service.Mobile,
                    WechatNumber=service.WechatNumber
                };
            }
        }
    }
}
