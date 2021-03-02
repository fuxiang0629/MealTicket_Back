using MealTicket_Admin_APIService.Filter;
using MealTicket_Admin_Handler;
using MealTicket_Admin_Handler.Model;
using Ninject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MealTicket_Admin_APIService.controller
{
    [RoutePrefix("frontaccount")]
    public class FrontAccountController:ApiController
    {
        private FrontAccountHandler frontAccountHandler;

        public FrontAccountController()
        {
            frontAccountHandler = WebApiManager.Kernel.Get<FrontAccountHandler>();
        }

        #region====账户管理====
        /// <summary>
        /// 查询前端账户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("list"),HttpPost]
        [Description("查询前端账户列表")]
        public PageRes<FrontAccountInfo> GetFrontAccountList(GetFrontAccountListRequest request) 
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            return frontAccountHandler.GetFrontAccountList(request);
        }

        /// <summary>
        /// 编辑前端账户基础信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("baseinfo/modify")]
        [Description("编辑前端账户基础信息")]
        public object ModifyFrontAccountBaseInfo(ModifyFrontAccountBaseInfoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountBaseInfo(request);
            return null;
        }

        /// <summary>
        /// 修改前端账户推荐人
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("refer/modify")]
        [Description("修改前端账户推荐人")]
        public object ModifyFrontAccountRefer(ModifyFrontAccountReferRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountRefer(request);
            return null;
        }

        /// <summary>
        /// 修改前端账户状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("status/modify")]
        [Description("修改前端账户状态")]
        public object ModifyFrontAccountStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountStatus(request);
            return null;
        }

        /// <summary>
        /// 修改前端账户交易状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("trade/status/modify")]
        [Description("修改前端账户交易状态")]
        public object ModifyFrontAccountTradeStatus(ModifyFrontAccountTradeStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountTradeStatus(request);
            return null;
        }

        /// <summary>
        /// 修改前端账户监控状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("monitor/status/modify")]
        [Description("修改前端账户监控状态")]
        public object ModifyFrontAccountMonitorStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountMonitorStatus(request);
            return null;
        }

        /// <summary>
        /// 修改前端账户提现状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("cash/status/modify")]
        [Description("修改前端账户提现状态")]
        public object ModifyFrontAccountCashStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountCashStatus(request);
            return null;
        }

        /// <summary>
        /// 修改前端账户登录密码
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("loginpassword/modify")]
        [Description("修改前端账户登录密码")]
        public object ModifyFrontAccountLoginPassword(ModifyFrontAccountLoginPasswordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountLoginPassword(request);
            return null;
        }

        /// <summary>
        /// 修改前端账户交易密码
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("transactionpassword/modify")]
        [Description("修改前端账户交易密码")]
        public object ModifyFrontAccountTransactionPassword(ModifyFrontAccountTransactionPasswordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountTransactionPassword(request);
            return null;
        }

        /// <summary>
        /// 修改前端账户钱包状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("wallet/status/modify")]
        [Description("修改前端账户钱包状态")]
        public object ModifyFrontAccountWalletStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountWalletStatus(request);
            return null;
        }

        /// <summary>
        /// 修改前端账户余额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("wallet/modify")]
        [Description("修改前端账户余额")]
        public object ModifyFrontAccountWallet(ModifyFrontAccountWalletRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountWallet(request);
            return null;
        }

        /// <summary>
        /// 查询前端账户余额变更记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("wallet/change/record"), HttpPost]
        [Description("查询前端账户余额变更记录")]
        public PageRes<FrontAccountWalletChangeRecordInfo> GetFrontAccountWalletChangeRecord(GetFrontAccountWalletChangeRecordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountWalletChangeRecord(request);
        }

        /// <summary>
        /// 查询前端账户银行卡列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("bankcard/list"),HttpPost]
        [Description("查询前端账户银行卡列表")]
        public PageRes<FrontAccountBankCardInfo> GetFrontAccountBankCardList(GetFrontAccountBankCardListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountBankCardList(request);
        }

        /// <summary>
        /// 查询前端账户推荐奖励类别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("recommend/prize/type"), HttpPost]
        [Description("查询前端账户推荐奖励类别")]
        public PageRes<FrontAccountRecommendPrizeTypeInfo> GetFrontAccountRecommendPrizeType(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountRecommendPrizeType(request);
        }

        /// <summary>
        /// 修改前端账户推荐奖励类别状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("recommend/prize/type/status/modify"), HttpPost]
        [Description("修改前端账户推荐奖励类别状态")]
        public object ModifyFrontAccountRecommendPrizeTypeStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountRecommendPrizeTypeStatus(request);
            return null;
        }

        /// <summary>
        /// 查询前端账户推荐奖励分类级别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("recommend/prize/type/level"), HttpPost]
        [Description("查询前端账户推荐奖励分类级别")]
        public PageRes<FrontAccountRecommendPrizeTypeLevelInfo> GetFrontAccountRecommendPrizeTypeLevel(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountRecommendPrizeTypeLevel(request);
        }

        /// <summary>
        /// 添加前端账户推荐奖励分类级别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("recommend/prize/type/level/add"), HttpPost]
        [Description("添加前端账户推荐奖励分类级别")]
        public object AddFrontAccountRecommendPrizeTypeLevel(AddFrontAccountRecommendPrizeTypeLevelRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.AddFrontAccountRecommendPrizeTypeLevel(request);
            return null;
        }

        /// <summary>
        /// 编辑前端账户推荐奖励分类级别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("recommend/prize/type/level/modify"), HttpPost]
        [Description("编辑前端账户推荐奖励分类级别")]
        public object ModifyFrontAccountRecommendPrizeTypeLevel(ModifyFrontAccountRecommendPrizeTypeLevelRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountRecommendPrizeTypeLevel(request);
            return null;
        }

        /// <summary>
        /// 修改前端账户推荐奖励分类级别状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("recommend/prize/type/level/status/modify"), HttpPost]
        [Description("修改前端账户推荐奖励分类级别状态")]
        public object ModifyFrontAccountRecommendPrizeTypeLevelStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountRecommendPrizeTypeLevelStatus(request);
            return null;
        }

        /// <summary>
        /// 删除前端账户推荐奖励分类级别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("recommend/prize/type/level/delete"), HttpPost]
        [Description("删除前端账户推荐奖励分类级别")]
        public object DeleteFrontAccountRecommendPrizeTypeLevel(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.DeleteFrontAccountRecommendPrizeTypeLevel(request);
            return null;
        }

        /// <summary>
        /// 查询前端账户交易费用配置列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("tradepar/setting/list"), HttpPost]
        [Description("查询前端账户交易费用配置列表")]
        public PageRes<FrontAccountTradeParSettingInfo> GetFrontAccountTradeParSettingList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountTradeParSettingList(request);
        }

        /// <summary>
        /// 添加前端账户交易费用配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("tradepar/setting/add"), HttpPost]
        [Description("添加前端账户交易费用配置")]
        public object AddFrontAccountTradeParSetting(AddFrontAccountTradeParSettingRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.AddFrontAccountTradeParSetting(request);
            return null;
        }

        /// <summary>
        /// 编辑前端账户交易费用配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("tradepar/setting/modify"), HttpPost]
        [Description("编辑前端账户交易费用配置")]
        public object ModifyFrontAccountTradeParSetting(ModifyFrontAccountTradeParSettingRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountTradeParSetting(request);
            return null;
        }

        /// <summary>
        /// 修改前端账户交易费用配置状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("tradepar/setting/status/modify"), HttpPost]
        [Description("修改前端账户交易费用配置状态")]
        public object ModifyFrontAccountTradeParSettingStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountTradeParSettingStatus(request);
            return null;
        }

        /// <summary>
        /// 删除前端账户交易费用配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("tradepar/setting/delete"), HttpPost]
        [Description("删除前端账户交易费用配置")]
        public object DeleteFrontAccountTradeParSetting(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.DeleteFrontAccountTradeParSetting(request);
            return null;
        }

        /// <summary>
        /// 查询前端账户实名认证列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("realname/list"), HttpPost]
        [Description("查询前端账户实名认证列表")]
        public PageRes<FrontAccountRealNameInfo> GetFrontAccountRealNameList(GetFrontAccountRealNameListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountRealNameList(request);
        }

        /// <summary>
        /// 查询前端账户实名认证信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("realname/info"), HttpPost]
        [Description("查询前端账户实名认证信息")]
        public FrontAccountRealNameInfo GetFrontAccountRealNameInfo(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountRealNameInfo(request);
        }

        /// <summary>
        /// 审核前端账户实名认证
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("realname/examine"), HttpPost]
        [Description("审核前端账户实名认证")]
        public object ExamineFrontAccountRealName(ExamineFrontAccountRealNameRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ExamineFrontAccountRealName(request);
            return null;
        }

        /// <summary>
        /// 查询前端账户注册日志
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("register/log"),HttpPost]
        [Description("查询前端账户注册日志")]
        public PageRes<FrontAccountRegisterLogInfo> GetFrontAccountRegisterLog(GetFrontAccountRegisterLogRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountRegisterLog(request);
        }

        /// <summary>
        /// 查询前端账户登录日志
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("login/log"), HttpPost]
        [Description("查询前端账户登录日志")]
        public PageRes<FrontAccountLoginLogInfo> GetFrontAccountLoginLog(GetFrontAccountLoginLogRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountLoginLog(request);
        }

        /// <summary>
        /// 查询用户监控席位列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("seat/list"), HttpPost]
        [Description("查询用户监控席位列表")]
        [CheckUserPowerFilter]
        public PageRes<FrontAccountSeatInfo> GetFrontAccountSeatList(GetFrontAccountSeatListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSeatList(request);
        }

        /// <summary>
        /// 添加用户监控席位
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("seat/add"), HttpPost]
        [Description("添加用户监控席位")]
        [CheckUserPowerFilter]
        public object AddFrontAccountSeat(AddFrontAccountSeatRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.AddFrontAccountSeat(request);
            return null;
        }

        /// <summary>
        /// 批量添加用户监控席位
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("seat/batchadd"), HttpPost]
        [Description("批量添加用户监控席位")]
        [CheckUserPowerFilter]
        public object BatchAddFrontAccountSeat(BatchAddFrontAccountSeatRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.BatchAddFrontAccountSeat(request);
            return null;
        }

        /// <summary>
        /// 编辑用户监控席位
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("seat/modify"), HttpPost]
        [Description("编辑用户监控席位")]
        [CheckUserPowerFilter]
        public object ModifyFrontAccountSeat(ModifyFrontAccountSeatRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountSeat(request);
            return null;
        }

        /// <summary>
        /// 修改用户监控席位状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("seat/status/modify"), HttpPost]
        [Description("修改用户监控席位状态")]
        [CheckUserPowerFilter]
        public object ModifyFrontAccountSeatStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountSeatStatus(request);
            return null;
        }

        /// <summary>
        /// 删除用户监控席位
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("seat/delete"), HttpPost]
        [Description("删除用户监控席位")]
        [CheckUserPowerFilter]
        public object DeleteFrontAccountSeat(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.DeleteFrontAccountSeat(request);
            return null;
        }

        /// <summary>
        /// 查询用户自选股列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/list"), HttpPost]
        [Description("查询用户自选股列表")]
        [CheckUserPowerFilter]
        public PageRes<FrontAccountOptionalInfo> GetFrontAccountOptionalList(GetFrontAccountOptionalListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountOptionalList(request);
        }

        /// <summary>
        /// 添加用户自选股
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/add"), HttpPost]
        [Description("添加用户自选股")]
        [CheckUserPowerFilter]
        public object AddFrontAccountOptional(AddFrontAccountOptionalRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.AddFrontAccountOptional(request);
            return null;
        }

        /// <summary>
        /// 删除用户自选股
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/delete"), HttpPost]
        [Description("删除用户自选股")]
        [CheckUserPowerFilter]
        public object DeleteFrontAccountOptional(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.DeleteFrontAccountOptional(request);
            return null;
        }

        /// <summary>
        /// 批量导入用户自选股
        /// </summary>
        /// <returns></returns>
        [Route("optional/batch/add"), HttpPost]
        [Description("批量导入用户自选股")]
        [CheckUserPowerFilter]
        public async Task<object> BatchAddFrontAccountOptional(long accountId)
        {
            string path = string.Empty;
            // 检查是否是 multipart/form-data 
            if (Request.Content.IsMimeMultipartContent("form-data"))
            {
                if (Request.Content.Headers.ContentLength > 0)
                {
                    // 设置上传目录 
                    string root = System.AppDomain.CurrentDomain.BaseDirectory;
                    var provider = new MultipartFormDataStreamProvider(root);
                    await Request.Content.ReadAsMultipartAsync(provider);

                    if (provider.FileData.Count() > 0)
                    {
                        var file = provider.FileData[0];
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("gb2312").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<MarketTimeInfo> marketTimeList = new List<MarketTimeInfo>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 2)
                            {
                                continue;
                            }
                            var stock = datas[0].Split('.');
                            if (stock.Count() != 2)
                            {
                                continue;
                            }

                            marketTimeList.Add(new MarketTimeInfo
                            {
                                Market = stock[1] == "SZ" ? 0 : 1,
                                SharesCode = int.Parse(stock[0]).ToString("000000"),
                                SharesName = datas[1]
                            });
                        }
                        return frontAccountHandler.BatchAddSharesMonitor(accountId, marketTimeList);
                    }
                    else
                    {
                        throw new WebApiException(400, "上传文件内容不能为空");
                    }
                }
                else
                {
                    throw new WebApiException(400, "上传数据不能为空");
                }
            }
            else
            {
                throw new WebApiException(400, "请求媒体参数不正确，请确保使用的是multipart/form-data方式");
            }
        }

        /// <summary>
        /// 绑定自选股席位
        /// </summary>
        /// <returns></returns>
        [Description("绑定自选股席位")]
        [Route("optional/seat/bind"), HttpPost]
        [CheckUserPowerFilter]
        public object BindFrontAccountOptionalSeat(BindFrontAccountOptionalSeatRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.BindFrontAccountOptionalSeat(request);
            return null;
        }

        /// <summary>
        /// 批量绑定自选股席位
        /// </summary>
        /// <returns></returns>
        [Description("批量绑定自选股席位")]
        [Route("optional/seat/batchbind"), HttpPost]
        [CheckUserPowerFilter]
        public object BatchBindFrontAccountOptionalSeat(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.BatchBindFrontAccountOptionalSeat(request);
        }

        /// <summary>
        /// 查询自选股监控走势
        /// </summary>
        /// <returns></returns>
        [Description("查询自选股监控走势")]
        [Route("optional/trend"), HttpPost]
        [CheckUserPowerFilter]
        public PageRes<FrontAccountOptionalTrendInfo> GetFrontAccountOptionalTrend(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountOptionalTrend(request);
        }

        /// <summary>
        /// 修改自选股监控走势状态
        /// </summary>
        /// <returns></returns>
        [Description("修改自选股监控走势状态")]
        [Route("optional/trend/status/modify"), HttpPost]
        [CheckUserPowerFilter]
        public object ModifyFrontAccountOptionalTrendStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountOptionalTrendStatus(request);
            return null;
        }

        /// <summary>
        /// 查询自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/trend/par"), HttpPost]
        [Description("查询自选股走势参数")]
        [CheckUserPowerFilter]
        public PageRes<FrontAccountOptionalTrendParInfo> GetFrontAccountOptionalTrendPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountOptionalTrendPar(request);
        }

        /// <summary>
        /// 添加自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/trend/par/add"), HttpPost]
        [Description("添加自选股走势参数")]
        [CheckUserPowerFilter]
        public object AddFrontAccountOptionalTrendPar(AddFrontAccountOptionalTrendParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.AddFrontAccountOptionalTrendPar(request);
            return null;
        }

        /// <summary>
        /// 编辑自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/trend/par/modify"), HttpPost]
        [Description("编辑走势模板参数")]
        [CheckUserPowerFilter]
        public object ModifyFrontAccountOptionalTrendPar(ModifyFrontAccountOptionalTrendParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountOptionalTrendPar(request);
            return null;
        }

        /// <summary>
        /// 删除自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/trend/par/delete"), HttpPost]
        [Description("删除自选股走势参数")]
        [CheckUserPowerFilter]
        public object DeleteFrontAccountOptionalTrendPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.DeleteFrontAccountOptionalTrendPar(request);
            return null;
        }

        /// <summary>
        /// 查询自选股走势再触发配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/trend/tri"), HttpPost]
        [Description("查询自选股走势再触发配置")]
        [CheckUserPowerFilter]
        public FrontAccountOptionalTrendTriInfo GetFrontAccountOptionalTrendTri(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountOptionalTrendTri(request);
        }

        /// <summary>
        /// 设置自选股走势再触发配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/trend/tri/modify"), HttpPost]
        [Description("设置自选股走势再触发配置")]
        [CheckUserPowerFilter]
        public object ModifyFrontAccountOptionalTrendTri(ModifyFrontAccountOptionalTrendTriRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountOptionalTrendTri(request);
            return null;
        }

        /// <summary>
        /// 批量更新监控参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("optional/monitor/par/batchupdate"), HttpPost]
        [Description("批量更新监控参数")]
        public object BatchUpdateOptionalMonitorTrendPar(BatchUpdateOptionalMonitorTrendParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.BatchUpdateOptionalMonitorTrendPar(request);
        }

        /// <summary>
        /// 查询跟投用户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("follow/list"), HttpPost]
        [Description("查询跟投用户列表")]
        [CheckUserPowerFilter]
        public PageRes<FrontAccountInfo> GetFrontAccountFollowList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountFollowList(request);
        }

        /// <summary>
        /// 添加跟投用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("follow/add"), HttpPost]
        [Description("查询跟投用户列表")]
        [CheckUserPowerFilter]
        public object AddFrontAccountFollow(AddFrontAccountFollowRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.AddFrontAccountFollow(request);
            return null;
        }

        /// <summary>
        /// 删除跟投用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("follow/delete"), HttpPost]
        [Description("查询跟投用户列表")]
        [CheckUserPowerFilter]
        public object DeleteFrontAccountFollow(DeleteFrontAccountFollowRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.DeleteFrontAccountFollow(request);
            return null;
        }

        /// <summary>
        /// 查询操盘用户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("main/list"), HttpPost]
        [Description("查询操盘用户列表")]
        [CheckUserPowerFilter]
        public PageRes<FrontAccountInfo> GetFrontAccountMainList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountMainList(request);
        }

        /// <summary>
        /// 添加操盘用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("main/add"), HttpPost]
        [Description("添加操盘用户")]
        [CheckUserPowerFilter]
        public object AddFrontAccountMain(AddFrontAccountMainRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.AddFrontAccountMain(request);
            return null;
        }

        /// <summary>
        /// 查询监控股票分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/group"), HttpPost]
        [Description("查询监控股票分组列表")]
        [CheckUserPowerFilter]
        public List<AccountOptionalGroupInfo> GetAccountOptionalGroup(DetailsRequest request)
        {
            return frontAccountHandler.GetAccountOptionalGroup(request);
        }

        /// <summary>
        /// 添加监控股票分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/group/add"), HttpPost]
        [Description("添加监控股票分组")]
        [CheckUserPowerFilter]
        public object AddAccountOptionalGroup(AddAccountOptionalGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.AddAccountOptionalGroup(request);
            return null;
        }

        /// <summary>
        /// 编辑监控股票分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/group/modify"), HttpPost]
        [Description("编辑监控股票分组")]
        [CheckUserPowerFilter]
        public object ModifyAccountOptionalGroup(ModifyAccountOptionalGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyAccountOptionalGroup(request);
            return null;
        }

        /// <summary>
        /// 删除监控股票分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/group/delete"), HttpPost]
        [Description("删除监控股票分组")]
        [CheckUserPowerFilter]
        public object DeleteAccountOptionalGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.DeleteAccountOptionalGroup(request);
            return null;
        }

        /// <summary>
        /// 查询监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/group/rel/list"), HttpPost]
        [Description("查询监控分组内股票")]
        [CheckUserPowerFilter]
        public PageRes<AccountOptionalGroupRelInfo> GetAccountOptionalGroupRelList(GetAccountOptionalGroupRelListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetAccountOptionalGroupRelList(request);
        }

        /// <summary>
        /// 添加监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/group/rel/add"), HttpPost]
        [Description("添加监控分组内股票")]
        [CheckUserPowerFilter]
        public object AddAccountOptionalGroupRel(AddAccountOptionalGroupRelRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.AddAccountOptionalGroupRel(request);
            return null;
        }

        /// <summary>
        /// 删除监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/group/rel/delete"), HttpPost]
        [Description("删除监控分组内股票")]
        [CheckUserPowerFilter]
        public object DeleteAccountOptionalGroupRel(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.DeleteAccountOptionalGroupRel(request);
            return null;
        }

        /// <summary>
        /// 修改监控分组内股票是否持续
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/group/rel/groupiscontinue/modify"), HttpPost]
        [Description("修改监控分组内股票是否持续")]
        [CheckUserPowerFilter]
        public object ModifyAccountOptionalGroupRelGroupIsContinue(ModifyAccountOptionalGroupRelGroupIsContinueRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyAccountOptionalGroupRelGroupIsContinue(request);
            return null;
        }

        /// <summary>
        /// 批量导入监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("optional/group/rel/batchadd"), HttpPost]
        [Description("批量导入监控分组内股票")]
        [CheckUserPowerFilter]
        public async Task<object> BatchAddAccountOptionalGroupRel()
        {
            // 检查是否是 multipart/form-data 
            if (Request.Content.IsMimeMultipartContent("form-data"))
            {
                if (Request.Content.Headers.ContentLength > 0)
                {
                    // 设置上传目录 
                    string root = System.AppDomain.CurrentDomain.BaseDirectory;
                    var provider = new MultipartFormDataStreamProvider(root);
                    await Request.Content.ReadAsMultipartAsync(provider);

                    if (provider.FileData.Count() > 0)
                    {
                        var file = provider.FileData[0];

                        bool GroupIsContinue = bool.Parse(provider.FormData["GroupIsContinue"]);
                        long GroupId = long.Parse(provider.FormData["GroupId"]);
                        DateTime? ValidStartTime;
                        DateTime ValidStartTimeDt;
                        if (DateTime.TryParse(provider.FormData["ValidStartTime"], out ValidStartTimeDt))
                        {
                            ValidStartTime = ValidStartTimeDt;
                        }
                        else
                        {
                            ValidStartTime = null;
                        }
                        DateTime? ValidEndTime;
                        DateTime ValidEndTimeDt;
                        if (DateTime.TryParse(provider.FormData["ValidEndTime"], out ValidEndTimeDt))
                        {
                            ValidEndTime = ValidEndTimeDt;
                        }
                        else
                        {
                            ValidEndTime = null;
                        }

                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("gb2312").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<SharesInfo> sharesList = new List<SharesInfo>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 2)
                            {
                                continue;
                            }
                            var stock = datas[0].Split('.');
                            if (stock.Count() != 2)
                            {
                                continue;
                            }
                            sharesList.Add(new SharesInfo
                            {
                                Market = stock[1] == "SZ" ? 0 : 1,
                                SharesCode = int.Parse(stock[0]).ToString("000000"),
                                SharesName = datas[1]
                            });
                        }
                        return frontAccountHandler.BatchAddAccountOptionalGroupRel(GroupId, GroupIsContinue, ValidStartTime, ValidEndTime, sharesList);
                    }
                    else
                    {
                        throw new WebApiException(400, "上传文件内容不能为空");
                    }
                }
                else
                {
                    throw new WebApiException(400, "上传数据不能为空");
                }
            }
            else
            {
                throw new WebApiException(400, "请求媒体参数不正确，请确保使用的是multipart/form-data方式");
            }
        }

        /// <summary>
        /// 查询用户杠杆配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("fundmultiple/list"), HttpPost]
        [Description("查询用户杠杆配置")]
        [CheckUserPowerFilter]
        public PageRes<AccountFundmultipleInfo> GetAccountFundmultipleList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetAccountFundmultipleList(request);
        }

        /// <summary>
        /// 添加用户杠杆配置
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("fundmultiple/add"), HttpPost]
        [Description("添加用户杠杆配置")]
        public object AddAccountFundmultiple(AddAccountFundmultipleRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.AddAccountFundmultiple(request);
            return null;
        }

        /// <summary>
        /// 编辑用户杠杆配置
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("fundmultiple/modify"), HttpPost]
        [Description("编辑用户杠杆配置")]
        public object ModifyAccountFundmultiple(ModifyAccountFundmultipleRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyAccountFundmultiple(request);
            return null;
        }

        /// <summary>
        /// 删除用户杠杆配置
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("fundmultiple/delete"), HttpPost]
        [Description("删除用户杠杆配置")]
        public object DeleteAccountFundmultiple(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.DeleteAccountFundmultiple(request);
            return null;
        }

        /// <summary>
        /// 查询用户跟投申请列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("follow/apply/list"), HttpPost]
        [Description("查询用户跟投申请列表")]
        [CheckUserPowerFilter]
        public PageRes<AccountFollowApplyInfo> GetAccountFollowApplyList(GetAccountFollowApplyListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetAccountFollowApplyList(request);
        }

        /// <summary>
        /// 设置用户跟投申请状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("follow/apply/status/modify"), HttpPost]
        [Description("设置用户跟投申请状态")]
        [CheckUserPowerFilter]
        public object ModifyAccountFollowApplyStatus(ModifyAccountFollowApplyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyAccountFollowApplyStatus(request);
            return null;
        }
        #endregion

        #region====交易管理====
        /// <summary>
        /// 查询今日交易统计信息
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/trade/today/statistics"), HttpPost]
        [Description("查询今日交易统计信息")]
        public FrontAccountSharesTradeTodayStatisticsInfo GetFrontAccountSharesTradeTodayStatistics()
        {
            return frontAccountHandler.GetFrontAccountSharesTradeTodayStatistics();
        }

        /// <summary>
        /// 查询交易排行列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/trade/rank/list"), HttpPost]
        [Description("查询交易排行列表")]
        public PageRes<FrontAccountSharesTradeRankInfo> GetFrontAccountSharesTradeRankList(GetFrontAccountSharesTradeRankListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesTradeRankList(request);
        }

        /// <summary>
        /// 查询前端账户持仓统计
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/hold/statistics"), HttpPost]
        [Description("查询前端账户持仓统计")]
        public PageRes<FrontAccountSharesHoldStatisticsInfo> GetFrontAccountSharesHoldStatistics(GetFrontAccountSharesHoldStatisticsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesHoldStatistics(request);
        }

        /// <summary>
        /// 查询前端账户持仓列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/hold/list"), HttpPost]
        [Description("查询前端账户持仓列表")]
        public PageRes<FrontAccountSharesHoldInfo> GetFrontAccountSharesHoldList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesHoldList(request);
        }

        /// <summary>
        /// 前端账户持仓卖出
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/hold/sell"), HttpPost]
        [Description("前端账户持仓卖出")]
        public object SellFrontAccountSharesHold(SellFrontAccountSharesHoldRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.SellFrontAccountSharesHold(request);
            return null;
        }

        /// <summary>
        /// 查询前端账户交易记录
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/trade/record/list"), HttpPost]
        [Description("查询前端账户交易记录")]
        public PageRes<FrontAccountSharesTradeRecordInfo> GetFrontAccountSharesTradeRecordList(GetFrontAccountSharesTradeRecordListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesTradeRecordList(request);
        }

        /// <summary>
        /// 前端账户交易撤单
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/trade/cancel"), HttpPost]
        [Description("前端账户交易撤单")]
        public object CancelFrontAccountSharesTrade(CancelFrontAccountSharesTradeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.CancelFrontAccountSharesTrade(request);
            return null;
        }

        /// <summary>
        /// 查询前端账户交易成交记录
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/trade/deal/record/list"), HttpPost]
        [Description("查询前端账户交易成交记录")]
        public PageRes<FrontAccountSharesTradeDealRecordInfo> GetFrontAccountSharesTradeDealRecordList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesTradeDealRecordList(request);
        }

        /// <summary>
        /// 查询前端账户盈亏统计
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/profit/statistics"), HttpPost]
        [Description("查询前端账户盈亏统计")]
        public PageRes<FrontAccountSharesProfitStatisticsInfo> GetFrontAccountSharesProfitStatistics(GetFrontAccountSharesProfitStatisticsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesProfitStatistics(request);
        }

        /// <summary>
        /// 查询前端账户盈亏记录
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/profit/record"), HttpPost]
        [Description("查询前端账户盈亏记录")]
        public PageRes<FrontAccountSharesProfitRecordInfo> GetFrontAccountSharesProfitRecord(GetFrontAccountSharesProfitRecordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesProfitRecord(request);
        }

        /// <summary>
        /// 查询保证金变更记录
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/deposit/change/record"), HttpPost]
        [Description("查询保证金变更记录")]
        public PageRes<FrontAccountSharesDepositChangeRecordInfo> GetFrontAccountSharesDepositChangeRecord(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesDepositChangeRecord(request);
        }

        /// <summary>
        /// 查询借款变更记录
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/fundamount/change/record"), HttpPost]
        [Description("查询借款变更记录")]
        public PageRes<FrontAccountSharesFundAmountChangeRecordInfo> GetFrontAccountSharesFundAmountChangeRecord(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesFundAmountChangeRecord(request);
        }

        /// <summary>
        /// 查询前端账户交易异常记录
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/trade/abnormal/record/list"), HttpPost]
        [Description("查询前端账户交易异常记录")]
        public PageRes<FrontAccountSharesTradeAbnormalRecordInfo> GetFrontAccountSharesTradeAbnormalRecordList(GetFrontAccountSharesTradeAbnormalRecordListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesTradeAbnormalRecordList(request);
        }

        /// <summary>
        /// 前端账户交易当日队列委托消费
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/trade/abnormal/queue/consume"), HttpPost]
        [Description("前端账户交易当日队列委托消费")]
        public object ConsumeFrontAccountSharesTradeQueue()
        {
            frontAccountHandler.ConsumeFrontAccountSharesTradeQueue();
            return null;
        }

        /// <summary>
        /// 查询前端账户交易异常账号委托记录
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/trade/abnormal/record/details"), HttpPost]
        [Description("查询前端账户交易账号委托记录")]
        public PageRes<FrontAccountSharesTradeAbnormalRecordDetails> GetFrontAccountSharesTradeAbnormalRecordDetails(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesTradeAbnormalRecordDetails(request);
        }

        /// <summary>
        /// 添加前端账户交易异常账号委托
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/trade/abnormal/record/details/add"), HttpPost]
        [Description("添加前端账户交易异常账号委托")]
        public object AddFrontAccountSharesTradeAbnormalRecordDetails(AddFrontAccountSharesTradeAbnormalRecordDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.AddFrontAccountSharesTradeAbnormalRecordDetails(request);
            return null;
        }

        /// <summary>
        /// 编辑前端账户交易异常账号委托
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/trade/abnormal/record/details/modify"), HttpPost]
        [Description("编辑前端账户交易异常账号委托")]
        public object ModifyFrontAccountSharesTradeAbnormalRecordDetails(ModifyFrontAccountSharesTradeAbnormalRecordDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountSharesTradeAbnormalRecordDetails(request);
            return null;
        }

        /// <summary>
        /// 删除前端账户交易异常账号委托
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/trade/abnormal/record/details/delete"), HttpPost]
        [Description("删除前端账户交易异常账号委托")]
        public object DeleteFrontAccountSharesTradeAbnormalRecordDetails(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.DeleteFrontAccountSharesTradeAbnormalRecordDetails(request);
            return null;
        }

        /// <summary>
        /// 结束异常交易
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/trade/abnormal/record/over"), HttpPost]
        [Description("结束异常交易")]
        public object OverFrontAccountSharesTradeAbnormalRecord(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.OverFrontAccountSharesTradeAbnormalRecord(request);
            return null;
        }

        /// <summary>
        /// 查询股票派股派息记录
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/allot/record"), HttpPost]
        [Description("查询股票派股派息记录")]
        public PageRes<FrontAccountSharesAllotRecordInfo> GetFrontAccountSharesAllotRecord(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesAllotRecord(request);
        }

        /// <summary>
        /// 添加股票派股派息
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/allot/add"), HttpPost]
        [Description("添加股票派股派息")]
        public object AddFrontAccountSharesAllot(AddFrontAccountSharesAllotRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.AddFrontAccountSharesAllot(request);
            return null;
        }

        /// <summary>
        /// 编辑股票派股派息
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/allot/modify"), HttpPost]
        [Description("编辑股票派股派息")]
        public object ModifyFrontAccountSharesAllot(ModifyFrontAccountSharesAllotRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.ModifyFrontAccountSharesAllot(request);
            return null;
        }

        /// <summary>
        /// 删除股票派股派息
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/allot/delete"), HttpPost]
        [Description("删除股票派股派息")]
        public object DeleteFrontAccountSharesAllot(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            frontAccountHandler.DeleteFrontAccountSharesAllot(request);
            return null;
        }

        /// <summary>
        /// 查询股票派股派息详情
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/allot/details"), HttpPost]
        [Description("查询股票派股派息详情")]
        public PageRes<FrontAccountSharesAllotDetails> GetFrontAccountSharesAllotDetails(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return frontAccountHandler.GetFrontAccountSharesAllotDetails(request);
        }
        #endregion
    }
}
