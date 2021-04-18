﻿using MealTicket_Web_APIService.Filter;
using MealTicket_Web_Handler;
using MealTicket_Web_Handler.Model;
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

namespace MealTicket_Web_APIService.controller
{
    [RoutePrefix("web")]
    public class TrendController:ApiController
    {
        private TrendHandler trendHandler;

        public TrendController()
        {
            trendHandler = WebApiManager.Kernel.Get<TrendHandler>();
        }

        /// <summary>
        /// 获取用户保证金余额
        /// </summary>
        /// <returns></returns>
        [Description("获取用户保证金余额")]
        [Route("account/wallet/info"), HttpPost]
        [CheckUserLoginFilter]
        public AccountWalletInfo AccountGetWalletInfo(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.AccountGetWalletInfo(request,basedata);
        }

        /// <summary>
        /// 设置用户保留余额
        /// </summary>
        /// <returns></returns>
        [Description("设置用户保留余额")]
        [Route("account/deposit/remain/set"), HttpPost]
        [CheckUserLoginFilter]
        public object SetAccountRemainDeposit(SetAccountRemainDepositRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.SetAccountRemainDeposit(request, basedata);
            return null;
        }

        /// <summary>
        /// 根据股票代码/名称/简拼获取股票列表
        /// </summary>
        /// <returns></returns>
        [Description("根据股票代码/名称/简拼获取股票列表")]
        [Route("shares/list"), HttpPost]
        public List<SharesInfo> GetSharesList(GetSharesListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return trendHandler.GetSharesList(request);
        }

        /// <summary>
        /// 用户登入
        /// </summary>
        /// <returns></returns>
        [Description("用户登入")]
        [Route("account/login"), HttpPost]
        public AccountLoginInfo AccountLogin(AccountLoginRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.AccountLogin(request, basedata);
        }

        /// <summary>
        /// 用户退出登入
        /// </summary>
        /// <returns></returns>
        [Description("用户退出登入")]
        [Route("account/logout"), HttpPost]
        public object AccountLogout()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AccountLogout(basedata);
            return null;
        }

