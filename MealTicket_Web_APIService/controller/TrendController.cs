using MealTicket_Web_APIService.Filter;
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
        /// 根据股票代码/名称/简拼获取股票列表
        /// </summary>
        /// <returns></returns>
        [Description("根据股票代码/名称/简拼获取股票列表")]
        [Route("shares/list"), HttpPost]
        [CheckUserLoginFilter]
        public List<SharesInfo> GetSharesList(GetSharesListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetSharesList(request,basedata);
        }

        /// <summary>
        /// 获取股票涨跌幅
        /// </summary>
        /// <returns></returns>
        [Description("获取股票涨跌幅")]
        [Route("shares/list/quotes"), HttpPost]
        [CheckUserLoginFilter]
        public List<SharesListQuotesInfo> GetSharesListQuotes(List<SharesListQuotesInfo> request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetSharesListQuotes(request, basedata);
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
        /// 批量导入自选股列表
        /// </summary>
        /// <returns></returns>
        [Route("account/optional/batch/add"), HttpPost]
        [Description("批量导入自选股列表")]
        [CheckUserLoginFilter]
        public async Task<object> BatchAddAccountOptional()
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
                        HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
                        return trendHandler.BatchAddAccountOptional(basedata.AccountId, sharesList);
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
        /// 查询自选股走势系统参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/optional/trend/sys/par"), HttpPost]
        [Description("查询自选股走势系统参数")]
        [CheckUserLoginFilter]
        public PageRes<AccountOptionalTrendParInfo> GetAccountOptionalTrendSysPar(GetAccountOptionalTrendSysParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountOptionalTrendSysPar(request, basedata);
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
        public List<AccountTrendTriInfo> GetAccountTrendTriList(GetAccountTrendTriListRequest request)
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountTrendTriList(request,basedata);
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
        /// 查询今日触发涨跌停股票数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/riselimit/tri/list"), HttpPost]
        [Description("查询今日触发涨跌停股票数据")]
        [CheckUserLoginFilter]
        public List<AccountRiseLimitTriInfo> GetAccountRiseLimitTriList(GetAccountRiseLimitTriListRequest request)
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountRiseLimitTriList(request,basedata);
        }

        /// <summary>
        /// 查询今日触发涨跌停股票数据详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/riselimit/tri/details"), HttpPost]
        [Description("查询今日触发涨跌停股票数据详情")]
        [CheckUserLoginFilter]
        public List<AccountRiseLimitTriDetails> GetAccountRiseLimitTriDetails(GetAccountTrendTriDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountRiseLimitTriDetails(request, basedata);
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
        /// 查询监控记录-涨跌停
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/realtime/riselimit/record"), HttpPost]
        [Description("查询监控记录-涨跌停")]
        [CheckUserLoginFilter]
        public PageRes<AccountRealTimeRiselimitRecordInfo> GetAccountRealTimeRiselimitRecord(GetAccountRealTimeRiselimitRecordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountRealTimeRiselimitRecord(request, basedata);
        }

        /// <summary>
        /// 查询监控记录详情-涨跌停
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/realtime/riselimit/record/details"), HttpPost]
        [Description("查询监控记录详情")]
        [CheckUserLoginFilter]
        public PageRes<AccountRealTimeRiselimitRecordDetailsInfo> GetAccountRealTimeRiselimitRecordDetails(GetAccountRealTimeRiselimitRecordDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountRealTimeRiselimitRecordDetails(request, basedata);
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
        public List<FollowAccountInfo> GetFollowAccountList(GetFollowAccountListRequest request)
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetFollowAccountList(request,basedata);
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
            trendHandler.AddAccountHoldConditionTrade(request, basedata);
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
        public object GetAccountBuyConditionTradeSharesList(GetAccountBuyConditionTradeSharesListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionTradeSharesList(request, basedata);
        }

        /// <summary>
        /// 获取系统买入条件单股票列表
        /// </summary>
        /// <returns></returns>
        [Route("sys/conditiontrade/buy/shares/list"), HttpPost]
        [Description("获取系统买入条件单股票列表")]
        [CheckUserLoginFilter]
        public object GetSysBuyConditionTradeSharesList(GetAccountBuyConditionTradeSharesListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetSysBuyConditionTradeSharesList(request, basedata);
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
                            int _market = stock[1] == "SZ" ? 0 : 1;
                            string _sharesCode = int.Parse(stock[0]).ToString("000000");
                            string _sharesName = datas[1];
                            if (marketTimeList.Where(e => e.Market == _market && e.SharesCode == _sharesCode).FirstOrDefault()!=null)
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
        /// 添加买入板块设置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("condition/buy/plate/add"), HttpPost]
        [Description("添加买入板块设置")]
        [CheckUserLoginFilter]
        public object AddConditionBuyPlate(AddConditionBuyPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditionBuyPlate(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量添加买入板块设置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("condition/buy/plate/add/batch"), HttpPost]
        [Description("批量添加买入板块设置")]
        [CheckUserLoginFilter]
        public object BatchAddConditionBuyPlate(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchAddConditionBuyPlate(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除买入板块设置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("condition/buy/plate/delete"), HttpPost]
        [Description("删除买入板块设置")]
        [CheckUserLoginFilter]
        public object DeleteConditionBuyPlate(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditionBuyPlate(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量删除买入板块设置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("condition/buy/plate/delete/batch"), HttpPost]
        [Description("批量删除买入板块设置")]
        [CheckUserLoginFilter]
        public object BatchDeleteConditionBuyPlate(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteConditionBuyPlate(request, basedata);
            return null;
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
        /// 编辑股票买入额外条件
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/modify"), HttpPost]
        [Description("编辑股票买入额外条件")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionOther(ModifyAccountBuyConditionOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionOther(request, basedata);
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
        /// 编辑股票买入转自动条件
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/modify"), HttpPost]
        [Description("编辑股票买入转自动条件")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionAuto(ModifyAccountBuyConditionAutoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionAuto(request, basedata);
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
        /// 查询股票买入额外条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/par/plate"), HttpPost]
        [Description("查询股票买入额外条件类型参数(板块涨跌幅)")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherParInfo> GetAccountBuyConditionOtherParPlate(GetAccountBuyConditionOtherParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionOtherParPlate(request, basedata);
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
        /// 批量添加股票买入额外条件类型参数(板块涨跌幅1)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/conditiontrade/buy/other/par/add/batch"), HttpPost]
        [Description("批量添加股票买入额外条件类型参数(板块涨跌幅1)")]
        public async Task<object> BatchAddAccountBuyConditionOtherPar()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');

                            list.Add(datas[0].Trim());
                        }
                        return trendHandler.BatchAddAccountBuyConditionOtherPar(Type, RelId, list);
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
        /// 批量添加股票买入额外条件类型参数(板块涨跌幅2)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/conditiontrade/buy/other/par/add/batch2"), HttpPost]
        [Description("批量添加股票买入额外条件类型参数(板块涨跌幅2)")]
        public async Task<object> BatchAddAccountBuyConditionOtherPar2()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<BatchAddSharesConditionTrendPar2Obj> list = new List<BatchAddSharesConditionTrendPar2Obj>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 3)
                            {
                                continue;
                            }
                            string groupName = datas[0].Trim();
                            int compare = datas[1].Trim() == ">=" ? 1 : 2;
                            string rate = datas[2].Trim();

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName = groupName,
                                Compare = compare,
                                Rate = rate
                            });
                        }
                        return trendHandler.BatchAddAccountBuyConditionOtherPar2(Type, RelId, list);
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
        /// 批量删除股票买入额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/par/delete/batch"), HttpPost]
        [Description("批量删除股票买入额外条件类型参数")]
        [CheckUserLoginFilter]
        public object BatchDeleteAccountBuyConditionOtherPar(BatchDeleteAccountBuyConditionOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteAccountBuyConditionOtherPar(request, basedata);
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
        /// 查询股票买入转自动条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/par/plate"), HttpPost]
        [Description("查询股票买入转自动条件类型参数(板块涨跌幅)")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionAutoParInfo> GetAccountBuyConditionAutoParPlate(GetAccountBuyConditionAutoParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionAutoParPlate(request, basedata);
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
        /// 批量添加股票买入转自动条件类型参数(板块涨跌幅1)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/conditiontrade/buy/auto/par/add/batch"), HttpPost]
        [Description("批量添加股票买入转自动条件类型参数(板块涨跌幅1)")]
        public async Task<object> BatchAddAccountBuyConditionAutoPar()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');

                            list.Add(datas[0].Trim());
                        }
                        return trendHandler.BatchAddAccountBuyConditionAutoPar(Type, RelId, list);
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
        /// 批量添加股票买入转自动条件类型参数(板块涨跌幅2)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/conditiontrade/buy/auto/par/add/batch2"), HttpPost]
        [Description("批量添加股票买入转自动条件类型参数(板块涨跌幅2)")]
        public async Task<object> BatchAddAccountBuyConditionAutoPar2()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<BatchAddSharesConditionTrendPar2Obj> list = new List<BatchAddSharesConditionTrendPar2Obj>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 3)
                            {
                                continue;
                            }
                            string groupName = datas[0].Trim();
                            int compare = datas[1].Trim() == ">=" ? 1 : 2;
                            string rate = datas[2].Trim();

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName = groupName,
                                Compare = compare,
                                Rate = rate
                            });
                        }
                        return trendHandler.BatchAddAccountBuyConditionAutoPar2(Type, RelId, list);
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
        /// 批量删除股票买入转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/par/delete/batch"), HttpPost]
        [Description("批量删除股票买入转自动条件类型参数")]
        [CheckUserLoginFilter]
        public object BatchDeleteAccountBuyConditionAutoPar(BatchDeleteAccountBuyConditionAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteAccountBuyConditionAutoPar(request, basedata);
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

        #region==================额外关系====================
        /// <summary>
        /// 获取股票买入额外条件列表-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/list/other"), HttpPost]
        [Description("获取股票买入额外条件列表-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherInfo> GetAccountBuyConditionOtherList_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionOtherList_Other(request, basedata);
        }

        /// <summary>
        /// 添加股票买入额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/add/other"), HttpPost]
        [Description("添加股票买入额外条件-额外关系")]
        [CheckUserLoginFilter]
        public object AddAccountBuyConditionOther_Other(AddAccountBuyConditionOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuyConditionOther_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑股票买入额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/modify/other"), HttpPost]
        [Description("编辑股票买入额外条件-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionOther_Other(ModifyAccountBuyConditionOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionOther_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改股票买入额外条件状态-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/status/modify/other"), HttpPost]
        [Description("修改股票买入额外条件状态-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionOtherStatus_Other(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionOtherStatus_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除股票买入额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/delete/other"), HttpPost]
        [Description("删除股票买入额外条件-额外关系")]
        [CheckUserLoginFilter]
        public object DeleteAccountBuyConditionOther_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountBuyConditionOther_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取股票买入转自动条件列表-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/list/other"), HttpPost]
        [Description("获取股票买入转自动条件列表-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionAutoInfo> GetAccountBuyConditionAutoList_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionAutoList_Other(request, basedata);
        }

        /// <summary>
        /// 添加股票买入转自动条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/add/other"), HttpPost]
        [Description("添加股票买入转自动条件-额外关系")]
        [CheckUserLoginFilter]
        public object AddAccountBuyConditionAuto_Other(AddAccountBuyConditionAutoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuyConditionAuto_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑股票买入转自动条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/modify/other"), HttpPost]
        [Description("编辑股票买入转自动条件-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionAuto_Other(ModifyAccountBuyConditionAutoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionAuto_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改股票买入转自动条件状态-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/status/modify/other"), HttpPost]
        [Description("修改股票买入转自动条件状态-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionAutoStatus_Other(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionAutoStatus_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除股票买入转自动条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/delete/other"), HttpPost]
        [Description("删除股票买入转自动条件-额外关系")]
        [CheckUserLoginFilter]
        public object DeleteAccountBuyConditionAuto_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountBuyConditionAuto_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询股票买入额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/par/other"), HttpPost]
        [Description("查询股票买入额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherParInfo> GetAccountBuyConditionOtherPar_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionOtherPar_Other(request, basedata);
        }

        /// <summary>
        /// 查询股票买入额外条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/par/plate/other"), HttpPost]
        [Description("查询股票买入额外条件类型参数(板块涨跌幅)-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherParInfo> GetAccountBuyConditionOtherParPlate_Other(GetAccountBuyConditionOtherParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionOtherParPlate_Other(request, basedata);
        }

        /// <summary>
        /// 添加股票买入额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/par/add/other"), HttpPost]
        [Description("添加股票买入额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object AddAccountBuyConditionOtherPar_Other(AddAccountBuyConditionOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuyConditionOtherPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量添加股票买入额外条件类型参数(板块涨跌幅1)-额外关系
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/conditiontrade/buy/other/par/add/batch/other"), HttpPost]
        [Description("批量添加股票买入额外条件类型参数(板块涨跌幅1)-额外关系")]
        public async Task<object> BatchAddAccountBuyConditionOtherPar_Other()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');

                            list.Add(datas[0].Trim());
                        }
                        return trendHandler.BatchAddAccountBuyConditionOtherPar_Other(Type, RelId, list);
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
        /// 批量添加股票买入额外条件类型参数(板块涨跌幅2)-额外关系
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/conditiontrade/buy/other/par/add/batch2/other"), HttpPost]
        [Description("批量添加股票买入额外条件类型参数(板块涨跌幅2)-额外关系")]
        public async Task<object> BatchAddAccountBuyConditionOtherPar2_Other()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<BatchAddSharesConditionTrendPar2Obj> list = new List<BatchAddSharesConditionTrendPar2Obj>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 3)
                            {
                                continue;
                            }
                            string groupName = datas[0].Trim();
                            int compare = datas[1].Trim() == ">=" ? 1 : 2;
                            string rate = datas[2].Trim();

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName = groupName,
                                Compare = compare,
                                Rate = rate
                            });
                        }
                        return trendHandler.BatchAddAccountBuyConditionOtherPar2_Other(Type, RelId, list);
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
        /// 批量删除股票买入额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/par/delete/other/batch"), HttpPost]
        [Description("批量删除股票买入额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object BatchDeleteAccountBuyConditionOtherPar_Other(BatchDeleteAccountBuyConditionOtherPar_OtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteAccountBuyConditionOtherPar_Other(request, basedata);
            return null;
        }


        /// <summary>
        /// 编辑股票买入额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/par/modify/other"), HttpPost]
        [Description("编辑股票买入额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionOtherPar_Other(ModifyAccountBuyConditionOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionOtherPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除股票买入额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/other/par/delete/other"), HttpPost]
        [Description("删除股票买入额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object DeleteAccountBuyConditionOtherPar_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountBuyConditionOtherPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询股票买入转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/par/other"), HttpPost]
        [Description("查询股票买入转自动条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionAutoParInfo> GetAccountBuyConditionAutoPar_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionAutoPar_Other(request, basedata);
        }

        /// <summary>
        /// 查询股票买入转自动条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/par/plate/other"), HttpPost]
        [Description("查询股票买入转自动条件类型参数(板块涨跌幅)-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionAutoParInfo> GetAccountBuyConditionAutoParPlate_Other(GetAccountBuyConditionAutoParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionAutoParPlate_Other(request, basedata);
        }

        /// <summary>
        /// 添加股票买入转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/par/add/other"), HttpPost]
        [Description("添加股票买入转自动条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object AddAccountBuyConditionAutoPar_Other(AddAccountBuyConditionAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuyConditionAutoPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量添加股票买入转自动条件类型参数(板块涨跌幅1)-额外关系
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/conditiontrade/buy/auto/par/add/batch/other"), HttpPost]
        [Description("批量添加股票买入转自动条件类型参数(板块涨跌幅1)-额外关系")]
        public async Task<object> BatchAddAccountBuyConditionAutoPar_Other()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');

                            list.Add(datas[0].Trim());
                        }
                        return trendHandler.BatchAddAccountBuyConditionAutoPar_Other(Type, RelId, list);
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
        /// 批量添加股票买入转自动条件类型参数(板块涨跌幅2)-额外关系
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/conditiontrade/buy/auto/par/add/batch2/other"), HttpPost]
        [Description("批量添加股票买入转自动条件类型参数(板块涨跌幅2)-额外关系")]
        public async Task<object> BatchAddAccountBuyConditionAutoPar2_Other()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<BatchAddSharesConditionTrendPar2Obj> list = new List<BatchAddSharesConditionTrendPar2Obj>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 3)
                            {
                                continue;
                            }
                            string groupName = datas[0].Trim();
                            int compare = datas[1].Trim() == ">=" ? 1 : 2;
                            string rate = datas[2].Trim();

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName = groupName,
                                Compare = compare,
                                Rate = rate
                            });
                        }
                        return trendHandler.BatchAddAccountBuyConditionAutoPar2_Other(Type, RelId, list);
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
        /// 批量删除股票买入转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/par/delete/other/batch"), HttpPost]
        [Description("批量删除股票买入转自动条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object BatchDeleteAccountBuyConditionAutoPar_Other(BatchDeleteAccountBuyConditionAutoPar_OtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteAccountBuyConditionAutoPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑股票买入转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/par/modify/other"), HttpPost]
        [Description("编辑股票买入转自动条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyAccountBuyConditionAutoPar_Other(ModifyAccountBuyConditionAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuyConditionAutoPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除股票买入转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/conditiontrade/buy/auto/par/delete/other"), HttpPost]
        [Description("删除股票买入转自动条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object DeleteAccountBuyConditionAutoPar_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountBuyConditionAutoPar_Other(request, basedata);
            return null;
        }
        #endregion

        /// <summary>
        /// 查询买入提示列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("buy/tip/list"), HttpPost]
        [Description("查询买入提示列表")]
        [CheckUserLoginFilter]
        public GetBuyTipListRes GetBuyTipList(GetBuyTipListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetBuyTipList(request,basedata);
        }

        /// <summary>
        /// 确认买入提示
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("buy/tip/confirm"), HttpPost]
        [Description("确认买入提示")]
        [CheckUserLoginFilter]
        public object ConfirmBuyTip(ConfirmBuyTipRequest request)
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
        /// 复制条件模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/copy"), HttpPost]
        [Description("复制条件模板")]
        [CheckUserLoginFilter]
        public object CopyConditiontradeTemplate(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.CopyConditiontradeTemplate(request, basedata);
            return null;
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
        /// 获取系统条件卖出模板详情列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("sys/conditiontrade/template/sell/details/list"), HttpPost]
        [Description("获取系统条件卖出模板详情列表")]
        [CheckUserLoginFilter]
        public List<ConditiontradeTemplateSellDetailsInfo> GetSysConditiontradeTemplateSellDetailsList(GetConditiontradeTemplateSellDetailsListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetSysConditiontradeTemplateSellDetailsList(request);
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
        /// 批量设置股票自定义分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/buy/shares/group/batch/set"), HttpPost]
        [Description("批量设置股票自定义分组")]
        [CheckUserLoginFilter]
        public object BatchSetConditiontradeBuySharesGroup(BatchSetConditiontradeBuySharesGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchSetConditiontradeBuySharesGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量设置股票跟投
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/buy/shares/follow/batch/set"), HttpPost]
        [Description("批量设置股票跟投")]
        [CheckUserLoginFilter]
        public object BatchSetConditiontradeBuySharesFollow(BatchSetConditiontradeBuySharesFollowRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchSetConditiontradeBuySharesFollow(request, basedata);
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
        /// 获取条件单走势模板列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("shares/condition/trend/list"), HttpPost]
        [Description("获取条件单走势模板列表")]
        [CheckUserLoginFilter]
        public List<SharesMonitorTrendInfo> GetSharesConditionTrendList()
        {
            return trendHandler.GetSharesConditionTrendList();
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
        /// 编辑条件买入模板额外条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/modify"), HttpPost]
        [Description("编辑条件买入模板额外条件")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyOther(ModifyConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyOther(request, basedata);
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
        /// 编辑条件买入模板转自动条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/modify"), HttpPost]
        [Description("编辑条件买入模板转自动条件")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyAuto(ModifyConditiontradeTemplateBuyAutoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyAuto(request, basedata);
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
        /// 查询条件买入模板额外条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/plate"), HttpPost]
        [Description("查询条件买入模板额外条件类型参数(板块涨跌幅)")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateBuyOtherParPlate(GetConditiontradeTemplateBuyOtherParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyOtherParPlate(request, basedata);
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
        /// 批量添加条件买入模板额外条件类型参数(板块涨跌幅1)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/other/par/add/batch"), HttpPost]
        [Description("批量添加条件买入模板额外条件类型参数(板块涨跌幅1)")]
        public async Task<object> BatchAddConditiontradeTemplateBuyOtherPar()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');

                            list.Add(datas[0].Trim());
                        }
                        return trendHandler.BatchAddConditiontradeTemplateBuyOtherPar(Type, RelId, list);
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
        /// 批量添加条件买入模板额外条件类型参数(板块涨跌幅2)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/other/par/add/batch2"), HttpPost]
        [Description("批量添加条件买入模板额外条件类型参数(板块涨跌幅2)")]
        public async Task<object> BatchAddConditiontradeTemplateBuyOtherPar2()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<BatchAddSharesConditionTrendPar2Obj> list = new List<BatchAddSharesConditionTrendPar2Obj>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 3)
                            {
                                continue;
                            }
                            string groupName = datas[0].Trim();
                            int compare = datas[1].Trim() == ">=" ? 1 : 2;
                            string rate = datas[2].Trim();

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName = groupName,
                                Compare = compare,
                                Rate = rate
                            });
                        }
                        return trendHandler.BatchAddConditiontradeTemplateBuyOtherPar2(Type, RelId, list);
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
        /// 批量删除条件买入模板额外条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/delete/batch"), HttpPost]
        [Description("批量删除条件买入模板额外条件类型参数(板块涨跌幅)")]
        [CheckUserLoginFilter]
        public object BatchDeleteConditiontradeTemplateBuyOtherPar(BatchDeleteConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteConditiontradeTemplateBuyOtherPar(request, basedata);
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
        /// 查询条件买入模板转自动条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/plate"), HttpPost]
        [Description("查询条件买入模板转自动条件类型参数(板块涨跌幅)")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyAutoParInfo> GetConditiontradeTemplateBuyAutoParPlate(GetConditiontradeTemplateBuyAutoParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyAutoParPlate(request, basedata);
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
        /// 批量添加条件买入模板转自动条件类型参数(板块涨跌幅1)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/auto/par/add/batch"), HttpPost]
        [Description("批量添加条件买入模板转自动条件类型参数(板块涨跌幅1)")]
        public async Task<object> BatchAddConditiontradeTemplateBuyAutoPar()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');

                            list.Add(datas[0].Trim());
                        }
                        return trendHandler.BatchAddConditiontradeTemplateBuyAutoPar(Type, RelId, list);
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
        /// 批量添加条件买入模板转自动条件类型参数(板块涨跌幅2)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/auto/par/add/batch2"), HttpPost]
        [Description("批量添加条件买入模板转自动条件类型参数(板块涨跌幅2)")]
        public async Task<object> BatchAddConditiontradeTemplateBuyAutoPar2()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<BatchAddSharesConditionTrendPar2Obj> list = new List<BatchAddSharesConditionTrendPar2Obj>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 3)
                            {
                                continue;
                            }
                            string groupName = datas[0].Trim();
                            int compare = datas[1].Trim() == ">=" ? 1 : 2;
                            string rate = datas[2].Trim();

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName = groupName,
                                Compare = compare,
                                Rate = rate
                            });
                        }
                        return trendHandler.BatchAddConditiontradeTemplateBuyAutoPar2(Type, RelId, list);
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
        /// 批量删除条件买入模板转自动条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/delete/batch"), HttpPost]
        [Description("批量删除条件买入模板转自动条件类型参数(板块涨跌幅)")]
        [CheckUserLoginFilter]
        public object BatchDeleteConditiontradeTemplateBuyAutoPar(BatchDeleteConditiontradeTemplateBuyAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteConditiontradeTemplateBuyAutoPar(request, basedata);
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


        #region=======================额外关系=====================================
        /// <summary>
        /// 获取条件买入模板额外条件列表-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/list/other"), HttpPost]
        [Description("获取条件买入模板额外条件列表-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherInfo> GetConditiontradeTemplateBuyOtherList_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyOtherList_Other(request, basedata);
        }

        /// <summary>
        /// 添加条件买入模板额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/add/other"), HttpPost]
        [Description("添加条件买入模板额外条件-额外关系")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateBuyOther_Other(AddConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateBuyOther_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/modify/other"), HttpPost]
        [Description("编辑条件买入模板额外条件-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyOther_Other(ModifyConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyOther_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板额外条件状态-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/status/modify/other"), HttpPost]
        [Description("修改条件买入模板额外条件状态-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyOtherStatus_Other(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyOtherStatus_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/delete/other"), HttpPost]
        [Description("删除条件买入模板额外条件-额外关系")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateBuyOther_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateBuyOther_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取条件买入模板转自动条件列表-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/list/other"), HttpPost]
        [Description("获取条件买入模板转自动条件列表-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyAutoInfo> GetConditiontradeTemplateBuyAutoList_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyAutoList_Other(request, basedata);
        }

        /// <summary>
        /// 添加条件买入模板转自动条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/add/other"), HttpPost]
        [Description("添加条件买入模板转自动条件-额外关系")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateBuyAuto_Other(AddConditiontradeTemplateBuyAutoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateBuyAuto_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板转自动条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/modify/other"), HttpPost]
        [Description("编辑条件买入模板转自动条件-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyAuto_Other(ModifyConditiontradeTemplateBuyAutoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyAuto_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板转自动条件状态-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/status/modify/other"), HttpPost]
        [Description("修改条件买入模板转自动条件状态-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyAutoStatus_Other(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyAutoStatus_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板转自动条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/delete/other"), HttpPost]
        [Description("删除条件买入模板转自动条件-额外关系")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateBuyAuto_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateBuyAuto_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询条件买入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/other"), HttpPost]
        [Description("查询条件买入模板额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateBuyOtherPar_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyOtherPar_Other(request, basedata);
        }

        /// <summary>
        /// 查询条件买入模板额外条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/plate/other"), HttpPost]
        [Description("查询条件买入模板额外条件类型参数(板块涨跌幅)-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateBuyOtherParPlate_Other(GetConditiontradeTemplateBuyOtherParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyOtherParPlate_Other(request, basedata);
        }

        /// <summary>
        /// 添加条件买入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/add/other"), HttpPost]
        [Description("添加条件买入模板额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateBuyOtherPar_Other(AddConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateBuyOtherPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量添加条件买入模板额外条件类型参数(板块涨跌幅1)-额外关系
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/other/par/add/batch/other"), HttpPost]
        [Description("批量添加条件买入模板额外条件类型参数(板块涨跌幅1)-额外关系")]
        public async Task<object> BatchAddConditiontradeTemplateBuyOtherPar_Other()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');

                            list.Add(datas[0].Trim());
                        }
                        return trendHandler.BatchAddConditiontradeTemplateBuyOtherPar_Other(Type, RelId, list);
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
        /// 批量添加条件买入模板额外条件类型参数(板块涨跌幅2)-额外关系
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/other/par/add/batch2/other"), HttpPost]
        [Description("批量添加条件买入模板额外条件类型参数(板块涨跌幅2)-额外关系")]
        public async Task<object> BatchAddConditiontradeTemplateBuyOtherPar2_Other()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<BatchAddSharesConditionTrendPar2Obj> list = new List<BatchAddSharesConditionTrendPar2Obj>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 3)
                            {
                                continue;
                            }
                            string groupName = datas[0].Trim();
                            int compare = datas[1].Trim() == ">=" ? 1 : 2;
                            string rate = datas[2].Trim();

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName = groupName,
                                Compare = compare,
                                Rate = rate
                            });
                        }
                        return trendHandler.BatchAddConditiontradeTemplateBuyOtherPar2_Other(Type, RelId, list);
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
        /// 批量删除条件买入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/delete/other/batch"), HttpPost]
        [Description("批量删除条件买入模板额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object BatchDeleteConditiontradeTemplateBuyOtherPar_Other(BatchDeleteConditiontradeTemplateBuyOtherPar_OtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteConditiontradeTemplateBuyOtherPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/modify/other"), HttpPost]
        [Description("编辑条件买入模板额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyOtherPar_Other(ModifyConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyOtherPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/delete/other"), HttpPost]
        [Description("删除条件买入模板额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateBuyOtherPar_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateBuyOtherPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询条件买入模板转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/other"), HttpPost]
        [Description("查询条件买入模板转自动条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyAutoParInfo> GetConditiontradeTemplateBuyAutoPar_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyAutoPar_Other(request, basedata);
        }

        /// <summary>
        /// 查询条件买入模板转自动条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/plate/other"), HttpPost]
        [Description("查询条件买入模板转自动条件类型参数(板块涨跌幅)-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyAutoParInfo> GetConditiontradeTemplateBuyAutoParPlate_Other(GetConditiontradeTemplateBuyAutoParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateBuyAutoParPlate_Other(request, basedata);
        }

        /// <summary>
        /// 添加条件买入模板转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/add/other"), HttpPost]
        [Description("添加条件买入模板转自动条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateBuyAutoPar_Other(AddConditiontradeTemplateBuyAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateBuyAutoPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量添加条件买入模板转自动条件类型参数(板块涨跌幅1)-额外关系
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/auto/par/add/batch/other"), HttpPost]
        [Description("批量添加条件买入模板转自动条件类型参数(板块涨跌幅1)-额外关系")]
        public async Task<object> BatchAddConditiontradeTemplateBuyAutoPar_Other()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');

                            list.Add(datas[0].Trim());
                        }
                        return trendHandler.BatchAddConditiontradeTemplateBuyAutoPar_Other(Type, RelId, list);
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
        /// 批量添加条件买入模板转自动条件类型参数(板块涨跌幅2)-额外关系
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/buy/auto/par/add/batch2/other"), HttpPost]
        [Description("批量添加条件买入模板转自动条件类型参数(板块涨跌幅2)-额外关系")]
        public async Task<object> BatchAddConditiontradeTemplateBuyAutoPar2_Other()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<BatchAddSharesConditionTrendPar2Obj> list = new List<BatchAddSharesConditionTrendPar2Obj>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 3)
                            {
                                continue;
                            }
                            string groupName = datas[0].Trim();
                            int compare = datas[1].Trim() == ">=" ? 1 : 2;
                            string rate = datas[2].Trim();

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName = groupName,
                                Compare = compare,
                                Rate = rate
                            });
                        }
                        return trendHandler.BatchAddConditiontradeTemplateBuyAutoPar2_Other(Type, RelId, list);
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
        /// 批量删除条件买入模板转自动条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/delete/other/batch"), HttpPost]
        [Description("批量删除条件买入模板转自动条件类型参数(板块涨跌幅)-额外关系")]
        [CheckUserLoginFilter]
        public object BatchDeleteConditiontradeTemplateBuyAutoPar_Other(BatchDeleteConditiontradeTemplateBuyAutoPar_OtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteConditiontradeTemplateBuyAutoPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/modify/other"), HttpPost]
        [Description("编辑条件买入模板转自动条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateBuyAutoPar_Other(ModifyConditiontradeTemplateBuyAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateBuyAutoPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/delete/other"), HttpPost]
        [Description("删除条件买入模板转自动条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateBuyAutoPar_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateBuyAutoPar_Other(request, basedata);
            return null;
        }
        #endregion


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
        public object DeleteConditiontradeBuyGroup(DeleteConditiontradeBuyGroupRequest request)
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

        /// <summary>
        /// 查询条件买入自定义分组同步被授权账户分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/syncroaccount/list"), HttpPost]
        [Description("查询条件买入自定义分组同步被授权账户分组列表")]
        [CheckUserLoginFilter]
        public List<ConditiontradeBuyGroupSharesAccount> GetConditiontradeBuyGroupSyncroAccountList(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeBuyGroupSyncroAccountList(request, basedata);
        }

        /// <summary>
        /// 添加条件买入自定义分组同步被授权账户分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/syncroaccount/add"), HttpPost]
        [Description("添加条件买入自定义分组同步被授权账户分组")]
        [CheckUserLoginFilter]
        public object AddConditiontradeBuyGroupSyncroAccount(AddConditiontradeBuyGroupSharesAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeBuyGroupSyncroAccount(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件买入自定义分组同步被授权账户分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/syncroaccount/modify"), HttpPost]
        [Description("编辑条件买入自定义分组同步被授权账户分组")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeBuyGroupSyncroAccount(ModifyConditiontradeBuyGroupSyncroAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeBuyGroupSyncroAccount(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件买入自定义分组同步被授权账户分组状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/syncroaccount/status/modify"), HttpPost]
        [Description("修改条件买入自定义分组同步被授权账户分组状态")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeBuyGroupSyncroAccountStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeBuyGroupSyncroAccountStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件买入自定义分组下一轮买入
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/syncroaccount/buynext/modify"), HttpPost]
        [Description("修改条件买入自定义分组下一轮买入")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeBuyGroupSyncroAccountBuynext(ModifyConditiontradeBuyGroupSyncroAccountBuynextRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeBuyGroupSyncroAccountBuynext(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件买入自定义分组同步用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/buy/group/syncroaccount/delete"), HttpPost]
        [Description("删除条件买入自定义分组同步用户")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeBuyGroupSyncroAccount(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeBuyGroupSyncroAccount(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取用户买入条件设置
        /// </summary>
        /// <returns></returns>
        [Description("获取用户买入条件设置")]
        [Route("account/buy/setting"), HttpPost]
        [CheckUserLoginFilter]
        public List<AccountBuySettingInfo> GetAccountBuySetting(GetAccountBuySettingRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuySetting(request, basedata);
        }

        /// <summary>
        /// 修改用户买入条件设置
        /// </summary>
        /// <returns></returns>
        [Description("修改用户买入条件设置")]
        [Route("account/buy/setting/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountBuySetting(ModifyAccountBuySettingRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuySetting(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取用户买入条件设置参数列表
        /// </summary>
        /// <returns></returns>
        [Description("获取用户买入条件设置参数列表")]
        [Route("account/buy/setting/par"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AccountBuySettingParInfo> GetAccountBuySettingParList(GetAccountBuySettingParListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuySettingParList(request, basedata);
        }

        /// <summary>
        /// 添加用户买入条件设置参数
        /// </summary>
        /// <returns></returns>
        [Description("添加用户买入条件设置参数")]
        [Route("account/buy/setting/par/add"), HttpPost]
        [CheckUserLoginFilter]
        public object AddAccountBuySettingPar(AddAccountBuySettingParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountBuySettingPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑用户买入条件设置参数
        /// </summary>
        /// <returns></returns>
        [Description("编辑用户买入条件设置参数")]
        [Route("account/buy/setting/par/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountBuySettingPar(ModifyAccountBuySettingParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountBuySettingPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除用户买入条件设置参数
        /// </summary>
        /// <returns></returns>
        [Description("删除用户买入条件设置参数")]
        [Route("account/buy/setting/par/delete"), HttpPost]
        [CheckUserLoginFilter]
        public object DeleteAccountBuySettingPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountBuySettingPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取用户买入条件触发记录
        /// </summary>
        /// <returns></returns>
        [Description("获取用户买入条件触发记录")]
        [Route("account/buy/condition/record"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionRecordInfo> GetAccountBuyConditionRecord(GetAccountBuyConditionRecordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountBuyConditionRecord(request, basedata);
        }

        /// <summary>
        /// 获取跟投分组列表
        /// </summary>
        /// <returns></returns>
        [Description("获取跟投分组列表")]
        [Route("account/follow/group/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AccountFollowGroupInfo> GetAccountFollowGroupList(GetAccountFollowGroupListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountFollowGroupList(request, basedata);
        }

        /// <summary>
        /// 获取下一跟投分组
        /// </summary>
        /// <returns></returns>
        [Description("获取跟投分组列表")]
        [Route("account/follow/group/next"), HttpPost]
        [CheckUserLoginFilter]
        public AccountFollowGroupInfo GetAccountNextFollowGroup()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountNextFollowGroup(basedata);
        }

        /// <summary>
        /// 添加跟投分组
        /// </summary>
        /// <returns></returns>
        [Description("添加跟投分组")]
        [Route("account/follow/group/add"), HttpPost]
        [CheckUserLoginFilter]
        public object AddAccountFollowGroup(AddAccountFollowGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountFollowGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑跟投分组
        /// </summary>
        /// <returns></returns>
        [Description("编辑跟投分组")]
        [Route("account/follow/group/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountFollowGroup(ModifyAccountFollowGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountFollowGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改跟投分组状态
        /// </summary>
        /// <returns></returns>
        [Description("修改跟投分组状态")]
        [Route("account/follow/group/status/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountFollowGroupStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountFollowGroupStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除跟投分组
        /// </summary>
        /// <returns></returns>
        [Description("删除跟投分组")]
        [Route("account/follow/group/delete"), HttpPost]
        [CheckUserLoginFilter]
        public object DeleteAccountFollowGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountFollowGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取跟投分组账户列表
        /// </summary>
        /// <returns></returns>
        [Description("获取跟投分组账户列表")]
        [Route("account/follow/group/followaccount/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AccountFollowGroupFollowAccountInfo> GetAccountFollowGroupFollowAccountList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountFollowGroupFollowAccountList(request, basedata);
        }

        /// <summary>
        /// 添加跟投分组账户
        /// </summary>
        /// <returns></returns>
        [Description("添加跟投分组账户")]
        [Route("account/follow/group/followaccount/add"), HttpPost]
        [CheckUserLoginFilter]
        public object AddAccountFollowGroupFollowAccount(AddAccountFollowGroupFollowAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountFollowGroupFollowAccount(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑跟投分组账户
        /// </summary>
        /// <returns></returns>
        [Description("编辑跟投分组账户")]
        [Route("account/follow/group/followaccount/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountFollowGroupFollowAccount(ModifyAccountFollowGroupFollowAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountFollowGroupFollowAccount(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改跟投分组账户状态
        /// </summary>
        /// <returns></returns>
        [Description("修改跟投分组账户状态")]
        [Route("account/follow/group/followaccount/status/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountFollowGroupFollowAccountStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountFollowGroupFollowAccountStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除跟投分组账户
        /// </summary>
        /// <returns></returns>
        [Description("删除跟投分组账户")]
        [Route("account/follow/group/followaccount/delete"), HttpPost]
        [CheckUserLoginFilter]
        public object DeleteAccountFollowGroupFollowAccount(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountFollowGroupFollowAccount(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取自动买入同步设置列表
        /// </summary>
        /// <returns></returns>
        [Description("获取自动买入同步设置列表")]
        [Route("account/auto/buy/synchro/setting/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AccountAutoBuySyncroSettingInfo> GetAccountAutoBuySyncroSettingList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountAutoBuySyncroSettingList(request,basedata);
        }

        /// <summary>
        /// 根据手机号匹配用户信息
        /// </summary>
        /// <returns></returns>
        [Description("根据手机号匹配用户信息")]
        [Route("bymobile/account/list"), HttpPost]
        [CheckUserLoginFilter]
        public List<AccountInfo> GetAccountListByMobile(GetAccountListByMobileRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountListByMobile(request, basedata);
        }

        /// <summary>
        /// 添加自动买入同步设置
        /// </summary>
        /// <returns></returns>
        [Description("添加自动买入同步设置")]
        [Route("account/auto/buy/synchro/setting/add"), HttpPost]
        [CheckUserLoginFilter]
        public object AddAccountAutoBuySyncroSetting(AddAccountAutoBuySyncroSettingRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountAutoBuySyncroSetting(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑自动买入同步设置
        /// </summary>
        /// <returns></returns>
        [Description("编辑自动买入同步设置")]
        [Route("account/auto/buy/synchro/setting/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoBuySyncroSetting(ModifyAccountAutoBuySyncroSettingRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoBuySyncroSetting(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改自动买入同步设置状态
        /// </summary>
        /// <returns></returns>
        [Description("修改自动买入同步设置状态")]
        [Route("account/auto/buy/synchro/setting/status/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoBuySyncroSettingStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoBuySyncroSettingStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除自动买入同步设置
        /// </summary>
        /// <returns></returns>
        [Description("删除自动买入同步设置")]
        [Route("account/auto/buy/synchro/setting/delete"), HttpPost]
        [CheckUserLoginFilter]
        public object DeleteAccountAutoBuySyncroSetting(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountAutoBuySyncroSetting(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取自动买入同步被授权账户列表
        /// </summary>
        /// <returns></returns>
        [Description("获取自动买入同步被授权账户列表")]
        [Route("account/auto/buy/synchro/authorized/account/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AutoBuySyncroAuthorizedAccountInfo> GetAutoBuySyncroAuthorizedAccountList(GetAutoBuySyncroAuthorizedAccountListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAutoBuySyncroAuthorizedAccountList(request, basedata);
        }

        /// <summary>
        /// 查询交易时间分组列表
        /// </summary>
        /// <returns></returns>
        [Description("查询交易时间分组列表")]
        [Route("shares/tradetime/group/list"), HttpPost]
        [CheckUserLoginFilter]
        public List<SharesTradeTimeGroupInfo> GetSharesTradeTimeGroupList()
        {
            return trendHandler.GetSharesTradeTimeGroupList();
        }

        /// <summary>
        /// 获取自动买入同步设置分组列表
        /// </summary>
        /// <returns></returns>
        [Description("获取自动买入同步设置分组列表")]
        [Route("account/auto/buy/synchro/setting/authorizedgroup/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AccountAutoBuySyncroSettingAuthorizedGroupInfo> GetAccountAutoBuySyncroSettingAuthorizedGroupList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountAutoBuySyncroSettingAuthorizedGroupList(request, basedata);
        }

        /// <summary>
        /// 添加自动买入同步设置分组
        /// </summary>
        /// <returns></returns>
        [Description("添加自动买入同步设置分组")]
        [Route("account/auto/buy/synchro/setting/authorizedgroup/add"), HttpPost]
        [CheckUserLoginFilter]
        public object AddAccountAutoBuySyncroSettingAuthorizedGroup(AddAccountAutoBuySyncroSettingAuthorizedGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountAutoBuySyncroSettingAuthorizedGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑自动买入同步设置分组
        /// </summary>
        /// <returns></returns>
        [Description("编辑自动买入同步设置分组")]
        [Route("account/auto/buy/synchro/setting/authorizedgroup/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoBuySyncroSettingAuthorizedGroup(ModifyAccountAutoBuySyncroSettingAuthorizedGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoBuySyncroSettingAuthorizedGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 绑定自动买入同步设置分组账户
        /// </summary>
        /// <returns></returns>
        [Description("绑定自动买入同步设置分组账户")]
        [Route("account/auto/buy/synchro/setting/authorizedgroup/bind"), HttpPost]
        [CheckUserLoginFilter]
        public object BindAccountAutoBuySyncroSettingAuthorizedGroup(BindAccountAutoBuySyncroSettingAuthorizedGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BindAccountAutoBuySyncroSettingAuthorizedGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除自动买入同步设置分组
        /// </summary>
        /// <returns></returns>
        [Description("删除自动买入同步设置分组")]
        [Route("account/auto/buy/synchro/setting/authorizedgroup/delete"), HttpPost]
        [CheckUserLoginFilter]
        public object DeleteAccountAutoBuySyncroSettingAuthorizedGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountAutoBuySyncroSettingAuthorizedGroup(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询实时监控过滤表示
        /// </summary>
        /// <returns></returns>
        [Description("查询实时监控过滤表示")]
        [Route("account/realtime/tab/setting"), HttpPost]
        [CheckUserLoginFilter]
        public List<AccountRealTimeTabSetting> GetAccountRealTimeTabSetting(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountRealTimeTabSetting(request, basedata);
        }

        /// <summary>
        /// 更新实时监控过滤表示
        /// </summary>
        /// <returns></returns>
        [Description("更新实时监控过滤表示")]
        [Route("account/realtime/tab/setting/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountRealTimeTabSetting(ModifyAccountRealTimeTabSettingRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountRealTimeTabSetting(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取授权账户股票列表
        /// </summary>
        /// <returns></returns>
        [Description("获取授权账户股票列表")]
        [Route("authorize/account/shares/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AuthorizeAccountSharesInfo> GetAuthorizeAccountSharesList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAuthorizeAccountSharesList(request, basedata);
        }

        /// <summary>
        /// 修改授权账户股票状态
        /// </summary>
        /// <returns></returns>
        [Description("修改授权账户股票状态")]
        [Route("authorize/account/shares/status/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAuthorizeAccountSharesStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAuthorizeAccountSharesStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量修改授权账户股票状态
        /// </summary>
        /// <returns></returns>
        [Description("批量修改授权账户股票状态")]
        [Route("authorize/account/shares/status/modify/batch"), HttpPost]
        [CheckUserLoginFilter]
        public object BatchModifyAuthorizeAccountSharesStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchModifyAuthorizeAccountSharesStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取同步未读股票数量
        /// </summary>
        /// <returns></returns>
        [Description("获取同步未读股票数量")]
        [Route("authorize/account/shares/notreadcount"), HttpPost]
        [CheckUserLoginFilter]
        public object GetAuthorizeAccountSharesNotReadCount()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAuthorizeAccountSharesNotReadCount(basedata);
        }

        /// <summary>
        /// 获取板块涨跌幅排行
        /// </summary>
        /// <returns></returns>
        [Description("获取板块涨跌幅排行")]
        [Route("plate/riserate/rank/list"), HttpPost]
        [CheckUserLoginFilter]
        public List<SharesPlateInfo> GetPlateRiseRateRankList()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetPlateRiseRateRankList(basedata);
        }

        #region====自动选票====
        /// <summary>
        /// 获取自动选票列表
        /// </summary>
        /// <returns></returns>
        [Description("获取自动选票列表")]
        [Route("account/auto/tobuy/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<AccountAutoTobuyInfo> GetAccountAutoTobuyList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountAutoTobuyList(request, basedata);
        }

        /// <summary>
        /// 添加自动选票
        /// </summary>
        /// <returns></returns>
        [Description("添加自动选票")]
        [Route("account/auto/tobuy/add"), HttpPost]
        [CheckUserLoginFilter]
        public object AddAccountAutoTobuy(AddAccountAutoTobuyRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountAutoTobuy(request, basedata);
            return null;
        }

        /// <summary>
        /// 从模板导入自动选票
        /// </summary>
        /// <returns></returns>
        [Description("从模板导入自动选票")]
        [Route("account/auto/tobuy/export"), HttpPost]
        [CheckUserLoginFilter]
        public object ExportAccountAutoTobuy(ExportAccountAutoTobuyRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ExportAccountAutoTobuy(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑自动选票
        /// </summary>
        /// <returns></returns>
        [Description("编辑自动选票")]
        [Route("account/auto/tobuy/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoTobuy(ModifyAccountAutoTobuyRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoTobuy(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改自动选票状态
        /// </summary>
        /// <returns></returns>
        [Description("修改自动选票状态")]
        [Route("account/auto/tobuy/status/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoTobuyStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoTobuyStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除自动选票
        /// </summary>
        /// <returns></returns>
        [Description("删除自动选票")]
        [Route("account/auto/tobuy/delete"), HttpPost]
        [CheckUserLoginFilter]
        public object DeleteAccountAutoTobuy(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountAutoTobuy(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取自动选票触发条件分组列表
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/list"), HttpPost]
        [Description("获取自动选票触发条件分组列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherGroupInfo> GetAccountAutoTobuyOtherList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountAutoTobuyOtherList(request, basedata);
        }

        /// <summary>
        /// 添加自动选票触发条件分组
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/add"), HttpPost]
        [Description("添加自动选票触发条件分组")]
        [CheckUserLoginFilter]
        public object AddAccountAutoTobuyOther(AddAccountBuyConditionOtherGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountAutoTobuyOther(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑自动选票触发条件分组
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/modify"), HttpPost]
        [Description("编辑自动选票触发条件分组")]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoTobuyOther(ModifyAccountBuyConditionOtherGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoTobuyOther(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改自动选票触发条件分组状态
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/status/modify"), HttpPost]
        [Description("修改自动选票触发条件分组状态")]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoTobuyOtherStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoTobuyOtherStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除自动选票触发条件分组
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/delete"), HttpPost]
        [Description("删除自动选票触发条件分组")]
        [CheckUserLoginFilter]
        public object DeleteAccountAutoTobuyOther(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountAutoTobuyOther(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取自动选票触发条件参数列表
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/list"), HttpPost]
        [Description("获取自动选票触发条件参数列表")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherInfo> GetAccountAutoTobuyOtherTrendList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountAutoTobuyOtherTrendList(request, basedata);
        }

        /// <summary>
        /// 添加自动选票触发条件参数
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/add"), HttpPost]
        [Description("添加自动选票触发条件参数")]
        [CheckUserLoginFilter]
        public object AddAccountAutoTobuyOtherTrend(AddAccountBuyConditionOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountAutoTobuyOtherTrend(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑自动选票触发条件参数
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/modify"), HttpPost]
        [Description("编辑自动选票触发条件参数")]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoTobuyOtherTrend(ModifyAccountBuyConditionOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoTobuyOtherTrend(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改自动选票触发条件参数状态
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/status/modify"), HttpPost]
        [Description("修改自动选票触发条件参数状态")]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoTobuyOtherTrendStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoTobuyOtherTrendStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除自动选票触发条件参数
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/delete"), HttpPost]
        [Description("删除自动选票触发条件参数")]
        [CheckUserLoginFilter]
        public object DeleteAccountAutoTobuyOtherTrend(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountAutoTobuyOtherTrend(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询自动选票触发条件参数设置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/par"), HttpPost]
        [Description("查询自动选票触发条件参数设置")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherParInfo> GetAccountAutoTobuyOtherTrendPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountAutoTobuyOtherTrendPar(request, basedata);
        }

        /// <summary>
        /// 查询自动选票触发条件参数设置(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/par/plate"), HttpPost]
        [Description("查询自动选票触发条件参数设置(板块涨跌幅)")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherParInfo> GetAccountAutoTobuyOtherTrendParPlate(GetAccountBuyConditionOtherParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountAutoTobuyOtherTrendParPlate(request, basedata);
        }

        /// <summary>
        /// 添加自动选票触发条件参数设置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/par/add"), HttpPost]
        [Description("添加自动选票触发条件参数设置")]
        [CheckUserLoginFilter]
        public object AddAccountAutoTobuyOtherTrendPar(AddAccountBuyConditionOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountAutoTobuyOtherTrendPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量添加自动选票触发条件参数设置(板块涨跌幅1)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/auto/tobuy/other/trend/par/add/batch"), HttpPost]
        [Description("批量添加自动选票触发条件参数设置(板块涨跌幅1)")]
        public async Task<object> BatchAccountAutoTobuyOtherTrendPar()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');

                            list.Add(datas[0].Trim());
                        }
                        return trendHandler.BatchAccountAutoTobuyOtherTrendPar(Type, RelId, list);
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
        /// 批量添加自动选票触发条件参数设置(板块涨跌幅2)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/auto/tobuy/other/trend/par/add/batch2"), HttpPost]
        [Description("批量添加自动选票触发条件参数设置(板块涨跌幅2)")]
        public async Task<object> BatchAccountAutoTobuyOtherTrendPar2()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<BatchAddSharesConditionTrendPar2Obj> list = new List<BatchAddSharesConditionTrendPar2Obj>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 3)
                            {
                                continue;
                            }
                            string groupName = datas[0].Trim();
                            int compare = datas[1].Trim() == ">=" ? 1 : 2;
                            string rate = datas[2].Trim();

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName = groupName,
                                Compare = compare,
                                Rate = rate
                            });
                        }
                        return trendHandler.BatchAccountAutoTobuyOtherTrendPar2(Type, RelId, list);
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
        /// 批量删除自动选票触发条件参数设置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/par/delete/batch"), HttpPost]
        [Description("批量删除自动选票触发条件参数设置")]
        [CheckUserLoginFilter]
        public object BatchDeleteAccountAutoTobuyOtherTrendPar(BatchDeleteAccountBuyConditionOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteAccountAutoTobuyOtherTrendPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑自动选票触发条件参数设置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/par/modify"), HttpPost]
        [Description("编辑自动选票触发条件参数设置")]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoTobuyOtherTrendPar(ModifyAccountBuyConditionOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoTobuyOtherTrendPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除自动选票触发条件参数设置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/par/delete"), HttpPost]
        [Description("删除自动选票触发条件参数设置")]
        [CheckUserLoginFilter]
        public object DeleteAccountAutoTobuyOtherTrendPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountAutoTobuyOtherTrendPar(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取自动选票触发条件参数列表-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/list/other"), HttpPost]
        [Description("获取自动选票触发条件参数列表-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherInfo> GetAccountAutoTobuyOtherTrendList_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountAutoTobuyOtherTrendList_Other(request, basedata);
        }

        /// <summary>
        /// 添加自动选票触发条件参数-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/add/other"), HttpPost]
        [Description("添加自动选票触发条件参数-额外关系")]
        [CheckUserLoginFilter]
        public object AddAccountAutoTobuyOtherTrend_Other(AddAccountBuyConditionOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountAutoTobuyOtherTrend_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑自动选票触发条件参数-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/modify/other"), HttpPost]
        [Description("编辑自动选票触发条件参数-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoTobuyOtherTrend_Other(ModifyAccountBuyConditionOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoTobuyOtherTrend_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改自动选票触发条件参数状态-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/status/modify/other"), HttpPost]
        [Description("修改自动选票触发条件参数状态-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoTobuyOtherTrendStatus_Other(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoTobuyOtherTrendStatus_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除自动选票触发条件参数-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/delete/other"), HttpPost]
        [Description("删除自动选票触发条件参数-额外关系")]
        [CheckUserLoginFilter]
        public object DeleteAccountAutoTobuyOtherTrend_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountAutoTobuyOtherTrend_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询自动选票触发条件参数设置-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/par/other"), HttpPost]
        [Description("查询自动选票触发条件参数设置-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherParInfo> GetAccountAutoTobuyOtherTrendPar_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountAutoTobuyOtherTrendPar_Other(request, basedata);
        }

        /// <summary>
        /// 查询自动选票触发条件参数设置(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/par/plate/other"), HttpPost]
        [Description("查询自动选票触发条件参数设置(板块涨跌幅)-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<AccountBuyConditionOtherParInfo> GetAccountAutoTobuyOtherTrendParPlate_Other(GetAccountBuyConditionOtherParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetAccountAutoTobuyOtherTrendParPlate_Other(request, basedata);
        }

        /// <summary>
        /// 添加自动选票触发条件参数设置-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/par/add/other"), HttpPost]
        [Description("添加自动选票触发条件参数设置-额外关系")]
        [CheckUserLoginFilter]
        public object AddAccountAutoTobuyOtherTrendPar_Other(AddAccountBuyConditionOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddAccountAutoTobuyOtherTrendPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 批量添加自动选票触发条件参数设置(板块涨跌幅1)-额外关系
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/auto/tobuy/other/trend/par/add/batch/other"), HttpPost]
        [Description("批量添加自动选票触发条件参数设置(板块涨跌幅1)-额外关系")]
        public async Task<object> BatchAddAccountAutoTobuyOtherTrendPar_Other()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');

                            list.Add(datas[0].Trim());
                        }
                        return trendHandler.BatchAddAccountAutoTobuyOtherTrendPar_Other(Type, RelId, list);
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
        /// 批量添加自动选票触发条件参数设置(板块涨跌幅2)-额外关系
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("account/auto/tobuy/other/trend/par/add/batch2/other"), HttpPost]
        [Description("批量添加自动选票触发条件参数设置(板块涨跌幅2)-额外关系")]
        public async Task<object> BatchAddAccountAutoTobuyOtherTrendPar2_Other()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<BatchAddSharesConditionTrendPar2Obj> list = new List<BatchAddSharesConditionTrendPar2Obj>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 3)
                            {
                                continue;
                            }
                            string groupName = datas[0].Trim();
                            int compare = datas[1].Trim() == ">=" ? 1 : 2;
                            string rate = datas[2].Trim();

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName = groupName,
                                Compare = compare,
                                Rate = rate
                            });
                        }
                        return trendHandler.BatchAddAccountAutoTobuyOtherTrendPar2_Other(Type, RelId, list);
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
        /// 批量删除自动选票触发条件参数设置-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/par/delete/other/batch"), HttpPost]
        [Description("批量删除自动选票触发条件参数设置-额外关系")]
        [CheckUserLoginFilter]
        public object BatchDeleteAccountAutoTobuyOtherTrendPar_Other(BatchDeleteAccountBuyConditionOtherPar_OtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteAccountAutoTobuyOtherTrendPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑自动选票触发条件参数设置-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/par/modify/other"), HttpPost]
        [Description("编辑自动选票触发条件参数设置-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyAccountAutoTobuyOtherTrendPar_Other(ModifyAccountBuyConditionOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyAccountAutoTobuyOtherTrendPar_Other(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除自动选票触发条件参数设置-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("account/auto/tobuy/other/trend/par/delete/other"), HttpPost]
        [Description("删除自动选票触发条件参数设置-额外关系")]
        [CheckUserLoginFilter]
        public object DeleteAccountAutoTobuyOtherTrendPar_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteAccountAutoTobuyOtherTrendPar_Other(request, basedata);
            return null;
        }
        #endregion

        #region===自动加入模板===
        /// <summary>
        /// 获取自动加入模板详情列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/details/list"), HttpPost]
        [Description("获取自动加入模板详情列表")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateJoinDetailsInfo> GetConditiontradeTemplateJoinDetailsList(GetConditiontradeTemplateJoinDetailsListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return trendHandler.GetConditiontradeTemplateJoinDetailsList(request);
        }

        /// <summary>
        /// 添加自动加入模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/details/add"), HttpPost]
        [Description("添加自动加入模板详情")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateJoinDetails(AddConditiontradeTemplateJoinDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            trendHandler.AddConditiontradeTemplateJoinDetails(request);
            return null;
        }

        /// <summary>
        /// 编辑自动加入模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/details/modify"), HttpPost]
        [Description("编辑自动加入模板详情")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateJoinDetails(ModifyConditiontradeTemplateJoinDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            trendHandler.ModifyConditiontradeTemplateJoinDetails(request);
            return null;
        }

        /// <summary>
        /// 修改自动加入模板详情状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/details/status/modify"), HttpPost]
        [Description("修改自动加入模板详情状态")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateJoinDetailsStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            trendHandler.ModifyConditiontradeTemplateJoinDetailsStatus(request);
            return null;
        }

        /// <summary>
        /// 删除自动加入模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/details/delete"), HttpPost]
        [Description("删除自动加入模板详情")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateJoinDetails(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            trendHandler.DeleteConditiontradeTemplateJoinDetails(request);
            return null;
        }

        /// <summary>
        /// 获取自动加入模板额外条件分组列表
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/group/list"), HttpPost]
        [Description("获取自动加入模板额外条件分组列表")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherGroupInfo> GetConditiontradeTemplateJoinOtherGroupList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateJoinOtherGroupList(request);
        }

        /// <summary>
        /// 添加自动加入模板额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/group/add"), HttpPost]
        [Description("添加自动加入模板额外条件分组")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateJoinOtherGroup(AddConditiontradeTemplateBuyOtherGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateJoinOtherGroup(request);
            return null;
        }

        /// <summary>
        /// 编辑自动加入模板额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/group/modify"), HttpPost]
        [Description("编辑自动加入模板额外条件分组")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateJoinOtherGroup(ModifyConditiontradeTemplateBuyOtherGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateJoinOtherGroup(request);
            return null;
        }

        /// <summary>
        /// 修改自动加入模板额外条件分组状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/group/status/modify"), HttpPost]
        [Description("修改自动加入模板额外条件分组状态")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateJoinOtherGroupStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateJoinOtherGroupStatus(request);
            return null;
        }

        /// <summary>
        /// 删除自动加入模板额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/group/delete"), HttpPost]
        [Description("删除自动加入模板额外条件分组")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateJoinOtherGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateJoinOtherGroup(request);
            return null;
        }

        /// <summary>
        /// 获取自动加入模板额外条件列表
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/list"), HttpPost]
        [Description("获取自动加入模板额外条件列表")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherInfo> GetConditiontradeTemplateJoinOtherList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateJoinOtherList(request);
        }

        /// <summary>
        /// 添加自动加入模板额外条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/add"), HttpPost]
        [Description("添加自动加入模板额外条件")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateJoinOther(AddConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateJoinOther(request);
            return null;
        }

        /// <summary>
        /// 编辑自动加入模板额外条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/modify"), HttpPost]
        [Description("编辑自动加入模板额外条件")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateJoinOther(ModifyConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateJoinOther(request);
            return null;
        }

        /// <summary>
        /// 修改自动加入模板额外条件状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/status/modify"), HttpPost]
        [Description("修改自动加入模板额外条件状态")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateJoinOtherStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateJoinOtherStatus(request);
            return null;
        }

        /// <summary>
        /// 删除自动加入模板额外条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/delete"), HttpPost]
        [Description("删除自动加入模板额外条件")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateJoinOther(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateJoinOther(request);
            return null;
        }

        /// <summary>
        /// 查询自动加入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par"), HttpPost]
        [Description("查询自动加入模板额外条件类型参数")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateJoinOtherPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateJoinOtherPar(request);
        }

        /// <summary>
        /// 查询自动加入模板额外条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/plate"), HttpPost]
        [Description("查询自动加入模板额外条件类型参数(板块涨跌幅)")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateJoinOtherParPlate(GetConditiontradeTemplateBuyOtherParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateJoinOtherParPlate(request);
        }

        /// <summary>
        /// 添加自动加入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/add"), HttpPost]
        [Description("添加自动加入模板额外条件类型参数")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateJoinOtherPar(AddConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateJoinOtherPar(request);
            return null;
        }

        /// <summary>
        /// 批量添加自动加入模板额外条件类型参数(板块涨跌幅1)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/join/other/par/add/batch"), HttpPost]
        [Description("批量添加自动加入模板额外条件类型参数(板块涨跌幅1)")]
        public async Task<object> BatchAddConditiontradeTemplateJoinOtherPar()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');

                            list.Add(datas[0].Trim());
                        }
                        return trendHandler.BatchAddConditiontradeTemplateJoinOtherPar(Type, RelId, list);
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
        /// 批量添加自动加入模板额外条件类型参数(板块涨跌幅2)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/join/other/par/add/batch2"), HttpPost]
        [Description("批量添加自动加入模板额外条件类型参数(板块涨跌幅2)")]
        public async Task<object> BatchAddConditiontradeTemplateJoinOtherPar2()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<BatchAddSharesConditionTrendPar2Obj> list = new List<BatchAddSharesConditionTrendPar2Obj>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 3)
                            {
                                continue;
                            }
                            string groupName = datas[0].Trim();
                            int compare = datas[1].Trim() == ">=" ? 1 : 2;
                            string rate = datas[2].Trim();

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName = groupName,
                                Compare = compare,
                                Rate = rate
                            });
                        }
                        return trendHandler.BatchAddConditiontradeTemplateJoinOtherPar2(Type, RelId, list);
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
        /// 批量删除自动加入模板额外条件类型参数（板块涨跌幅）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/delete/batch"), HttpPost]
        [Description("批量删除自动加入模板额外条件类型参数（板块涨跌幅）")]
        [CheckUserLoginFilter]
        public object BatchDeleteConditiontradeTemplateJoinOtherPar(BatchDeleteConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteConditiontradeTemplateJoinOtherPar(request);
            return null;
        }

        /// <summary>
        /// 编辑自动加入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/modify"), HttpPost]
        [Description("编辑自动加入模板额外条件类型参数")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateJoinOtherPar(ModifyConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateJoinOtherPar(request);
            return null;
        }

        /// <summary>
        /// 删除自动加入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/delete"), HttpPost]
        [Description("删除自动加入模板额外条件类型参数")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateJoinOtherPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateJoinOtherPar(request);
            return null;
        }

        /// <summary>
        /// 获取自动加入模板额外条件列表-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/list/other"), HttpPost]
        [Description("获取自动加入模板额外条件列表-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherInfo> GetConditiontradeTemplateJoinOtherList_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateJoinOtherList_Other(request);
        }

        /// <summary>
        /// 添加自动加入模板额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/add/other"), HttpPost]
        [Description("添加自动加入模板额外条件-额外关系")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateJoinOther_Other(AddConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateJoinOther_Other(request);
            return null;
        }

        /// <summary>
        /// 编辑自动加入模板额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/modify/other"), HttpPost]
        [Description("编辑自动加入模板额外条件-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateJoinOther_Other(ModifyConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateJoinOther_Other(request);
            return null;
        }

        /// <summary>
        /// 修改自动加入模板额外条件状态-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/status/modify/other"), HttpPost]
        [Description("修改自动加入模板额外条件状态-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateJoinOtherStatus_Other(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateJoinOtherStatus_Other(request);
            return null;
        }

        /// <summary>
        /// 删除自动加入模板额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/delete/other"), HttpPost]
        [Description("删除自动加入模板额外条件-额外关系")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateJoinOther_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateJoinOther_Other(request);
            return null;
        }

        /// <summary>
        /// 查询自动加入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/other"), HttpPost]
        [Description("查询自动加入模板额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateJoinOtherPar_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateJoinOtherPar_Other(request);
        }

        /// <summary>
        /// 查询自动加入模板额外条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/plate/other"), HttpPost]
        [Description("查询自动加入模板额外条件类型参数(板块涨跌幅)-额外关系")]
        [CheckUserLoginFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateJoinOtherParPlate_Other(GetConditiontradeTemplateBuyOtherParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetConditiontradeTemplateJoinOtherParPlate_Other(request);
        }

        /// <summary>
        /// 添加自动加入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/add/other"), HttpPost]
        [Description("添加自动加入模板额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object AddConditiontradeTemplateJoinOtherPar_Other(AddConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.AddConditiontradeTemplateJoinOtherPar_Other(request);
            return null;
        }

        /// <summary>
        /// 批量添加自动加入模板额外条件类型参数(板块涨跌幅1)-额外关系
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/join/other/par/add/batch/other"), HttpPost]
        [Description("批量添加自动加入模板额外条件类型参数(板块涨跌幅1)-额外关系")]
        public async Task<object> BatchAddConditiontradeTemplateJoinOtherPar_Other()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');

                            list.Add(datas[0].Trim());
                        }
                        return trendHandler.BatchAddConditiontradeTemplateJoinOtherPar_Other(Type, RelId, list);
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
        /// 批量添加自动加入模板额外条件类型参数(板块涨跌幅2)-额外关系
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("conditiontrade/template/join/other/par/add/batch2/other"), HttpPost]
        [Description("批量添加自动加入模板额外条件类型参数(板块涨跌幅2)-额外关系")]
        public async Task<object> BatchAddConditiontradeTemplateJoinOtherPar2_Other()
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
                        var Type = int.Parse(provider.FormData["Type"]);
                        var RelId = long.Parse(provider.FormData["RelId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<BatchAddSharesConditionTrendPar2Obj> list = new List<BatchAddSharesConditionTrendPar2Obj>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 3)
                            {
                                continue;
                            }
                            string groupName = datas[0].Trim();
                            int compare = datas[1].Trim() == ">=" ? 1 : 2;
                            string rate = datas[2].Trim();

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName = groupName,
                                Compare = compare,
                                Rate = rate
                            });
                        }
                        return trendHandler.BatchAddConditiontradeTemplateJoinOtherPar2_Other(Type, RelId, list);
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
        /// 批量删除自动加入模板额外条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/delete/other/batch"), HttpPost]
        [Description("批量删除自动加入模板额外条件类型参数(板块涨跌幅)-额外关系")]
        [CheckUserLoginFilter]
        public object BatchDeleteConditiontradeTemplateJoinOtherPar_Other(BatchDeleteConditiontradeTemplateBuyOtherPar_OtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.BatchDeleteConditiontradeTemplateJoinOtherPar_Other(request);
            return null;
        }

        /// <summary>
        /// 编辑自动加入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/modify/other"), HttpPost]
        [Description("编辑自动加入模板额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object ModifyConditiontradeTemplateJoinOtherPar_Other(ModifyConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.ModifyConditiontradeTemplateJoinOtherPar_Other(request);
            return null;
        }

        /// <summary>
        /// 删除自动加入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/delete/other"), HttpPost]
        [Description("删除自动加入模板额外条件类型参数-额外关系")]
        [CheckUserLoginFilter]
        public object DeleteConditiontradeTemplateJoinOtherPar_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            trendHandler.DeleteConditiontradeTemplateJoinOtherPar_Other(request);
            return null;
        }
        #endregion

        /// <summary>
        /// 股票条件搜索
        /// </summary>
        /// <returns></returns>
        [Description("股票条件搜索")]
        [Route("shares/condition/search"), HttpPost]
        [CheckUserLoginFilter]
        //public List<SharesConditionSearchInfo> GetSharesConditionSearch(GetSharesConditionSearchRequest request)
        public object GetSharesConditionSearch(GetSharesConditionSearchRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return trendHandler.GetSharesConditionSearch(request,basedata);
        }
    }
}
