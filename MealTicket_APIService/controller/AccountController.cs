using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using FXCommon.Common;
using MealTicket_APIService.Filter;
using MealTicket_Handler;
using MealTicket_Handler.Model;
using Ninject;
using SMS_SendHandler;

namespace MealTicket_APIService.controller
{
    [RoutePrefix("account")]
    public class AccountController:ApiController
    {
        private AccountHandler accountHandler;

        public AccountController()
        {
            accountHandler = WebApiManager.Kernel.Get<AccountHandler>();
        }

        /// <summary>
        /// 用户登入
        /// </summary>
        /// <returns></returns>
        [Description("用户登入")]
        [Route("login"), HttpPost]
        public AccountLoginInfo AccountLogin(AccountLoginRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.AccountLogin(request, basedata);
        }

        /// <summary>
        /// 用户首次设置交易密码
        /// </summary>
        /// <returns></returns>
        [Description("用户首次设置交易密码")]
        [Route("transactionpassword/set"), HttpPost]
        public AccountLoginInfo AccountSetTransactionPassword(AccountSetTransactionPasswordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.AccountSetTransactionPassword(request,basedata);
        }

        /// <summary>
        /// 检查手机号是否已经注册
        /// </summary>
        /// <returns></returns>
        [Description("检查手机号是否已经注册")]
        [Route("register/check"), HttpPost]
        public object CheckAccountRegister(CheckAccountRegisterRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return accountHandler.CheckAccountRegister(request);
        }