        /// <summary>
        /// 查询自选股列表
        /// </summary>
        /// <returns></returns>
        [Description("查询自选股列表")]
        [Route("account/optional/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AccountOptionalInfo> GetAccountOptionalList(GetAccountOptionalListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountOptionalList(request, basedata);
        }

        /// <summary>
        /// 添加自选股
        /// </summary>
        /// <returns></returns>
        [Description("添加自选股")]
        [Route("account/optional/add"), HttpPost]
        [CheckUserLoginFilter]
        public object AddAccountOptional(AddAccountOptionalRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountOptional(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除自选股
        /// </summary>
        /// <returns></returns>
        [Description("删除自选股")]
        [Route("account/optional/delete"), HttpPost]
        [CheckUserLoginFilter]
        public object DeleteAccountOptional(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountOptional(request, basedata);
            return null;
        }

        /// <summary>
        /// 绑定自选股席位
        /// </summary>
        /// <returns></returns>
        [Description("绑定自选股席位")]
        [Route("account/optional/seat/bind"), HttpPost]
        [CheckUserLoginFilter]
        public object BindAccountOptionalSeat(BindAccountOptionalSeatRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BindAccountOptionalSeat(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询自选股监控走势
        /// </summary>
        /// <returns></returns>
        [Description("查询自选股监控走势")]
        [Route("account/optional/trend"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AccountOptionalTrendInfo> GetAccountOptionalTrend(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountOptionalTrend(request, basedata);
        }

        /// <summary>
        /// 修改自选股监控走势状态
        /// </summary>
        /// <returns></returns>
        [Description("修改自选股监控走势状态")]
        [Route("account/optional/trend/status/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountOptionalTrendStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountOptionalTrendStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/trend/par"), HttpPost]
        [Description("查询自选股走势参数")]
        [CheckUserLoginFilter]
        public PageRes<AccountOptionalTrendParInfo> GetAccountOptionalTrendPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountOptionalTrendPar(request, basedata);
        }

        /// <summary>
        /// 添加自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/trend/par/add"), HttpPost]
        [Description("添加自选股走势参数")]
        [CheckUserLoginFilter]
        public object AddAccountOptionalTrendPar(AddAccountOptionalTrendParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountOptionalTrendPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/trend/par/modify"), HttpPost]
        [Description("编辑走势模板参数")]
        [CheckUserLoginFilter]
        public object ModifyAccountOptionalTrendPar(ModifyAccountOptionalTrendParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountOptionalTrendPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/trend/par/delete"), HttpPost]
        [Description("删除自选股走势参数")]
        [CheckUserLoginFilter]
        public object DeleteAccountOptionalTrendPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountOptionalTrendPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询自选股走势再触发配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/trend/tri"), HttpPost]
        [Description("查询自选股走势再触发配置")]
        [CheckUserLoginFilter]
        public AccountOptionalTrendTriInfo GetAccountOptionalTrendTri(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountOptionalTrendTri(request, basedata);
        }

        /// <summary>
        /// 设置自选股走势再触发配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/trend/tri/modify"), HttpPost]
        [Description("设置自选股走势再触发配置")]
        [CheckUserLoginFilter]
        public object ModifyAccountOptionalTrendTri(ModifyAccountOptionalTrendTriRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountOptionalTrendTri(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询用户席位列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/seat/list"), HttpPost]
        [Description("查询用户席位列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountSeatInfo> GetAccountSeatList(GetAccountSeatListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountSeatList(request, basedata);
        }

        /// <summary>
        /// 编辑用户席位
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/seat/modify"), HttpPost]
        [Description("编辑用户席位")]
        [CheckUserLoginFilter]
        public object ModifyAccountSeat(ModifyAccountSeatRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountSeat(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除用户席位
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/seat/delete"), HttpPost]
        [Description("删除用户席位")]
        [CheckUserLoginFilter]
        public object DeleteAccountSeat(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountSeat(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询今日行情数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("shares/quotes/today"), HttpPost]
        [Description("查询今日行情数据")]
        [CheckUserLoginFilter]
        public GetSharesQuotesTodayRes GetSharesQuotesToday()
        {
            return trendHandler.GetSharesQuotesToday();
        }

        /// <summary>
        /// 查询监控股票分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/group"), HttpPost]
        [Description("查询监控股票分组列表")]
        [CheckUserLoginFilter]
        public List<AccountOptionalGroupInfo> GetAccountOptionalGroup()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountOptionalGroup(basedata);
        }

        /// <summary>
        /// 添加监控股票分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/group/add"), HttpPost]
        [Description("添加监控股票分组")]
        [CheckUserLoginFilter]
        public object AddAccountOptionalGroup(AddAccountOptionalGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountOptionalGroup(request,basedata);
            return null;
        }

        /// <summary>
        /// 编辑监控股票分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/group/modify"), HttpPost]
        [Description("编辑监控股票分组")]
        [CheckUserLoginFilter]
        public object ModifyAccountOptionalGroup(ModifyAccountOptionalGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountOptionalGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除监控股票分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/group/delete"), HttpPost]
        [Description("删除监控股票分组")]
        [CheckUserLoginFilter]
        public object DeleteAccountOptionalGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountOptionalGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/group/rel/list"), HttpPost]
        [Description("查询监控分组内股票")]
        [CheckUserLoginFilter]
        public PageRes<AccountOptionalGroupRelInfo> GetAccountOptionalGroupRelList(GetAccountOptionalGroupRelListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountOptionalGroupRelList(request,basedata);
        }

        /// <summary>
        /// 添加监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/group/rel/add"), HttpPost]
        [Description("添加监控分组内股票")]
        [CheckUserLoginFilter]
        public object AddAccountOptionalGroupRel(AddAccountOptionalGroupRelRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountOptionalGroupRel(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/group/rel/delete"), HttpPost]
        [Description("删除监控分组内股票")]
        [CheckUserLoginFilter]
        public object DeleteAccountOptionalGroupRel(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountOptionalGroupRel(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改监控分组内股票是否持续
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/group/rel/groupiscontinue/modify"), HttpPost]
        [Description("修改监控分组内股票是否持续")]
        [CheckUserLoginFilter]
        public object ModifyAccountOptionalGroupRelGroupIsContinue(ModifyAccountOptionalGroupRelGroupIsContinueRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountOptionalGroupRelGroupIsContinue(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量导入监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/group/rel/batchadd"), HttpPost]
        [Description("批量导入监控分组内股票")]
        [CheckUserLoginFilter]
        public async Task<object> BatchAddAccountOptionalGroupRel()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;

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
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
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
                        return trendHandler.BatchAddAccountOptionalGroupRel(GroupId,GroupIsContinue, ValidStartTime, ValidEndTime, sharesList, basedata);
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
        /// 查询今日触发股票数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/trend/tri/list"), HttpPost]
        [Description("查询今日触发股票数据")]
        [CheckUserLoginFilter]
        public List<AccountTrendTriInfo> GetAccountTrendTriList()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountTrendTriList(basedata);
        }

        /// <summary>
        /// 查询触发股票详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/trend/tri/details"), HttpPost]
        [Description("查询触发股票详情")]
        [CheckUserLoginFilter]
        public List<AccountTrendTriDetails> GetAccountTrendTriDetails(GetAccountTrendTriDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountTrendTriDetails(request,basedata);
        }

        /// <summary>
        /// 关闭已触发股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/trend/tri/close"), HttpPost]
        [Description("关闭已触发股票")]
        [CheckUserLoginFilter]
        public object CloseAccountTrendTri(DetailsRequest request)
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.CloseAccountTrendTri(request,basedata);
            return null;
        }

        /// <summary>
        /// 查询实时监控再触发配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/realtime/trend/tri"), HttpPost]
        [Description("查询实时监控再触发配置")]
        [CheckUserLoginFilter]
        public List<AccountOptionalTrendTriInfo> GetAccountRealTimeTrendTri(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountRealTimeTrendTri(request, basedata);
        }

        /// <summary>
        /// 设置实时监控再触发配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/realtime/trend/tri/modify"), HttpPost]
        [Description("设置实时监控再触发配置")]
        [CheckUserLoginFilter]
        public object ModifyAccountRealTimeTrendTri(ModifyAccountRealTimeTrendTriRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountRealTimeTrendTri(request, basedata);
            return null;
        }

        /// <summary>
        /// 设置监控分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/realtime/trend/tri/group/modify"), HttpPost]
        [Description("设置监控分组")]
        [CheckUserLoginFilter]
        public object ModifyAccountRealTimeTrendTriGroup(ModifyAccountRealTimeTrendTriGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountRealTimeTrendTriGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询监控记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/realtime/trend/record"), HttpPost]
        [Description("查询监控记录")]
        [CheckUserLoginFilter]
        public PageRes<AccountRealTimeTrendRecordInfo> GetAccountRealTimeTrendRecord(GetAccountRealTimeTrendRecordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountRealTimeTrendRecord(request, basedata);
        }

        /// <summary>
        /// 查询监控记录详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/realtime/trend/record/details"), HttpPost]
        [Description("查询监控记录详情")]
        [CheckUserLoginFilter]
        public PageRes<AccountRealTimeTrendRecordDetailsInfo> GetAccountRealTimeTrendRecordDetails(GetAccountRealTimeTrendRecordDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountRealTimeTrendRecordDetails(request, basedata);
        }

        /// <summary>
        /// 查询商品列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("goods/list"), HttpPost]
        [Description("查询商品列表")]
        [CheckUserLoginFilter]
        public List<GoodsInfo> GetGoodsList(GetGoodsListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return trendHandler.GetGoodsList(request);
        }

        /// <summary>
        /// 查询支付方式列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("payment/list"), HttpPost]
        [Description("查询支付方式列表")]
        [CheckUserLoginFilter]
        public List<PaymentInfo> GetPaymentList(GetPaymentListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return trendHandler.GetPaymentList(request);
        }

        /// <summary>
        /// 查询跟投人
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("follow/account/list"), HttpPost]
        [Description("查询跟投人")]
        [CheckUserLoginFilter]
        public List<FollowAccountInfo> GetFollowAccountList()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetFollowAccountList(basedata);
        }

        /// <summary>
        /// 查询用户交易账户信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/trade/info"), HttpPost]
        [Description("查询用户交易账户信息")]
        [CheckUserLoginFilter]
        public AccountTradeInfo GetAccountTradeInfo(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountTradeInfo(request,basedata);
        }

        /// <summary>
        /// 获取用户交易账户持仓列表
        /// </summary>
        /// <returns></returns>
        [Route("account/hold/list"), HttpPost]
        [Description("获取用户交易账户持仓列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountTradeHoldInfo> GetAccountTradeHoldList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountTradeHoldList(request, basedata);
        }

        /// <summary>
        /// 获取用户交易账户委托列表
        /// </summary>
        /// <returns></returns>
        [Route("account/entrust/list"), HttpPost]
        [Description("获取用户交易账户委托列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountTradeEntrustInfo> GetAccountTradeEntrustList(GetAccountTradeEntrustListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountTradeEntrustList(request, basedata);
        }

        /// <summary>
        /// 获取用户交易账户成交记录列表
        /// </summary>
        /// <returns></returns>
        [Route("account/entrust/deal/list"), HttpPost]
        [Description("获取用户交易账户成交记录列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountTradeEntrustDealInfo> GetAccountTradeEntrustDealList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountTradeEntrustDealList(request, basedata);
        }

        /// <summary>
        /// 根据股票代码获取股票列表
        /// </summary>
        /// <returns></returns>
        [Description("根据股票代码获取股票列表")]
        [Route("allshares/list"), HttpPost]
        public GetTradeSharesListRes GetTradeSharesList(GetTradeSharesListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (string.IsNullOrEmpty(request.SharesCode))
            {
                throw new WebApiException(400, "请输入股票代码");
            }
            return trendHandler.GetTradeSharesList(request);
        }

        /// <summary>
        /// 获取某只股票五档数据
        /// </summary>
        /// <returns></returns>
        [Route("shares/quotes"), HttpPost]
        [Description("获取某只股票五档数据")]
        [CheckUserLoginFilter]
        public TradeSharesQuotesInfo GetTradeSharesQuotesInfo(GetTradeSharesQuotesInfoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetTradeSharesQuotesInfo(request, basedata);
        }

        /// <summary>
        /// 股票申请买入
        /// </summary>
        /// <returns></returns>
        [Route("account/entrust/buy"), HttpPost]
        [Description("股票申请买入")]
        [CheckUserLoginFilter]
        public object AccountApplyTradeBuy(AccountApplyTradeBuyRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AccountApplyTradeBuy(request, basedata);
            return null;
        }

        /// <summary>
        /// 股票申请卖出
        /// </summary>
        /// <returns></returns>
        [Route("account/entrust/sell"), HttpPost]
        [Description("股票申请卖出")]
        [CheckUserLoginFilter]
        public object AccountApplyTradeSell(AccountApplyTradeSellRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AccountApplyTradeSell(request, basedata);
            return null;
        }

        /// <summary>
        /// 股票申请撤单
        /// </summary>
        /// <returns></returns>
        [Route("account/entrust/cancel"), HttpPost]
        [Description("股票申请撤单")]
        [CheckUserLoginFilter]
        public object AccountApplyTradeCancel(AccountApplyTradeCancelRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AccountApplyTradeCancel(request, basedata);
            return null;
        }

        /// <summary>
        /// 补交保证金
        /// </summary>
        /// <returns></returns>
        [Route("account/deposit/makeup"), HttpPost]
        [Description("补交保证金")]
        [CheckUserLoginFilter]
        public object AccountMakeupDeposit(AccountMakeupDepositRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AccountMakeupDeposit(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取跟投记录
        /// </summary>
        /// <returns></returns>
        [Route("account/follow/record"), HttpPost]
        [Description("获取跟投记录")]
        [CheckUserLoginFilter]
        public PageRes<AccountFollowRecordInfo> GetAccountFollowRecord(GetAccountFollowRecordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountFollowRecord(request, basedata);
        }

        /// <summary>
        /// 获取条件单列表
        /// </summary>
        /// <returns></returns>
        [Route("account/hold/conditiontrade/list"), HttpPost]
        [Description("获取条件单列表")]
        [CheckUserLoginFilter]
        public List<AccountHoldConditionTradeInfo> GetAccountHoldConditionTradeList(GetAccountHoldConditionTradeListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountHoldConditionTradeList(request);
        }

        /// <summary>
        /// 添加条件单
        /// </summary>
        /// <returns></returns>
        [Route("account/hold/conditiontrade/add"), HttpPost]
        [Description("添加条件单")]
        [CheckUserLoginFilter]
        public object AddAccountHoldConditionTrade(AddAccountHoldConditionTradeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (request.TradeType != 1 && request.TradeType != 2)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (request.Type != 1 && request.Type != 2 && request.Type != 3)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (request.Type == 1 && request.ConditionTime == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            if (request.Type != 1 && request.ConditionPrice == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountHoldConditionTrade(request);
            return null;
        }

        /// <summary>
        /// 复制条件单
        /// </summary>
        /// <returns></returns>
        [Route("account/hold/conditiontrade/copy"), HttpPost]
        [Description("复制条件单")]
        [CheckUserLoginFilter]
        public object CopyAccountHoldConditionTrade(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.CopyAccountHoldConditionTrade(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件单
        /// </summary>
        /// <returns></returns>
        [Route("account/hold/conditiontrade/modify"), HttpPost]
        [Description("编辑条件单")]
        [CheckUserLoginFilter]
        public object ModifyAccountHoldConditionTrade(ModifyAccountHoldConditionTradeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountHoldConditionTrade(request);
            return null;
        }

        /// <summary>
        /// 修改条件单状态
        /// </summary>
        /// <returns></returns>
        [Route("account/hold/conditiontrade/status/modify"), HttpPost]
        [Description("修改条件单状态")]
        [CheckUserLoginFilter]
        public object ModifyAccountHoldConditionTradeStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountHoldConditionTradeStatus(request);
            return null;
        }

        /// <summary>
        /// 删除条件单
        /// </summary>
        /// <returns></returns>
        [Route("account/hold/conditiontrade/delete"), HttpPost]
        [Description("删除条件单")]
        [CheckUserLoginFilter]
        public object DeleteAccountHoldConditionTrade(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountHoldConditionTrade(request);
            return null;
        }

        /// <summary>
        /// 获取买入条件单股票列表
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/shares/list"), HttpPost]
        [Description("获取买入条件单股票列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionTradeSharesInfo> GetAccountBuyConditionTradeSharesList(GetAccountBuyConditionTradeSharesListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionTradeSharesList(request, basedata);
        }

        /// <summary>
        /// 添加买入条件单股票
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/shares/add"), HttpPost]
        [Description("添加买入条件单股票")]
        [CheckUserLoginFilter]
        public object AddAccountBuyConditionTradeShares(AddAccountBuyConditionTradeSharesRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuyConditionTradeShares(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量导入条件买入股票数据
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/conditiontrade/buy/shares/batch/add"), HttpPost]
        [Description("批量导入条件买入股票数据")]
        public async Task<object> BatchAddAccountBuyConditionTradeShares()
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
                        var accountId = long.Parse(provider.FormData["accountId"]);
                        var groupId = long.Parse(provider.FormData["groupId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<SharesInfo> marketTimeList = new List<SharesInfo>();
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

                            marketTimeList.Add(new SharesInfo
                            {
                                Market = stock[1] == "SZ" ? 0 : 1,
                                SharesCode = int.Parse(stock[0]).ToString("000000"),
                                SharesName = datas[1]
                            });
                        }
                        HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
                        if (accountId == 0)
                        {
                            accountId = basedata.AccountId;
                        }
                        return trendHandler.BatchAddAccountBuyConditionTradeShares(marketTimeList,accountId, new List<long> { groupId });
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
        /// 修改条件买入股票状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/shares/status/modify"), HttpPost]
        [Description("修改条件买入股票状态")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionTradeSharesStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionTradeSharesStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除买入条件单股票
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/shares/delete"), HttpPost]
        [Description("删除买入条件单股票")]
        [CheckUserLoginFilter]
        public object DeleteAccountBuyConditionTradeShares(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            trendHandler.DeleteAccountBuyConditionTradeShares(request);
            return null;
        }

        /// <summary>
        /// 获取股票板块列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("shares/plate/list"), HttpPost]
        [Description("获取股票板块列表")]
        [CheckUserLoginFilter]
        public PageRes<SharesPlateInfo> GetSharesPlateList(GetSharesPlateListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetSharesPlateList(request,basedata);
        }

        /// <summary>
        /// 从系统导入板块股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("sys/plate/shares/import"), HttpPost]
        [Description("从系统导入板块股票")]
        [CheckUserLoginFilter]
        public object ImportysPlateShares(ImportysPlateSharesRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.ImportysPlateShares(request, basedata);
        }

        /// <summary>
        /// 批量修改股票板块股票状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/shares/plate/status/batch/modify"), HttpPost]
        [Description("批量修改股票板块股票状态")]
        [CheckUserLoginFilter]
        public object BatchModifyAccountSharesPlateStatus(BatchModifyAccountSharesPlateStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.BatchModifyAccountSharesPlateStatus(request, basedata);
        }

        /// <summary>
        /// 获取用户板块股票列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/shares/plate/shares/list"), HttpPost]
        [Description("获取用户板块股票列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountSharesPlateSharesInfo> GetAccountSharesPlateSharesList(GetAccountSharesPlateSharesListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountSharesPlateSharesList(request, basedata);
        }

        /// <summary>
        /// 获取用户板块所有股票列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/shares/plate/sharesall/list"), HttpPost]
        [Description("获取用户板块所有股票列表")]
        [CheckUserLoginFilter]
        public List<SharesInfo> GetAccountSharesPlateSharesAllList(GetAccountSharesPlateSharesListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountSharesPlateSharesAllList(request, basedata);
        }


        /// <summary>
        /// 获取股票买入条件列表
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/list"), HttpPost]
        [Description("获取股票买入条件列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionInfo> GetAccountBuyConditionList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionList(request, basedata);
        }

        /// <summary>
        /// 添加股票买入条件
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/add"), HttpPost]
        [Description("添加股票买入条件")]
        [CheckUserLoginFilter]
        public object AddAccountBuyCondition(AddAccountBuyConditionRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuyCondition(request, basedata);
            return null;
        }

        /// <summary>
        /// 复制股票买入条件
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/copy"), HttpPost]
        [Description("复制股票买入条件")]
        [CheckUserLoginFilter]
        public object CopyAccountBuyCondition(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.CopyAccountBuyCondition(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑股票买入条件
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/modify"), HttpPost]
        [Description("编辑股票买入条件")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyCondition(ModifyAccountBuyConditionRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyCondition(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改股票买入条件状态
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/status/modify"), HttpPost]
        [Description("修改股票买入条件状态")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除股票买入条件
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/delete"), HttpPost]
        [Description("删除股票买入条件")]
        [CheckUserLoginFilter]
        public object DeleteAccountBuyCondition(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountBuyCondition(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取股票买入额外条件分组列表
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/group/list"), HttpPost]
        [Description("获取股票买入额外条件分组列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherGroupInfo> GetAccountBuyConditionOtherGroupList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionOtherGroupList(request, basedata);
        }

        /// <summary>
        /// 添加股票买入额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/group/add"), HttpPost]
        [Description("添加股票买入额外条件分组")]
        [CheckUserLoginFilter]
        public object AddAccountBuyConditionOtherGroup(AddAccountBuyConditionOtherGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuyConditionOtherGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑股票买入额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/group/modify"), HttpPost]
        [Description("编辑股票买入额外条件分组")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionOtherGroup(ModifyAccountBuyConditionOtherGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionOtherGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改股票买入额外条件分组状态
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/group/status/modify"), HttpPost]
        [Description("修改股票买入额外条件分组状态")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionOtherGroupStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionOtherGroupStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除股票买入额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/group/delete"), HttpPost]
        [Description("删除股票买入额外条件分组")]
        [CheckUserLoginFilter]
        public object DeleteAccountBuyConditionOtherGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountBuyConditionOtherGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取股票买入转自动条件分组列表
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/group/list"), HttpPost]
        [Description("获取股票买入转自动条件分组列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionAutoGroupInfo> GetAccountBuyConditionAutoGroupList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionAutoGroupList(request, basedata);
        }

        /// <summary>
        /// 添加股票买入转自动条件分组
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/group/add"), HttpPost]
        [Description("添加股票买入转自动条件分组")]
        [CheckUserLoginFilter]
        public object AddAccountBuyConditionAutoGroup(AddAccountBuyConditionAutoGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuyConditionAutoGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑股票买入转自动条件分组
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/group/modify"), HttpPost]
        [Description("编辑股票买入转自动条件分组")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionAutoGroup(ModifyAccountBuyConditionAutoGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionAutoGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改股票买入转自动条件分组状态
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/group/status/modify"), HttpPost]
        [Description("修改股票买入转自动条件分组状态")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionAutoGroupStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionAutoGroupStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除股票买入转自动条件分组
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/group/delete"), HttpPost]
        [Description("删除股票买入转自动条件分组")]
        [CheckUserLoginFilter]
        public object DeleteAccountBuyConditionAutoGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountBuyConditionAutoGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取股票买入额外条件列表
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/list"), HttpPost]
        [Description("获取股票买入额外条件列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherInfo> GetAccountBuyConditionOtherList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionOtherList(request, basedata);
        }

        /// <summary>
        /// 添加股票买入额外条件
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/add"), HttpPost]
        [Description("添加股票买入额外条件")]
        [CheckUserLoginFilter]
        public object AddAccountBuyConditionOther(AddAccountBuyConditionOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuyConditionOther(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改股票买入额外条件状态
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/status/modify"), HttpPost]
        [Description("修改股票买入额外条件状态")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionOtherStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionOtherStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除股票买入额外条件
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/delete"), HttpPost]
        [Description("删除股票买入额外条件")]
        [CheckUserLoginFilter]
        public object DeleteAccountBuyConditionOther(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountBuyConditionOther(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取股票买入转自动条件列表
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/list"), HttpPost]
        [Description("获取股票买入转自动条件列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionAutoInfo> GetAccountBuyConditionAutoList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionAutoList(request, basedata);
        }

        /// <summary>
        /// 添加股票买入转自动条件
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/add"), HttpPost]
        [Description("添加股票买入转自动条件")]
        [CheckUserLoginFilter]
        public object AddAccountBuyConditionAuto(AddAccountBuyConditionAutoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuyConditionAuto(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改股票买入转自动条件状态
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/status/modify"), HttpPost]
        [Description("修改股票买入转自动条件状态")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionAutoStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionAutoStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除股票买入转自动条件
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/delete"), HttpPost]
        [Description("删除股票买入转自动条件")]
        [CheckUserLoginFilter]
        public object DeleteAccountBuyConditionAuto(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountBuyConditionAuto(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询股票买入额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/par"), HttpPost]
        [Description("查询股票买入额外条件类型参数")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherParInfo> GetAccountBuyConditionOtherPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionOtherPar(request, basedata);
        }

        /// <summary>
        /// 添加股票买入额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/par/add"), HttpPost]
        [Description("添加股票买入额外条件类型参数")]
        [CheckUserLoginFilter]
        public object AddAccountBuyConditionOtherPar(AddAccountBuyConditionOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuyConditionOtherPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑股票买入额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/par/modify"), HttpPost]
        [Description("编辑股票买入额外条件类型参数")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionOtherPar(ModifyAccountBuyConditionOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionOtherPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除股票买入额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/par/delete"), HttpPost]
        [Description("删除股票买入额外条件类型参数")]
        [CheckUserLoginFilter]
        public object DeleteAccountBuyConditionOtherPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountBuyConditionOtherPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询股票买入转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/par"), HttpPost]
        [Description("查询股票买入转自动条件类型参数")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionAutoParInfo> GetAccountBuyConditionAutoPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionAutoPar(request, basedata);
        }

        /// <summary>
        /// 添加股票买入转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/par/add"), HttpPost]
        [Description("添加股票买入转自动条件类型参数")]
        [CheckUserLoginFilter]
        public object AddAccountBuyConditionAutoPar(AddAccountBuyConditionAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuyConditionAutoPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑股票买入转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/par/modify"), HttpPost]
        [Description("编辑股票买入转自动条件类型参数")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionAutoPar(ModifyAccountBuyConditionAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionAutoPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除股票买入转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/par/delete"), HttpPost]
        [Description("删除股票买入转自动条件类型参数")]
        [CheckUserLoginFilter]
        public object DeleteAccountBuyConditionAutoPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountBuyConditionAutoPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询买入提示列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("buy/tip/list"), HttpPost]
        [Description("查询买入提示列表")]
        [CheckUserLoginFilter]
        public List<BuyTipInfo> GetBuyTipList()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetBuyTipList(basedata);
        }

        /// <summary>
        /// 确认买入提示
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("buy/tip/confirm"), HttpPost]
        [Description("确认买入提示")]
        [CheckUserLoginFilter]
        public object ConfirmBuyTip(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ConfirmBuyTip(request,basedata);
            return null;
        }

        /// <summary>
        /// 获取条件模板列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/list"), HttpPost]
        [Description("获取条件模板列表")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateInfo> GetConditiontradeTemplateList(GetConditiontradeTemplateListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateList(request,basedata);
        }

        /// <summary>
        /// 添加条件模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/add"), HttpPost]
        [Description("添加条件模板")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplate(AddConditiontradeTemplateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplate(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/modify"), HttpPost]
        [Description("编辑条件模板")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplate(ModifyConditiontradeTemplateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplate(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件模板状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/status/modify"), HttpPost]
        [Description("修改条件模板状态")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/delete"), HttpPost]
        [Description("删除条件模板")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplate(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplate(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取条件卖出模板详情列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/sell/details/list"), HttpPost]
        [Description("获取条件卖出模板详情列表")]
        [CheckUserLoginFilter]
        public List<ConditiontradeTemplateSellDetailsInfo> GetConditiontradeTemplateSellDetailsList(GetConditiontradeTemplateSellDetailsListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateSellDetailsList(request, basedata);
        }

        /// <summary>
        /// 添加条件卖出模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/sell/details/add"), HttpPost]
        [Description("添加条件卖出模板详情")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateSellDetails(AddConditiontradeTemplateSellDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateSellDetails(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件卖出模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/sell/details/modify"), HttpPost]
        [Description("编辑条件卖出模板详情")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateSellDetails(ModifyConditiontradeTemplateSellDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateSellDetails(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件卖出模板状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/sell/details/status/modify"), HttpPost]
        [Description("修改条件卖出模板状态")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateSellDetailsStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateSellDetailsStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件卖出模板
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/sell/details/delete"), HttpPost]
        [Description("删除条件卖出模板")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateSellDetails(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateSellDetails(request, basedata);
            return null;
        }

        /// <summary>
        /// 卖出模板导入
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/sell/import"), HttpPost]
        [Description("卖出模板导入")]
        [CheckUserLoginFilter]
        public object ImportConditiontradeTemplateSell(ImportConditiontradeTemplateSellRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ImportConditiontradeTemplateSell(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量删除条件买入股票
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/buy/shares/delete/batch"), HttpPost]
        [Description("批量删除条件买入股票")]
        [CheckUserLoginFilter]
        public object BatchDeleteConditiontradeBuyShares(BatchDeleteConditiontradeBuySharesRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteConditiontradeBuyShares(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取条件买入模板详情列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/details/list"), HttpPost]
        [Description("获取条件买入模板详情列表")]
        public PageRes<ConditiontradeTemplateBuyDetailsInfo> GetConditiontradeTemplateBuyDetailsList(GetConditiontradeTemplateBuyDetailsListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyDetailsList(request, basedata);
        }

        /// <summary>
        /// 获取条件买入系统模板详情列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/sys/template/buy/details/list"), HttpPost]
        [Description("获取条件买入系统模板详情列表")]
        public PageRes<ConditiontradeTemplateBuyDetailsInfo> GetConditiontradeSysTemplateBuyDetailsList(GetConditiontradeTemplateBuyDetailsListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeSysTemplateBuyDetailsList(request, basedata);
        }

        /// <summary>
        /// 添加条件买入模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/details/add"), HttpPost]
        [Description("添加条件买入模板详情")]
        public object AddConditiontradeTemplateBuyDetails(AddConditiontradeTemplateBuyDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateBuyDetails(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/details/modify"), HttpPost]
        [Description("编辑条件买入模板详情")]
        public object ModifyConditiontradeTemplateBuyDetails(ModifyConditiontradeTemplateBuyDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyDetails(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板详情状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/details/status/modify"), HttpPost]
        [Description("修改条件买入模板详情状态")]
        public object ModifyConditiontradeTemplateBuyDetailsStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyDetailsStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/details/delete"), HttpPost]
        [Description("删除条件买入模板详情")]
        public object DeleteConditiontradeTemplateBuyDetails(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateBuyDetails(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取条件买入模板额外条件分组列表
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/group/list"), HttpPost]
        [Description("获取条件买入模板额外条件分组列表")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherGroupInfo> GetConditiontradeTemplateBuyOtherGroupList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyOtherGroupList(request, basedata);
        }

        /// <summary>
        /// 添加条件买入模板额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/group/add"), HttpPost]
        [Description("添加条件买入模板额外条件分组")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateBuyOtherGroup(AddConditiontradeTemplateBuyOtherGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateBuyOtherGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/group/modify"), HttpPost]
        [Description("编辑股票买入额外条件分组")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyOtherGroup(ModifyConditiontradeTemplateBuyOtherGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyOtherGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板额外条件分组状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/group/status/modify"), HttpPost]
        [Description("修改条件买入模板额外条件分组状态")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyOtherGroupStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyOtherGroupStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/group/delete"), HttpPost]
        [Description("删除条件买入模板额外条件分组")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateBuyOtherGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateBuyOtherGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取条件买入模板转自动条件分组列表
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/group/list"), HttpPost]
        [Description("获取条件买入模板转自动条件分组列表")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyAutoGroupInfo> GetConditiontradeTemplateBuyAutoGroupList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyAutoGroupList(request, basedata);
        }

        /// <summary>
        /// 添加条件买入模板转自动条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/group/add"), HttpPost]
        [Description("添加条件买入模板转自动条件分组")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateBuyAutoGroup(AddConditiontradeTemplateBuyAutoGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateBuyAutoGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板转自动条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/group/modify"), HttpPost]
        [Description("编辑条件买入模板转自动条件分组")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyAutoGroup(ModifyConditiontradeTemplateBuyAutoGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyAutoGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板转自动条件分组状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/group/status/modify"), HttpPost]
        [Description("修改条件买入模板转自动条件分组状态")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyAutoGroupStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyAutoGroupStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板转自动条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/group/delete"), HttpPost]
        [Description("删除条件买入模板转自动条件分组")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateBuyAutoGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateBuyAutoGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取条件买入模板额外条件列表
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/list"), HttpPost]
        [Description("获取条件买入模板额外条件列表")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherInfo> GetConditiontradeTemplateBuyOtherList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyOtherList(request, basedata);
        }

        /// <summary>
        /// 添加条件买入模板额外条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/add"), HttpPost]
        [Description("添加条件买入模板额外条件")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateBuyOther(AddConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateBuyOther(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板额外条件状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/status/modify"), HttpPost]
        [Description("修改条件买入模板额外条件状态")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyOtherStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyOtherStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板额外条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/delete"), HttpPost]
        [Description("删除条件买入模板额外条件")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateBuyOther(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateBuyOther(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取条件买入模板转自动条件列表
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/list"), HttpPost]
        [Description("获取条件买入模板转自动条件列表")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyAutoInfo> GetConditiontradeTemplateBuyAutoList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyAutoList(request, basedata);
        }

        /// <summary>
        /// 添加条件买入模板转自动条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/add"), HttpPost]
        [Description("添加条件买入模板转自动条件")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateBuyAuto(AddConditiontradeTemplateBuyAutoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateBuyAuto(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板转自动条件状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/status/modify"), HttpPost]
        [Description("修改条件买入模板转自动条件状态")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyAutoStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyAutoStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板转自动条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/delete"), HttpPost]
        [Description("删除条件买入模板转自动条件")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateBuyAuto(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateBuyAuto(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par"), HttpPost]
        [Description("查询条件买入模板额外条件类型参数")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateBuyOtherPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyOtherPar(request, basedata);
        }

        /// <summary>
        /// 添加条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/add"), HttpPost]
        [Description("添加条件买入模板额外条件类型参数")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateBuyOtherPar(AddConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateBuyOtherPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/modify"), HttpPost]
        [Description("编辑条件买入模板额外条件类型参数")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyOtherPar(ModifyConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyOtherPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/delete"), HttpPost]
        [Description("删除条件买入模板额外条件类型参数")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateBuyOtherPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateBuyOtherPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par"), HttpPost]
        [Description("查询条件买入模板转自动条件类型参数")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyAutoParInfo> GetConditiontradeTemplateBuyAutoPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyAutoPar(request, basedata);
        }

        /// <summary>
        /// 添加条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/add"), HttpPost]
        [Description("添加条件买入模板转自动条件类型参数")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateBuyAutoPar(AddConditiontradeTemplateBuyAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateBuyAutoPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/modify"), HttpPost]
        [Description("编辑条件买入模板转自动条件类型参数")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyAutoPar(ModifyConditiontradeTemplateBuyAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyAutoPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/delete"), HttpPost]
        [Description("删除条件买入模板转自动条件类型参数")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateBuyAutoPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateBuyAutoPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 买入模板导入
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/import"), HttpPost]
        [Description("买入模板导入")]
        [CheckUserLoginFilter]
        public object ImportConditiontradeTemplateBuy(ImportConditiontradeTemplateBuyRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ImportConditiontradeTemplateBuy(request, basedata);
            return null;
        }

        /// <summary>
        /// 买入共享股票导入
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/buy/share/import"), HttpPost]
        [Description("买入共享股票导入")]
        [CheckUserLoginFilter]
        public object ImportConditiontradeBuyShare(ImportConditiontradeBuyShareRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ImportConditiontradeBuyShare(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询条件买入自定义分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/list"), HttpPost]
        [Description("查询条件买入自定义分组列表")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeBuyGroupInfo> GetConditiontradeBuyGroupList(GetConditiontradeBuyGroupListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeBuyGroupList(request, basedata);
        }

        /// <summary>
        /// 添加条件买入自定义分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/add"), HttpPost]
        [Description("添加条件买入自定义分组")]
        [CheckUserLoginFilter]
        public object AddConditiontradeBuyGroup(AddConditiontradeBuyGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeBuyGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件买入自定义分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/modify"), HttpPost]
        [Description("编辑条件买入自定义分组")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeBuyGroup(ModifyConditiontradeBuyGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeBuyGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入自定义分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/delete"), HttpPost]
        [Description("删除条件买入自定义分组")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeBuyGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeBuyGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 添加条件买入自定义分组股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/shares/add"), HttpPost]
        [Description("添加条件买入自定义分组股票")]
        [CheckUserLoginFilter]
        public object AddConditiontradeBuyGroupShares(AddConditiontradeBuyGroupSharesRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeBuyGroupShares(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件买入股票自定义分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/shares/mygroup/modify"), HttpPost]
        [Description("修改条件买入股票自定义分组")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeBuySharesMyGroup(ModifyConditiontradeBuySharesMyGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeBuySharesMyGroup(request, basedata);
            return null;
        }


        /// <summary>
        /// 批量添加条件买入自定义分组股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/shares/batch/add"), HttpPost]
        [Description("批量添加条件买入自定义分组股票")]
        [CheckUserLoginFilter]
        public async Task<object> BatchAddConditiontradeBuyGroupShares()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;

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
                        long GroupId = long.Parse(provider.FormData["GroupId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
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
                        return trendHandler.BatchAddConditiontradeBuyGroupShares(GroupId,sharesList, basedata);
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
        /// 删除条件买入自定义分组股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/shares/delete"), HttpPost]
        [Description("删除条件买入自定义分组股票")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeBuyGroupShares(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeBuyGroupShares(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询交易时间板块数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("tradetime/market/list"), HttpPost]
        [Description("查询交易时间板块数据")]
        [CheckUserLoginFilter]
        public List<TradeTimeMarketInfo> GetTradeTimeMarketList()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetTradeTimeMarketList(basedata);
        }

        /// <summary>
        /// 查询条件买入自定义分组共享用户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/shareaccount/list"), HttpPost]
        [Description("查询条件买入自定义分组共享用户列表")]
        [CheckUserLoginFilter]
        public List<ConditiontradeBuyGroupSharesAccount> GetConditiontradeBuyGroupSharesAccountList(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeBuyGroupSharesAccountList(request, basedata);
        }

        /// <summary>
        /// 添加条件买入自定义分组共享用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/shareaccount/add"), HttpPost]
        [Description("添加条件买入自定义分组共享用户")]
        [CheckUserLoginFilter]
        public object AddConditiontradeBuyGroupSharesAccount(AddConditiontradeBuyGroupSharesAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeBuyGroupSharesAccount(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入自定义分组共享用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/shareaccount/delete"), HttpPost]
        [Description("删除条件买入自定义分组共享用户")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeBuyGroupSharesAccount(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeBuyGroupSharesAccount(request, basedata);
            return null;
        }
    }
}