        /// <summary>
        /// 获取图片验证码
        /// </summary>
        /// <returns></returns>
        [Description("获取图片验证码")]
        [Route("picverifycode"), HttpPost]
        public object GetPicVerifyCode(GetPicVerifyCodeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (!request.Mobile.CheckPhone())
            {
                throw new WebApiException(400, "手机号格式不正确");
            }
            if (request.BusinessType != 1 && request.BusinessType != 2 && request.BusinessType != 3 && request.BusinessType != 4)
            {
                throw new WebApiException(400, "参数错误");
            }
            string code = Utils.CreateRandomCode(4);
            string filePath = Utils.CreateValidateGraphic(code);
            //图片验证码入库
            accountHandler.CreatePicVerifyCode(request.Mobile, request.BusinessType, code, filePath);
            return string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], filePath);
        }

        /// <summary>
        /// 检查图片验证码是否正确
        /// </summary>
        /// <returns></returns>
        [Description("检查图片验证码是否正确")]
        [Route("picverifycode/check"), HttpPost]
        public object CheckPicVerifyCode(CheckPicVerifyCodeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            accountHandler.CheckPicVerifyCode(request);
            return null;
        }

        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <returns></returns>
        [Description("发送短信验证码")]
        [Route("sendverifycode"), HttpPost]
        public object SendVerifyCode(SendVerifyCodeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (request.BusinessType != 1 && request.BusinessType != 2 && request.BusinessType != 3 && request.BusinessType != 4)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (!request.Mobile.CheckPhone())
            {
                throw new WebApiException(400, "手机号格式不正确");
            }
            if (string.IsNullOrEmpty(request.PicVerifycode))
            {
                throw new WebApiException(400, "请输入图片验证码");
            }
            //创建验证码
            //生成6位数字验证码
            Random random = new Random();
            string VerifyCode = random.Next(0, 999999).ToString("000000");
            int operationCode = accountHandler.CreateSmsVerifyCode(request, VerifyCode);
            //发送验证码
            var par = new Dictionary<string, string>();
            par.Add("code", VerifyCode);
            try
            {
                var JG_SmsSend = new ThirdSms_JG(ConfigurationManager.AppSettings["jg_sms_appKey"], ConfigurationManager.AppSettings["jg_sms_masterSecret"]);
                var res = JG_SmsSend.SendSms(new SmsSendInfo
                {
                    Mobile = request.Mobile,
                    TemplateId = ConfigurationManager.AppSettings["codeTemplateId"],
                    TemplateParameters = par,
                    SignId = ConfigurationManager.AppSettings["codeSignId"],
                });
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("验证码发送失败", ex);
            }
            //返回操作码
            return "S" + request.BusinessType + "_" + operationCode.ToString("00");
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <returns></returns>
        [Description("用户注册")]
        [Route("register"), HttpPost]
        public object AccountRegister(AccountRegisterRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (string.IsNullOrEmpty(request.ActionCode))
            {
                throw new WebApiException(400, "请填写验证码");
            }
            if (string.IsNullOrEmpty(request.Verifycode))
            {
                throw new WebApiException(400, "请填写验证码");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            accountHandler.AccountRegister(request, basedata);
            return null;
        }

        /// <summary>
        /// 忘记密码重置
        /// </summary>
        /// <returns></returns>
        [Description("忘记密码重置")]
        [Route("password/forget"), HttpPost]
        public object AccountForgetPassword(AccountForgetPasswordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (string.IsNullOrEmpty(request.ActionCode))
            {
                throw new WebApiException(400, "请填写验证码");
            }
            if (string.IsNullOrEmpty(request.Verifycode))
            {
                throw new WebApiException(400, "请填写验证码");
            }
            accountHandler.AccountForgetPassword(request);
            return null;
        }

        /// <summary>
        /// 修改手机号
        /// </summary>
        /// <returns></returns>
        [Description("修改手机号")]
        [Route("mobile/modify"), HttpPost]
        [CheckUserLoginFilter]
        public AccountLoginInfo AccountModifyMobile(AccountModifyMobileRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (string.IsNullOrEmpty(request.ActionCode))
            {
                throw new WebApiException(400, "请填写验证码");
            }
            if (string.IsNullOrEmpty(request.Verifycode))
            {
                throw new WebApiException(400, "请填写验证码");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.AccountModifyMobile(request, basedata);
        }

        /// <summary>
        /// 修改登录密码
        /// </summary>
        /// <returns></returns>
        [Description("修改登录密码")]
        [Route("password/modify"), HttpPost]
        [CheckUserLoginFilter]
        public AccountLoginInfo AccountModifyPassword(AccountModifyPasswordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.AccountModifyPassword(request, basedata);
        }

        /// <summary>
        /// 修改交易密码
        /// </summary>
        /// <returns></returns>
        [Description("修改交易密码")]
        [Route("transactionpassword/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object AccountModifyTransactionPassword(AccountModifyTransactionPasswordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            accountHandler.AccountModifyTransactionPassword(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改个人信息
        /// </summary>
        /// <returns></returns>
        [Description("修改个人信息")]
        [Route("baseinfo/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object AccountModifyBaseInfo(AccountModifyBaseInfoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            accountHandler.AccountModifyBaseInfo(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取用户基本信息
        /// </summary>
        /// <returns></returns>
        [Description("获取用户基本信息")]
        [Route("baseinfo"), HttpPost]
        [CheckUserLoginFilter]
        public AccountBaseInfo AccountGetBaseInfo()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.AccountGetBaseInfo(basedata);
        }

        /// <summary>
        /// 搜索用户信息
        /// </summary>
        /// <returns></returns>
        [Description("搜索用户信息")]
        [Route("baseinfo/query"), HttpPost]
        [CheckUserLoginFilter]
        public List<AccountBaseInfo> QueryAccountBaseInfo(QueryAccountBaseInfoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.QueryAccountBaseInfo(request);
        }

        /// <summary>
        /// 获取推荐人基本信息
        /// </summary>
        /// <returns></returns>
        [Description("获取推荐人基本信息")]
        [Route("refer/baseinfo"), HttpPost]
        [CheckUserLoginFilter]
        public AccountBaseInfo AccountGetReferBaseInfo()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.AccountGetReferBaseInfo(basedata);
        }

        /// <summary>
        /// 设置推荐人
        /// </summary>
        /// <returns></returns>
        [Description("设置推荐人")]
        [Route("refer/set"), HttpPost]
        [CheckUserLoginFilter]
        public object AccountSetRefer(AccountSetReferRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            accountHandler.AccountSetRefer(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询被邀请人列表
        /// </summary>
        /// <returns></returns>
        [Description("查询被邀请人列表")]
        [Route("invitees/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AccountBaseInfo> AccountGetInviteesList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.AccountGetInviteesList(request,basedata);
        }

        /// <summary>
        /// 获取保证金余额
        /// </summary>
        /// <returns></returns>
        [Description("获取保证金余额")]
        [Route("wallet/info"), HttpPost]
        [CheckUserLoginFilter]
        public AccountWalletInfo AccountGetWalletInfo()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.AccountGetWalletInfo(basedata);
        }

        /// <summary>
        /// 获取保证金余额变更记录
        /// </summary>
        /// <returns></returns>
        [Description("获取保证金余额变更记录")]
        [Route("wallet/change/record"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AccountWalletChangeRecordInfo> AccountGetWalletChangeRecord(AccountGetWalletChangeRecordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.AccountGetWalletChangeRecord(request,basedata);
        }

        /// <summary>
        /// 检验银行卡是否有效支持
        /// </summary>
        /// <returns></returns>
        [Description("检验银行卡是否有效支持")]
        [CheckUserLoginFilter]
        [Route("bank/card/check"), HttpPost]
        public CheckBankCardRes CheckBankCard(CheckBankCardRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return accountHandler.CheckBankCard(request);
        }

        /// <summary>
        /// 绑定银行卡
        /// </summary>
        /// <returns></returns>
        [Description("绑定银行卡")]
        [CheckRealNameFilter]
        [Route("bank/card/bind"), HttpPost]
        public object BindBankCard(BindBankCardRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (string.IsNullOrEmpty(request.ActionCode))
            {
                throw new WebApiException(400, "请填写验证码");
            }
            if (string.IsNullOrEmpty(request.VerifyCode))
            {
                throw new WebApiException(400, "请填写验证码");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            accountHandler.BindBankCard(request, basedata);
            return null;
        }

        /// <summary>
        /// 解绑银行卡
        /// </summary>
        /// <returns></returns>
        [Description("解绑银行卡")]
        [CheckUserLoginFilter]
        [Route("bank/card/unbind"), HttpPost]
        public object UnBindBankCard(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            accountHandler.UnBindBankCard(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取我的银行卡列表
        /// </summary>
        /// <returns></returns>
        [Description("获取我的银行卡列表")]
        [CheckUserLoginFilter]
        [Route("bank/card/list"), HttpPost]
        public List<BankCardInfo> GetMyBankCardList()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.GetMyBankCardList(basedata);
        }

        /// <summary>
        /// 获取支持的银行列表
        /// </summary>
        /// <returns></returns>
        [Description("获取支持的银行列表")]
        [Route("bank/support/list"), HttpPost]
        public List<BankInfo> GetSupportBankList()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.GetSupportBankList();
        }

        /// <summary>
        /// 申请提现
        /// </summary>
        /// <returns></returns>
        [Description("申请提现")]
        [CheckRealNameFilter]
        [Route("cash/apply"), HttpPost]
        public object CashApply(CashApplyRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (request.CashType != 1 && request.CashType != 2 && request.CashType != 3)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (string.IsNullOrEmpty(request.TransactionPassword))
            {
                throw new WebApiException(400, "请输入交易密码");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            accountHandler.CashApply(request, basedata);
            return null;
        }

        /// <summary>
        /// 撤销提现申请
        /// </summary>
        /// <returns></returns>
        [Description("撤销提现申请")]
        [CheckRealNameFilter]
        [Route("cash/cancel"), HttpPost]
        public object CancelCashApply(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            accountHandler.CancelCashApply(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询我的提现记录
        /// </summary>
        /// <returns></returns>
        [Description("查询我的提现记录")]
        [CheckUserLoginFilter]
        [Route("cash/record/list"), HttpPost]
        public PageRes<CashRecordInfo> GetMyCashRecordList(GetMyCashRecordListRequest request)
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.GetMyCashRecordList(request, basedata);
        }

        /// <summary>
        /// 查询我的提现可退款金额
        /// </summary>
        /// <returns></returns>
        [Description("查询我的提现可退款金额")]
        [CheckUserLoginFilter]
        [Route("cash/refund/info"), HttpPost]
        public CashRefundInfo GetMyCashRefundInfo()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.GetMyCashRefundInfo(basedata);
        }

        /// <summary>
        /// 生成我的专属海报
        /// </summary>
        /// <returns></returns>
        [Description("生成我的专属海报")]
        [CheckUserLoginFilter]
        [Route("posters/create"), HttpPost]
        public object AccountCreatePosters()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.AccountCreatePosters(basedata);
        }

        /// <summary>
        /// 获取我的实名认证信息
        /// </summary>
        /// <returns></returns>
        [Description("获取我的实名认证信息")]
        [CheckUserLoginFilter]
        [Route("realname/info"), HttpPost]
        public RealNameInfo GetMyRealNameInfo()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return accountHandler.GetMyRealNameInfo(basedata);
        }

        /// <summary>
        /// 提交实名认证信息
        /// </summary>
        /// <returns></returns>
        [Description("提交实名认证信息")]
        [CheckUserLoginFilter]
        [Route("realname/apply"), HttpPost]
        public object AccountApplyRealName(ApplyRealNameRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            accountHandler.AccountApplyRealName(request,basedata);
            return null;
        }

        /// <summary>
        /// 获取客服信息
        /// </summary>
        /// <returns></returns>
        [Route("service/info"), HttpPost]
        [Description("获取客服信息")]
        public ServiceInfo GetServiceInfo(GetServiceInfoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            return accountHandler.GetServiceInfo(request);
        }
    }
}
