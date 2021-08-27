using MealTicket_Admin_APIService.Filter;
using MealTicket_Admin_Handler;
using MealTicket_Admin_Handler.Model;
using Ninject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MealTicket_Admin_APIService.controller
{
    [RoutePrefix("tradecenter")]
    public class TradeCenterController : ApiController
    {
        private TradeCenterHandler tradeCenterHandler;

        public TradeCenterController()
        {
            tradeCenterHandler = WebApiManager.Kernel.Get<TradeCenterHandler>();
        }

        #region====行情管理====
        /// <summary>
        /// 获取今日行情股票列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("today/shares/list"), HttpPost]
        [Description("获取今日行情股票列表")]
        public PageRes<TodaySharesInfo> GetTodaySharesList(GetTodaySharesListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetTodaySharesList(request);
        }

        /// <summary>
        /// 添加今日行情股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("today/shares/add"), HttpPost]
        [Description("添加今日行情股票")]
        public object AddTodayShares(AddTodaySharesRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddTodayShares(request);
            return null;
        }

        /// <summary>
        /// 编辑今日行情股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("today/shares/modify"), HttpPost]
        [Description("编辑今日行情股票")]
        public object ModifyTodayShares(ModifyTodaySharesRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifyTodayShares(request);
            return null;
        }

        /// <summary>
        /// 修改今日行情股票状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("today/shares/status/modify"), HttpPost]
        [Description("修改今日行情股票状态")]
        public object ModifyTodaySharesStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifyTodaySharesStatus(request);
            return null;
        }

        /// <summary>
        /// 删除今日行情股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("today/shares/delete"), HttpPost]
        [Description("删除今日行情股票")]
        public object DeleteTodayShares(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteTodayShares(request);
            return null;
        }

        /// <summary>
        /// 获取今日行情股票实时数据
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("today/shares/quotes"), HttpPost]
        [Description("获取今日行情股票实时数据")]
        public List<TodaySharesQuotesInfo> GetTodaySharesQuotes()
        {
            return tradeCenterHandler.GetTodaySharesQuotes();
        }

        /// <summary>
        /// 获取股票热门搜索列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/hot/search"), HttpPost]
        [Description("获取股票热门搜索列表")]
        public PageRes<SharesHotSearchInfo> GetSharesHotSearch(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesHotSearch(request);
        }

        /// <summary>
        /// 获取所有股票列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/list"), HttpPost]
        [Description("获取所有股票列表")]
        public PageRes<SharesInfo> GetSharesList(GetSharesListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesList(request);
        }

        /// <summary>
        /// 修改股票状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/status/modify"), HttpPost]
        [Description("修改股票状态")]
        public object ModifySharesStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesStatus(request);
            return null;
        }

        /// <summary>
        /// 修改股票限制状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/forbidstatus/modify"), HttpPost]
        [Description("修改股票限制状态")]
        public object ModifySharesForbidStatus(ModifySharesForbidStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesForbidStatus(request);
            return null;
        }

        /// <summary>
        /// 修改股票退市状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/marketstatus/modify"), HttpPost]
        [Description("修改股票退市状态")]
        public object ModifySharesMarketStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesMarketStatus(request);
            return null;
        }

        /// <summary>
        /// 获取停牌设置列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("suspension/list"), HttpPost]
        [Description("获取停牌设置列表")]
        public PageRes<SharesSuspensionInfo> GetSharesSuspensionList(GetSharesSuspensionListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesSuspensionList(request);
        }

        /// <summary>
        /// 修改停牌设置状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("suspension/status/modify"), HttpPost]
        [Description("修改停牌设置状态")]
        public object ModifySharesSuspensionStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesSuspensionStatus(request);
            return null;
        }

        /// <summary>
        /// 批量导入停/复牌数据
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("shares/suspensionstatus/batch/modify"), HttpPost]
        [Description("批量导入停/复牌数据")]
        public async Task<object> BatchModifySharesSuspensionStatus()
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
                        var type = int.Parse(provider.FormData["Type"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<SuspensionInfo> sharesList = new List<SuspensionInfo>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if ((datas.Length < 4 && type == 1) || (datas.Length < 2 && type == 2))
                            {
                                continue;
                            }
                            var stock = datas[0].Split('.');
                            if (stock.Count() != 2)
                            {
                                continue;
                            }
                            if (type == 1)
                            {
                                sharesList.Add(new SuspensionInfo
                                {
                                    Market = stock[1] == "SZ" ? 0 : 1,
                                    SharesCode = int.Parse(stock[0]).ToString("000000"),
                                    SharesName = datas[1],
                                    SuspensionStartTime = string.IsNullOrEmpty(datas[2]) ? DateTime.Parse("1900-01-01") : DateTime.Parse(datas[2]),
                                    SuspensionEndTime = string.IsNullOrEmpty(datas[3]) ? DateTime.Parse("9999-12-31") : DateTime.Parse(datas[3])
                                });
                            }
                            else if (type == 2)
                            {
                                sharesList.Add(new SuspensionInfo
                                {
                                    Market = stock[1] == "SZ" ? 0 : 1,
                                    SharesCode = int.Parse(stock[0]).ToString("000000"),
                                });
                            }
                            else
                            {
                                throw new WebApiException(400, "参数错误");
                            }
                        }
                        tradeCenterHandler.BatchModifySharesSuspensionStatus(type, sharesList);
                        return sharesList.Count();
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
        /// 批量导入上市时间数据
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("shares/markettime/batch/modify"), HttpPost]
        [Description("批量导入上市时间数据")]
        public async Task<object> BatchModifySharesMarketTime()
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
                        List<MarketTimeInfo> marketTimeList = new List<MarketTimeInfo>();
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] datas = temp[i].Split(',');
                            if (datas.Length < 9)
                            {
                                continue;
                            }
                            var stock = datas[0].Split('.');
                            if (stock.Count() != 2)
                            {
                                continue;
                            }
                            DateTime MarketTime = DateTime.Parse("1970-01-01 00:00:00");
                            if (!string.IsNullOrEmpty(datas[2]))
                            {
                                try
                                {
                                    int date;
                                    if (!int.TryParse(datas[2], out date))
                                    {
                                        MarketTime = DateTime.Parse(datas[2]);
                                    }
                                    else
                                    {
                                        IFormatProvider ifp = new CultureInfo("zh-CN", true);
                                        MarketTime = DateTime.ParseExact(datas[2], "yyyyMMdd", ifp);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    
                                }
                            }

                            long TotalCapital;
                            if (!long.TryParse(datas[7], out TotalCapital))
                            {
                                TotalCapital = 0;
                            }

                            long CirculatingCapital;
                            if (!long.TryParse(datas[8], out CirculatingCapital))
                            {
                                CirculatingCapital = 0;
                            }

                            try
                            {
                                marketTimeList.Add(new MarketTimeInfo
                                {
                                    Market = stock[1] == "SZ" ? 0 : 1,
                                    SharesCode = int.Parse(stock[0]).ToString("000000"),
                                    SharesName = datas[1],
                                    MarketTime = MarketTime,
                                    Industry = datas[3],
                                    Area = datas[4],
                                    Idea = datas[5],
                                    Business = datas[6],
                                    TotalCapital = long.Parse(datas[7]),
                                    CirculatingCapital = long.Parse(datas[8]),
                                });
                            }
                            catch (Exception ex)
                            { 
                            }
                        }
                        tradeCenterHandler.BatchModifySharesMarketTime(marketTimeList);
                        return marketTimeList.Count();
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
        /// 获取股票分笔明细列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/transactiondata/list"), HttpPost]
        [Description("获取股票分笔明细列表")]
        public PageRes<SharesTransactionDataInfo> GetSharesTransactionDataList(GetSharesTransactionDataListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesTransactionDataList(request);
        }

        /// <summary>
        /// 重置股票分笔明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/transactiondata/reset"), HttpPost]
        [Description("重置股票分笔明细")]
        public object ResetSharesTransactionData(ResetSharesTransactionDataRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ResetSharesTransactionData(request);
            return null;
        }

        /// <summary>
        /// 获取股票勘误列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/corrigendum/list"), HttpPost]
        [Description("获取股票勘误列表")]
        public PageRes<SharesCorrigendumInfo> GetSharesCorrigendumList(GetSharesCorrigendumListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesCorrigendumList(request);
        }

        /// <summary>
        /// 添加股票勘误
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/corrigendum/add"), HttpPost]
        [Description("添加股票勘误")]
        public object AddSharesCorrigendum(AddSharesCorrigendumRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesCorrigendum(request);
            return null;
        }

        /// <summary>
        /// 编辑股票勘误
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/corrigendum/modify"), HttpPost]
        [Description("编辑股票勘误")]
        public object ModifySharesCorrigendum(ModifySharesCorrigendumRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesCorrigendum(request);
            return null;
        }

        /// <summary>
        /// 删除股票勘误
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/corrigendum/delete"), HttpPost]
        [Description("删除股票勘误")]
        public object DeleteSharesCorrigendum(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesCorrigendum(request);
            return null;
        }

        /// <summary>
        /// 获取各类型板块数量
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/count"), HttpPost]
        [Description("获取各类型板块数量")]
        public List<SharesPlateTypeCount> GetSharesPlateTypeCount()
        {
            return tradeCenterHandler.GetSharesPlateTypeCount();
        }

        /// <summary>
        /// 修改类型板块是否包含基础板块
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/isbaseplate/modify"), HttpPost]
        [Description("修改类型板块是否包含基础板块")]
        public object ModifySharesPlateTypeIsBasePlate(ModifyStatusRequest request)
        {
            if (Request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            tradeCenterHandler.ModifySharesPlateTypeIsBasePlate(request);
            return null;
        }

        /// <summary>
        /// 获取板块管理列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/list"), HttpPost]
        [Description("获取板块管理列表")]
        public PageRes<SharesPlateInfo> GetSharesPlateList(GetSharesPlateListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesPlateList(request);
        }

        /// <summary>
        /// 添加板块管理
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/add"), HttpPost]
        [Description("添加板块管理")]
        public object AddSharesPlate(AddSharesPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesPlate(request);
            return null;
        }

        /// <summary>
        /// 批量导入板块数据
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("shares/plate/batch/add"), HttpPost]
        [Description("批量导入板块数据")]
        public async Task<object> BatchAddSharesPlate()
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
                        var type = int.Parse(provider.FormData["Type"]);
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
                            string datas = temp[i];
                            if (datas.Length < 1)
                            {
                                continue;
                            }

                            list.Add(datas);
                        }
                        return tradeCenterHandler.BatchAddSharesPlate(list, type);
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
        /// 编辑板块管理
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/modify"), HttpPost]
        [Description("编辑板块管理")]
        public object ModifySharesPlate(ModifySharesPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesPlate(request);
            return null;
        }

        /// <summary>
        /// 修改板块管理状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/status/modify"), HttpPost]
        [Description("修改板块管理状态")]
        public object ModifySharesPlateStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesPlateStatus(request);
            return null;
        }

        /// <summary>
        /// 修改板块管理基础版块状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/basestatus/modify"), HttpPost]
        [Description("修改板块管理基础版块状态")]
        public object ModifySharesPlateBaseStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesPlateBaseStatus(request);
            return null;
        }

        /// <summary>
        /// 修改板块管理挑选状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/choosestatus/modify"), HttpPost]
        [Description("修改板块管理挑选状态")]
        public object ModifySharesPlateChooseStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesPlateChooseStatus(request);
            return null;
        }

        /// <summary>
        /// 批量修改板块管理挑选状态
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("shares/plate/choosestatus/modify/batch"), HttpPost]
        [Description("批量修改板块管理挑选状态")]
        public async Task<object> ModifySharesPlateChooseStatusBatch()
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
                        var plateType = int.Parse(provider.FormData["PlateType"]);
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
                            string datas = temp[i];
                            if (string.IsNullOrEmpty(datas))
                            {
                                continue;
                            }
                            list.Add(datas);
                        }
                        return tradeCenterHandler.ModifySharesPlateChooseStatusBatch(list, plateType);
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
        /// 批量修改板块管理挑选状态(删除)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/choosestatus/delete/batch"), HttpPost]
        [Description("批量修改板块管理挑选状态(删除)")]
        public object BatchDeleteSharesPlateChooseStatus(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.BatchDeleteSharesPlateChooseStatus(request);
            return null;
        }

        /// <summary>
        /// 批量修改板块管理基础版块状态
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("shares/plate/basestatus/modify/batch"), HttpPost]
        [Description("批量修改板块管理基础版块状态")]
        public async Task<object> ModifySharesPlateBaseStatusBatch()
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
                        var plateType = int.Parse(provider.FormData["PlateType"]);
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
                            string datas = temp[i];
                            if (string.IsNullOrEmpty(datas))
                            {
                                continue;
                            }

                            list.Add(datas);
                        }
                        return tradeCenterHandler.ModifySharesPlateBaseStatusBatch(list, plateType);
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
        /// 批量修改板块管理基础版块状态(删除)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/basestatus/delete/batch"), HttpPost]
        [Description("批量修改板块管理基础版块状态(删除)")]
        public object BatchDeleteSharesPlateBaseStatus(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            tradeCenterHandler.BatchDeleteSharesPlateBaseStatus(request);
            return null;
        }

        /// <summary>
        /// 删除板块管理
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/delete"), HttpPost]
        [Description("删除板块管理")]
        public object DeleteSharesPlate(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesPlate(request);
            return null;
        }

        /// <summary>
        /// 批量删除板块管理
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/delete/batch"), HttpPost]
        [Description("批量删除板块管理")]
        public object BatchDeleteSharesPlate(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.BatchDeleteSharesPlate(request);
            return null;
        }

        /// <summary>
        /// 获取板块股票列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/shares/list"), HttpPost]
        [Description("获取板块股票列表")]
        public PageRes<SharesInfo> GetSharesPlateSharesList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesPlateSharesList(request);
        }

        /// <summary>
        /// 添加板块股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/shares/add"), HttpPost]
        [Description("添加板块股票")]
        public object AddSharesPlateShares(AddSharesPlateSharesRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesPlateShares(request);
            return null;
        }

        /// <summary>
        /// 批量导入板块股票
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("shares/plate/shares/batch/add"), HttpPost]
        [Description("批量导入板块股票")]
        public async Task<object> BatchAddSharesPlateShares()
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
                        var plateId = long.Parse(provider.FormData["PlateId"]);
                        var fileInfo = new FileInfo(file.LocalFileName);
                        var fileStream = fileInfo.OpenRead();
                        int fsLen = (int)fileStream.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fileStream.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.GetEncoding("utf-8").GetString(heByte);
                        string[] temp = myStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<SharesInfo> list = new List<SharesInfo>();
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

                            list.Add(new SharesInfo
                            {
                                Market = stock[1] == "SZ" ? 0 : 1,
                                SharesCode = int.Parse(stock[0]).ToString("000000"),
                                SharesName = datas[1]
                            });
                        }
                        return tradeCenterHandler.BatchAddSharesPlateShares(list, plateId);
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
        /// 删除板块股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/shares/delete"), HttpPost]
        [Description("删除板块股票")]
        public object DeleteSharesPlateShares(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesPlateShares(request);
            return null;
        }

        /// <summary>
        /// 查询板块涨跌幅过滤列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/forbid/list"), HttpPost]
        [Description("查询板块涨跌幅过滤列表")]
        public PageRes<SharesForbidInfo> GetSharesPlateForbidList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesPlateForbidList(request);
        }

        /// <summary>
        /// 添加板块涨跌幅过滤
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/forbid/add"), HttpPost]
        [Description("添加板块涨跌幅过滤")]
        public object AddSharesPlateForbid(AddSharesForbidRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesPlateForbid(request);
            return null;
        }

        /// <summary>
        /// 编辑板块涨跌幅过滤
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/forbid/modify"), HttpPost]
        [Description("编辑板块涨跌幅过滤")]
        public object ModifySharesPlateForbid(ModifySharesForbidRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesPlateForbid(request);
            return null;
        }

        /// <summary>
        /// 删除板块涨跌幅过滤
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/plate/forbid/delete"), HttpPost]
        [Description("删除板块涨跌幅过滤")]
        public object DeleteSharesPlateForbid(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesPlateForbid(request);
            return null;
        }
        #endregion

        #region====交易管理====
        /// <summary>
        /// 查询禁止股票名单列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/forbid/list"), HttpPost]
        [Description("查询禁止股票名单列表")]
        public PageRes<SharesForbidInfo> GetSharesForbidList(GetSharesForbidListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesForbidList(request);
        }

        /// <summary>
        /// 添加禁止股票名单
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/forbid/add"), HttpPost]
        [Description("添加禁止股票名单")]
        public object AddSharesForbid(AddSharesForbidRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesForbid(request);
            return null;
        }

        /// <summary>
        /// 编辑禁止股票名单
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/forbid/modify"), HttpPost]
        [Description("编辑禁止股票名单")]
        public object ModifySharesForbid(ModifySharesForbidRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesForbid(request);
            return null;
        }

        /// <summary>
        /// 删除禁止股票名单
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/forbid/delete"), HttpPost]
        [Description("删除禁止股票名单")]
        public object DeleteSharesForbid(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesForbid(request);
            return null;
        }

        /// <summary>
        /// 查询禁止日期分组列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/date/group/forbid/list"), HttpPost]
        [Description("查询禁止日期分组列表")]
        public PageRes<SharesDateGroupForbidInfo> GetSharesDateGroupForbidList(GetSharesDateGroupForbidListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesDateGroupForbidList(request);
        }

        /// <summary>
        /// 添加禁止日期分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/date/group/forbid/add"), HttpPost]
        [Description("添加禁止日期分组")]
        public object AddSharesDateGroupForbid(AddSharesDateGroupForbidRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesDateGroupForbid(request);
            return null;
        }

        /// <summary>
        /// 编辑禁止日期分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/date/group/forbid/modify"), HttpPost]
        [Description("编辑禁止日期分组")]
        public object ModifySharesDateGroupForbid(ModifySharesDateGroupForbidRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesDateGroupForbid(request);
            return null;
        }

        /// <summary>
        /// 修改禁止日期分组状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/date/group/forbid/status/modify"), HttpPost]
        [Description("修改禁止日期分组状态")]
        public object ModifySharesDateGroupForbidStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesDateGroupForbidStatus(request);
            return null;
        }

        /// <summary>
        /// 删除禁止日期分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/date/group/forbid/delete"), HttpPost]
        [Description("删除禁止日期分组")]
        public object DeleteSharesDateGroupForbid(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesDateGroupForbid(request);
            return null;
        }

        /// <summary>
        /// 查询禁止日期列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/date/forbid/list"), HttpPost]
        [Description("查询禁止日期列表")]
        public PageRes<SharesDateForbidInfo> GetSharesDateForbidList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesDateForbidList(request);
        }

        /// <summary>
        /// 添加禁止日期
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/date/forbid/add"), HttpPost]
        [Description("添加禁止日期")]
        public object AddSharesDateForbid(AddSharesDateForbidRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesDateForbid(request);
            return null;
        }

        /// <summary>
        /// 编辑禁止日期
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/date/forbid/modify"), HttpPost]
        [Description("编辑禁止日期")]
        public object ModifySharesDateForbid(ModifySharesDateForbidRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesDateForbid(request);
            return null;
        }

        /// <summary>
        /// 修改禁止日期状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/date/forbid/status/modify"), HttpPost]
        [Description("修改禁止日期状态")]
        public object ModifySharesDateForbidStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesDateForbidStatus(request);
            return null;
        }

        /// <summary>
        /// 删除禁止日期
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/date/forbid/delete"), HttpPost]
        [Description("删除禁止日期")]
        public object DeleteSharesDateForbid(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesDateForbid(request);
            return null;
        }

        /// <summary>
        /// 查询交易时间分组列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/tradetime/group/list"), HttpPost]
        [Description("查询交易时间分组列表")]
        public PageRes<SharesTradeTimeGroupInfo> GetSharesTradeTimeGroupList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesTradeTimeGroupList(request);
        }

        /// <summary>
        /// 添加交易时间分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/tradetime/group/add"), HttpPost]
        [Description("添加交易时间分组")]
        public object AddSharesTradeTimeGroup(AddSharesTradeTimeGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesTradeTimeGroup(request);
            return null;
        }

        /// <summary>
        /// 编辑交易时间分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/tradetime/group/modify"), HttpPost]
        [Description("编辑交易时间分组")]
        public object ModifySharesTradeTimeGroup(ModifySharesTradeTimeGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesTradeTimeGroup(request);
            return null;
        }

        /// <summary>
        /// 编辑交易时间
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/tradetime/modify"), HttpPost]
        [Description("编辑交易时间")]
        public object ModifySharesTradeTime(ModifySharesTradeTimeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesTradeTime(request);
            return null;
        }

        /// <summary>
        /// 删除交易时间分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/tradetime/group/delete"), HttpPost]
        [Description("删除交易时间分组")]
        public object DeleteSharesTradeTimeGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesTradeTimeGroup(request);
            return null;
        }

        /// <summary>
        /// 查询交易杠杆列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/tradelever/list"), HttpPost]
        [Description("查询交易杠杆列表")]
        public PageRes<SharesTradeLeverInfo> GetSharesTradeLeverList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesTradeLeverList(request);
        }

        /// <summary>
        /// 添加交易杠杆
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/tradelever/add"), HttpPost]
        [Description("添加交易杠杆")]
        public object AddSharesTradeLever(AddSharesTradeLeverRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesTradeLever(request);
            return null;
        }

        /// <summary>
        /// 编辑交易杠杆
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/tradelever/modify"), HttpPost]
        [Description("编辑交易杠杆")]
        public object ModifySharesTradeLever(ModifySharesTradeLeverRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesTradeLever(request);
            return null;
        }

        /// <summary>
        /// 删除交易杠杆
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/tradelever/delete"), HttpPost]
        [Description("删除交易杠杆")]
        public object DeleteSharesTradeLever(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesTradeLever(request);
            return null;
        }

        /// <summary>
        /// 查询平仓规则列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/traderules/list"), HttpPost]
        [Description("查询平仓规则列表")]
        public PageRes<SharesTradeRulesInfo> GetSharesTradeRulesList(GetSharesTradeRulesListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesTradeRulesList(request);
        }

        /// <summary>
        /// 添加平仓规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/traderules/add"), HttpPost]
        [Description("添加平仓规则")]
        public object AddSharesTradeRules(AddSharesTradeRulesRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesTradeRules(request);
            return null;
        }

        /// <summary>
        /// 编辑平仓规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/traderules/modify"), HttpPost]
        [Description("编辑平仓规则")]
        public object ModifySharesTradeRules(ModifySharesTradeRulesRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesTradeRules(request);
            return null;
        }

        /// <summary>
        /// 修改平仓规则状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/traderules/status/modify"), HttpPost]
        [Description("修改平仓规则状态")]
        public object ModifySharesTradeRulesStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesTradeRulesStatus(request);
            return null;
        }

        /// <summary>
        /// 删除平仓规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/traderules/delete"), HttpPost]
        [Description("删除平仓规则")]
        public object DeleteSharesTradeRules(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesTradeRules(request);
            return null;
        }

        /// <summary>
        /// 查询额外平仓规则列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/traderules/other/list"), HttpPost]
        [Description("查询额外平仓规则列表")]
        public PageRes<SharesTradeRulesOtherInfo> GetSharesTradeRulesOtherList(GetSharesTradeRulesOtherListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesTradeRulesOtherList(request);
        }

        /// <summary>
        /// 添加额外平仓规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/traderules/other/add"), HttpPost]
        [Description("添加额外平仓规则")]
        public object AddSharesTradeRulesOther(AddSharesTradeRulesOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesTradeRulesOther(request);
            return null;
        }

        /// <summary>
        /// 编辑额外平仓规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/traderules/other/modify"), HttpPost]
        [Description("编辑额外平仓规则")]
        public object ModifySharesTradeRulesOther(ModifySharesTradeRulesOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesTradeRulesOther(request);
            return null;
        }

        /// <summary>
        /// 修改额外平仓规则状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/traderules/other/status/modify"), HttpPost]
        [Description("修改额外平仓规则状态")]
        public object ModifySharesTradeRulesOtherStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesTradeRulesOtherStatus(request);
            return null;
        }

        /// <summary>
        /// 删除额外平仓规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/traderules/other/delete"), HttpPost]
        [Description("删除额外平仓规则")]
        public object DeleteSharesTradeRulesOther(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesTradeRulesOther(request);
            return null;
        }

        /// <summary>
        /// 查询仓位规则列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/positionrules/list"), HttpPost]
        [Description("查询仓位规则列表")]
        public PageRes<SharesPositionRulesInfo> GetSharesPositionRulesList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesPositionRulesList(request);
        }

        /// <summary>
        /// 添加仓位规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/positionrules/add"), HttpPost]
        [Description("添加仓位规则")]
        public object AddSharesPositionRules(AddSharesPositionRulesRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesPositionRules(request);
            return null;
        }

        /// <summary>
        /// 编辑仓位规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/positionrules/modify"), HttpPost]
        [Description("编辑仓位规则")]
        public object ModifySharesPositionRules(ModifySharesPositionRulesRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesPositionRules(request);
            return null;
        }

        /// <summary>
        /// 修改仓位规则状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/positionrules/status/modify"), HttpPost]
        [Description("修改仓位规则状态")]
        public object ModifySharesPositionRulesStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesPositionRulesStatus(request);
            return null;
        }

        /// <summary>
        /// 删除仓位规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/positionrules/delete"), HttpPost]
        [Description("删除仓位规则")]
        public object DeleteSharesPositionRules(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesPositionRules(request);
            return null;
        }

        /// <summary>
        /// 查询额外仓位规则列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/positionrules/other/list"), HttpPost]
        [Description("查询额外仓位规则列表")]
        public PageRes<SharesPositionRulesOtherInfo> GetSharesPositionRulesOtherList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesPositionRulesOtherList(request);
        }

        /// <summary>
        /// 添加额外仓位规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/positionrules/other/add"), HttpPost]
        [Description("添加额外仓位规则")]
        public object AddSharesPositionRulesOther(AddSharesPositionRulesOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesPositionRulesOther(request);
            return null;
        }

        /// <summary>
        /// 编辑额外仓位规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/positionrules/other/modify"), HttpPost]
        [Description("编辑额外仓位规则")]
        public object ModifySharesPositionRulesOther(ModifySharesPositionRulesOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesPositionRulesOther(request);
            return null;
        }

        /// <summary>
        /// 修改额外仓位规则状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/positionrules/other/status/modify"), HttpPost]
        [Description("修改额外仓位规则状态")]
        public object ModifySharesPositionRulesOtherStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesPositionRulesOtherStatus(request);
            return null;
        }

        /// <summary>
        /// 删除额外仓位规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/positionrules/other/delete"), HttpPost]
        [Description("删除额外仓位规则")]
        public object DeleteSharesPositionRulesOther(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesPositionRulesOther(request);
            return null;
        }

        /// <summary>
        /// 查询风控规则列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/riskrules/list"), HttpPost]
        [Description("查询风控规则列表")]
        public PageRes<SharesRiskRulesInfo> GetSharesRiskRulesList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesRiskRulesList(request);
        }

        /// <summary>
        /// 编辑风控规则
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/riskrules/modify"), HttpPost]
        [Description("编辑风控规则")]
        public object ModifySharesRiskRules(ModifySharesRiskRulesRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesRiskRules(request);
            return null;
        }

        /// <summary>
        /// 修改风控规则禁止状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/riskrules/forbidstatus/modify"), HttpPost]
        [Description("修改风控规则禁止状态")]
        public object ModifySharesRiskRulesForbidStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesRiskRulesForbidStatus(request);
            return null;
        }

        /// <summary>
        /// 修改风控规则状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/riskrules/status/modify"), HttpPost]
        [Description("修改风控规则状态")]
        public object ModifySharesRiskRulesStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesRiskRulesStatus(request);
            return null;
        }
        #endregion

        #region====行情监控====
        /// <summary>
        /// 获取行情监控列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/list"), HttpPost]
        [Description("获取行情监控列表")]
        public PageRes<SharesMonitorInfo> GetSharesMonitorList(GetSharesMonitorListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesMonitorList(request);
        }

        /// <summary>
        /// 添加行情监控
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/add"), HttpPost]
        [Description("添加行情监控")]
        public object AddSharesMonitor(AddSharesMonitorRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesMonitor(request);
            return null;
        }

        /// <summary>
        /// 修改行情监控状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/status/modify"), HttpPost]
        [Description("修改行情监控状态")]
        public object ModifySharesMonitorStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesMonitorStatus(request);
            return null;
        }

        /// <summary>
        /// 删除行情监控
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/delete"), HttpPost]
        [Description("删除行情监控")]
        public object DeleteSharesMonitor(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesMonitor(request);
            return null;
        }

        /// <summary>
        /// 批量删除行情监控
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/delete/batch"), HttpPost]
        [Description("批量删除行情监控")]
        public object BatchDeleteSharesMonitor(BatchDeleteSharesMonitorRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.BatchDeleteSharesMonitor(request);
            return null;
        }

        /// <summary>
        /// 批量导入行情监控数据
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("shares/monitor/batch/add"), HttpPost]
        [Description("批量导入行情监控数据")]
        public async Task<object> BatchAddSharesMonitor()
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
                        return tradeCenterHandler.BatchAddSharesMonitor(marketTimeList);
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
        /// 获取走势模板列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/list"), HttpPost]
        [Description("获取走势模板列表")]
        public PageRes<SharesMonitorTrendInfo> GetSharesMonitorTrendList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesMonitorTrendList(request);
        }

        /// <summary>
        /// 添加走势模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/add"), HttpPost]
        [Description("添加走势模板")]
        public object AddSharesMonitorTrend(AddSharesMonitorTrendRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesMonitorTrend(request);
            return null;
        }

        /// <summary>
        /// 编辑走势模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/modify"), HttpPost]
        [Description("编辑走势模板")]
        public object ModifySharesMonitorTrend(ModifySharesMonitorTrendRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesMonitorTrend(request);
            return null;
        }

        /// <summary>
        /// 修改走势模板状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/status/modify"), HttpPost]
        [Description("修改走势模板状态")]
        public object ModifySharesMonitorTrendStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesMonitorTrendStatus(request);
            return null;
        }

        /// <summary>
        /// 删除走势模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/delete"), HttpPost]
        [Description("删除走势模板")]
        public object DeleteSharesMonitorTrend(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesMonitorTrend(request);
            return null;
        }

        /// <summary>
        /// 获取走势模板参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/par"), HttpPost]
        [Description("获取走势模板参数")]
        public PageRes<SharesMonitorTrendParInfo> GetSharesMonitorTrendPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesMonitorTrendPar(request);
        }

        /// <summary>
        /// 添加走势模板参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/par/add"), HttpPost]
        [Description("添加走势模板参数")]
        public object AddSharesMonitorTrendPar(AddSharesMonitorTrendParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesMonitorTrendPar(request);
            return null;
        }

        /// <summary>
        /// 编辑走势模板参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/par/modify"), HttpPost]
        [Description("编辑走势模板参数")]
        public object ModifySharesMonitorTrendPar(ModifySharesMonitorTrendParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesMonitorTrendPar(request);
            return null;
        }

        /// <summary>
        /// 删除走势模板参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/par/delete"), HttpPost]
        [Description("删除走势模板参数")]
        public object DeleteSharesMonitorTrendPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesMonitorTrendPar(request);
            return null;
        }

        /// <summary>
        /// 获取监控走势关系列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/rel/list"), HttpPost]
        [Description("获取监控走势关系列表")]
        public PageRes<SharesMonitorTrendRelInfo> GetSharesMonitorTrendRelList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesMonitorTrendRelList(request);
        }

        /// <summary>
        /// 添加监控走势关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/rel/add"), HttpPost]
        [Description("添加监控走势关系")]
        public object AddSharesMonitorTrendRel(AddSharesMonitorTrendRelRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesMonitorTrendRel(request);
            return null;
        }

        /// <summary>
        /// 修改监控走势关系状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/rel/status/modify"), HttpPost]
        [Description("修改监控走势关系状态")]
        public object ModifySharesMonitorTrendRelStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesMonitorTrendRelStatus(request);
            return null;
        }

        /// <summary>
        /// 删除监控走势关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/rel/delete"), HttpPost]
        [Description("删除监控走势关系")]
        public object DeleteSharesMonitorTrendRel(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesMonitorTrendRel(request);
            return null;
        }

        /// <summary>
        /// 获取监控走势关系参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/rel/par"), HttpPost]
        [Description("获取监控走势关系参数")]
        public PageRes<SharesMonitorTrendParInfo> GetSharesMonitorTrendRelPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesMonitorTrendRelPar(request);
        }

        /// <summary>
        /// 添加监控走势关系参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/rel/par/add"), HttpPost]
        [Description("添加监控走势关系参数")]
        public object AddSharesMonitorTrendRelPar(AddSharesMonitorTrendRelParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesMonitorTrendRelPar(request);
            return null;
        }

        /// <summary>
        /// 编辑监控走势关系参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/rel/par/modify"), HttpPost]
        [Description("编辑监控走势关系参数")]
        public object ModifySharesMonitorTrendRelPar(ModifySharesMonitorTrendRelParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesMonitorTrendRelPar(request);
            return null;
        }

        /// <summary>
        /// 删除监控走势关系参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/trend/rel/par/delete"), HttpPost]
        [Description("删除监控走势关系参数")]
        public object DeleteSharesMonitorTrendRelPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesMonitorTrendRelPar(request);
            return null;
        }

        /// <summary>
        /// 获取监控股票分笔明细列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/transactiondata/list"), HttpPost]
        [Description("获取监控股票分笔明细列表")]
        public PageRes<SharesTransactionDataInfo> GetSharesMonitorTransactionDataList(GetSharesTransactionDataListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesMonitorTransactionDataList(request);
        }

        /// <summary>
        /// 重置监控股票分笔明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/transactiondata/reset"), HttpPost]
        [Description("重置监控股票分笔明细")]
        public object ResetSharesMonitorTransactionData(ResetSharesTransactionDataRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ResetSharesMonitorTransactionData(request);
            return null;
        }

        /// <summary>
        /// 批量更新监控参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/monitor/par/batchupdate"), HttpPost]
        [Description("批量更新监控参数")]
        public object BatchUpdateMonitorTrendPar(BatchUpdateMonitorTrendParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.BatchUpdateMonitorTrendPar(request);
        }

        /// <summary>
        /// 获取条件模板列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/list"), HttpPost]
        [Description("获取条件模板列表")]
        public PageRes<ConditiontradeTemplateInfo> GetConditiontradeTemplateList(GetConditiontradeTemplateListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateList(request, basedata);
        }

        /// <summary>
        /// 复制条件模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/copy"), HttpPost]
        [Description("复制条件模板")]
        public object CopyConditiontradeTemplate(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.CopyConditiontradeTemplate(request, basedata);
            return null;
        }

        /// <summary>
        /// 添加条件模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/add"), HttpPost]
        [Description("添加条件模板")]
        public object AddConditiontradeTemplate(AddConditiontradeTemplateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplate(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/modify"), HttpPost]
        [Description("编辑条件模板")]
        public object ModifyConditiontradeTemplate(ModifyConditiontradeTemplateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplate(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件模板状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/status/modify"), HttpPost]
        [Description("修改条件模板状态")]
        public object ModifyConditiontradeTemplateStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/delete"), HttpPost]
        [Description("删除条件模板")]
        public object DeleteConditiontradeTemplate(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplate(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取条件卖出模板详情列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/sell/details/list"), HttpPost]
        [Description("获取条件卖出模板详情列表")]
        public List<ConditiontradeTemplateSellDetailsInfo> GetConditiontradeTemplateSellDetailsList(GetConditiontradeTemplateSellDetailsListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateSellDetailsList(request, basedata);
        }

        /// <summary>
        /// 添加条件卖出模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/sell/details/add"), HttpPost]
        [Description("添加条件卖出模板详情")]
        public object AddConditiontradeTemplateSellDetails(AddConditiontradeTemplateSellDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateSellDetails(request, basedata);
            return null;
        }

        /// <summary>
        /// 编辑条件卖出模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/sell/details/modify"), HttpPost]
        [Description("编辑条件卖出模板详情")]
        public object ModifyConditiontradeTemplateSellDetails(ModifyConditiontradeTemplateSellDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateSellDetails(request, basedata);
            return null;
        }

        /// <summary>
        /// 修改条件卖出模板状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/sell/details/status/modify"), HttpPost]
        [Description("修改条件卖出模板状态")]
        public object ModifyConditiontradeTemplateSellDetailsStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateSellDetailsStatus(request, basedata);
            return null;
        }

        /// <summary>
        /// 删除条件卖出模板
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/sell/details/delete"), HttpPost]
        [Description("删除条件卖出模板")]
        public object DeleteConditiontradeTemplateSellDetails(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateSellDetails(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取条件买入模板详情列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/buy/details/list"), HttpPost]
        [Description("获取条件买入模板详情列表")]
        public PageRes<ConditiontradeTemplateBuyDetailsInfo> GetConditiontradeTemplateBuyDetailsList(GetConditiontradeTemplateBuyDetailsListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyDetailsList(request, basedata);
        }

        /// <summary>
        /// 添加条件买入模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/buy/details/add"), HttpPost]
        [Description("添加条件买入模板详情")]
        public object AddConditiontradeTemplateBuyDetails(AddConditiontradeTemplateBuyDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddConditiontradeTemplateBuyDetails(request);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/buy/details/modify"), HttpPost]
        [Description("编辑条件买入模板详情")]
        public object ModifyConditiontradeTemplateBuyDetails(ModifyConditiontradeTemplateBuyDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifyConditiontradeTemplateBuyDetails(request);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板详情状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/buy/details/status/modify"), HttpPost]
        [Description("修改条件买入模板详情状态")]
        public object ModifyConditiontradeTemplateBuyDetailsStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifyConditiontradeTemplateBuyDetailsStatus(request);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/buy/details/delete"), HttpPost]
        [Description("删除条件买入模板详情")]
        public object DeleteConditiontradeTemplateBuyDetails(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteConditiontradeTemplateBuyDetails(request);
            return null;
        }

        /// <summary>
        /// 获取条件买入模板额外条件分组列表
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/group/list"), HttpPost]
        [Description("获取条件买入模板额外条件分组列表")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherGroupInfo> GetConditiontradeTemplateBuyOtherGroupList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyOtherGroupList(request);
        }

        /// <summary>
        /// 添加条件买入模板额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/group/add"), HttpPost]
        [Description("添加条件买入模板额外条件分组")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateBuyOtherGroup(AddConditiontradeTemplateBuyOtherGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateBuyOtherGroup(request);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/group/modify"), HttpPost]
        [Description("编辑股票买入额外条件分组")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyOtherGroup(ModifyConditiontradeTemplateBuyOtherGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyOtherGroup(request);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板额外条件分组状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/group/status/modify"), HttpPost]
        [Description("修改条件买入模板额外条件分组状态")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyOtherGroupStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyOtherGroupStatus(request);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/group/delete"), HttpPost]
        [Description("删除条件买入模板额外条件分组")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateBuyOtherGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateBuyOtherGroup(request);
            return null;
        }

        /// <summary>
        /// 获取条件买入模板转自动条件分组列表
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/group/list"), HttpPost]
        [Description("获取条件买入模板转自动条件分组列表")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyAutoGroupInfo> GetConditiontradeTemplateBuyAutoGroupList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyAutoGroupList(request);
        }

        /// <summary>
        /// 添加条件买入模板转自动条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/group/add"), HttpPost]
        [Description("添加条件买入模板转自动条件分组")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateBuyAutoGroup(AddConditiontradeTemplateBuyAutoGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateBuyAutoGroup(request);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板转自动条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/group/modify"), HttpPost]
        [Description("编辑条件买入模板转自动条件分组")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyAutoGroup(ModifyConditiontradeTemplateBuyAutoGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyAutoGroup(request);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板转自动条件分组状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/group/status/modify"), HttpPost]
        [Description("修改条件买入模板转自动条件分组状态")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyAutoGroupStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyAutoGroupStatus(request);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板转自动条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/group/delete"), HttpPost]
        [Description("删除条件买入模板转自动条件分组")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateBuyAutoGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateBuyAutoGroup(request);
            return null;
        }

        /// <summary>
        /// 获取条件买入模板额外条件列表
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/list"), HttpPost]
        [Description("获取条件买入模板额外条件列表")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherInfo> GetConditiontradeTemplateBuyOtherList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyOtherList(request);
        }

        /// <summary>
        /// 添加条件买入模板额外条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/add"), HttpPost]
        [Description("添加条件买入模板额外条件")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateBuyOther(AddConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateBuyOther(request);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板额外条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/modify"), HttpPost]
        [Description("编辑条件买入模板额外条件")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyOther(ModifyConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyOther(request);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板额外条件状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/status/modify"), HttpPost]
        [Description("修改条件买入模板额外条件状态")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyOtherStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyOtherStatus(request);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板额外条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/delete"), HttpPost]
        [Description("删除条件买入模板额外条件")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateBuyOther(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateBuyOther(request);
            return null;
        }

        /// <summary>
        /// 获取条件买入模板转自动条件列表
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/list"), HttpPost]
        [Description("获取条件买入模板转自动条件列表")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyAutoInfo> GetConditiontradeTemplateBuyAutoList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyAutoList(request);
        }

        /// <summary>
        /// 添加条件买入模板转自动条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/add"), HttpPost]
        [Description("添加条件买入模板转自动条件")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateBuyAuto(AddConditiontradeTemplateBuyAutoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateBuyAuto(request);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板转自动条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/modify"), HttpPost]
        [Description("编辑条件买入模板转自动条件")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyAuto(ModifyConditiontradeTemplateBuyAutoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyAuto(request);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板转自动条件状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/status/modify"), HttpPost]
        [Description("修改条件买入模板转自动条件状态")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyAutoStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyAutoStatus(request);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板转自动条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/delete"), HttpPost]
        [Description("删除条件买入模板转自动条件")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateBuyAuto(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateBuyAuto(request);
            return null;
        }

        /// <summary>
        /// 查询条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par"), HttpPost]
        [Description("查询条件买入模板额外条件类型参数")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateBuyOtherPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyOtherPar(request);
        }

        /// <summary>
        /// 查询条件买入模板额外条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/plate"), HttpPost]
        [Description("查询条件买入模板额外条件类型参数(板块涨跌幅)")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateBuyOtherParPlate(GetConditiontradeTemplateBuyOtherParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyOtherParPlate(request);
        }

        /// <summary>
        /// 添加条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/add"), HttpPost]
        [Description("添加条件买入模板额外条件类型参数")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateBuyOtherPar(AddConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateBuyOtherPar(request);
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
                        return tradeCenterHandler.BatchAddConditiontradeTemplateBuyOtherPar(Type, RelId, list);
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
                        return tradeCenterHandler.BatchAddConditiontradeTemplateBuyOtherPar2(Type, RelId, list);
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
        /// 批量删除条件买入模板额外条件类型参数（板块涨跌幅）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/delete/batch"), HttpPost]
        [Description("批量删除条件买入模板额外条件类型参数（板块涨跌幅）")]
        [CheckUserPowerFilter]
        public object BatchDeleteConditiontradeTemplateBuyOtherPar(BatchDeleteConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.BatchDeleteConditiontradeTemplateBuyOtherPar(request);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/modify"), HttpPost]
        [Description("编辑条件买入模板额外条件类型参数")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyOtherPar(ModifyConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyOtherPar(request);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/delete"), HttpPost]
        [Description("删除条件买入模板额外条件类型参数")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateBuyOtherPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateBuyOtherPar(request);
            return null;
        }

        /// <summary>
        /// 查询条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par"), HttpPost]
        [Description("查询条件买入模板转自动条件类型参数")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyAutoParInfo> GetConditiontradeTemplateBuyAutoPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyAutoPar(request);
        }

        /// <summary>
        /// 查询条件买入模板转自动条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/plate"), HttpPost]
        [Description("查询条件买入模板转自动条件类型参数(板块涨跌幅)")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyAutoParInfo> GetConditiontradeTemplateBuyAutoParPlate(GetConditiontradeTemplateBuyAutoParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyAutoParPlate(request);
        }

        /// <summary>
        /// 添加条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/add"), HttpPost]
        [Description("添加条件买入模板转自动条件类型参数")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateBuyAutoPar(AddConditiontradeTemplateBuyAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateBuyAutoPar(request);
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
                        return tradeCenterHandler.BatchAddConditiontradeTemplateBuyAutoPar(Type, RelId, list);
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
                        return tradeCenterHandler.BatchAddConditiontradeTemplateBuyAutoPar2(Type, RelId, list);
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
        [CheckUserPowerFilter]
        public object BatchDeleteConditiontradeTemplateBuyAutoPar(BatchDeleteConditiontradeTemplateBuyAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.BatchDeleteConditiontradeTemplateBuyAutoPar(request);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/modify"), HttpPost]
        [Description("编辑条件买入模板转自动条件类型参数")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyAutoPar(ModifyConditiontradeTemplateBuyAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyAutoPar(request);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/delete"), HttpPost]
        [Description("删除条件买入模板转自动条件类型参数")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateBuyAutoPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateBuyAutoPar(request);
            return null;
        }

        #region======额外关系==========
        /// <summary>
        /// 获取条件买入模板额外条件列表-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/list/other"), HttpPost]
        [Description("获取条件买入模板额外条件列表-额外关系")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherInfo> GetConditiontradeTemplateBuyOtherList_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyOtherList_Other(request);
        }

        /// <summary>
        /// 添加条件买入模板额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/add/other"), HttpPost]
        [Description("添加条件买入模板额外条件-额外关系")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateBuyOther_Other(AddConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateBuyOther_Other(request);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/modify/other"), HttpPost]
        [Description("编辑条件买入模板额外条件-额外关系")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyOther_Other(ModifyConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyOther_Other(request);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板额外条件状态-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/status/modify/other"), HttpPost]
        [Description("修改条件买入模板额外条件状态-额外关系")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyOtherStatus_Other(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyOtherStatus_Other(request);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/delete/other"), HttpPost]
        [Description("删除条件买入模板额外条件-额外关系")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateBuyOther_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateBuyOther_Other(request);
            return null;
        }

        /// <summary>
        /// 获取条件买入模板转自动条件列表-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/list/other"), HttpPost]
        [Description("获取条件买入模板转自动条件列表-额外关系")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyAutoInfo> GetConditiontradeTemplateBuyAutoList_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyAutoList_Other(request);
        }

        /// <summary>
        /// 添加条件买入模板转自动条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/add/other"), HttpPost]
        [Description("添加条件买入模板转自动条件-额外关系")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateBuyAuto_Other(AddConditiontradeTemplateBuyAutoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateBuyAuto_Other(request);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板转自动条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/modify/other"), HttpPost]
        [Description("编辑条件买入模板转自动条件-额外关系")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyAuto_Other(ModifyConditiontradeTemplateBuyAutoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyAuto_Other(request);
            return null;
        }

        /// <summary>
        /// 修改条件买入模板转自动条件状态-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/status/modify/other"), HttpPost]
        [Description("修改条件买入模板转自动条件状态-额外关系")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyAutoStatus_Other(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyAutoStatus_Other(request);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板转自动条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/delete/other"), HttpPost]
        [Description("删除条件买入模板转自动条件-额外关系")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateBuyAuto_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateBuyAuto_Other(request);
            return null;
        }

        /// <summary>
        /// 查询条件买入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/other"), HttpPost]
        [Description("查询条件买入模板额外条件类型参数-额外关系")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateBuyOtherPar_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyOtherPar_Other(request);
        }

        /// <summary>
        /// 查询条件买入模板额外条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/plate/other"), HttpPost]
        [Description("查询条件买入模板额外条件类型参数(板块涨跌幅)-额外关系")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateBuyOtherParPlate_Other(GetConditiontradeTemplateBuyOtherParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyOtherParPlate_Other(request);
        }

        /// <summary>
        /// 添加条件买入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/add/other"), HttpPost]
        [Description("添加条件买入模板额外条件类型参数-额外关系")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateBuyOtherPar_Other(AddConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateBuyOtherPar_Other(request);
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
                        return tradeCenterHandler.BatchAddConditiontradeTemplateBuyOtherPar_Other(Type, RelId, list);
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
                        return tradeCenterHandler.BatchAddConditiontradeTemplateBuyOtherPar2_Other(Type, RelId, list);
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
        /// 批量删除条件买入模板额外条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/delete/other/batch"), HttpPost]
        [Description("批量删除条件买入模板额外条件类型参数(板块涨跌幅)-额外关系")]
        [CheckUserPowerFilter]
        public object BatchDeleteConditiontradeTemplateBuyOtherPar_Other(BatchDeleteConditiontradeTemplateBuyOtherPar_OtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.BatchDeleteConditiontradeTemplateBuyOtherPar_Other(request);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/modify/other"), HttpPost]
        [Description("编辑条件买入模板额外条件类型参数-额外关系")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyOtherPar_Other(ModifyConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyOtherPar_Other(request);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/other/par/delete/other"), HttpPost]
        [Description("删除条件买入模板额外条件类型参数-额外关系")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateBuyOtherPar_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateBuyOtherPar_Other(request);
            return null;
        }

        /// <summary>
        /// 查询条件买入模板转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/other"), HttpPost]
        [Description("查询条件买入模板转自动条件类型参数-额外关系")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyAutoParInfo> GetConditiontradeTemplateBuyAutoPar_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyAutoPar_Other(request);
        }

        /// <summary>
        /// 查询条件买入模板转自动条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/plate/other"), HttpPost]
        [Description("查询条件买入模板转自动条件类型参数(板块涨跌幅)-额外关系")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyAutoParInfo> GetConditiontradeTemplateBuyAutoParPlate_Other(GetConditiontradeTemplateBuyAutoParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateBuyAutoParPlate_Other(request);
        }

        /// <summary>
        /// 添加条件买入模板转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/add/other"), HttpPost]
        [Description("添加条件买入模板转自动条件类型参数-额外关系")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateBuyAutoPar_Other(AddConditiontradeTemplateBuyAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateBuyAutoPar_Other(request);
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
                        return tradeCenterHandler.BatchAddConditiontradeTemplateBuyAutoPar_Other(Type, RelId, list);
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
                        return tradeCenterHandler.BatchAddConditiontradeTemplateBuyAutoPar2_Other(Type, RelId, list);
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
        [CheckUserPowerFilter]
        public object BatchDeleteConditiontradeTemplateBuyAutoPar_Other(BatchDeleteConditiontradeTemplateBuyAutoPar_OtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.BatchDeleteConditiontradeTemplateBuyAutoPar_Other(request);
            return null;
        }

        /// <summary>
        /// 编辑条件买入模板转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/modify/other"), HttpPost]
        [Description("编辑条件买入模板转自动条件类型参数-额外关系")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateBuyAutoPar_Other(ModifyConditiontradeTemplateBuyAutoParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateBuyAutoPar_Other(request);
            return null;
        }

        /// <summary>
        /// 删除条件买入模板转自动条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/buy/auto/par/delete/other"), HttpPost]
        [Description("删除条件买入模板转自动条件类型参数-额外关系")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateBuyAutoPar_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateBuyAutoPar_Other(request);
            return null;
        }
        #endregion

        /// <summary>
        /// 获取条件单走势模板列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/condition/trend/list"), HttpPost]
        [Description("获取条件单走势模板列表")]
        public PageRes<SharesMonitorTrendInfo> GetSharesConditionTrendList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesConditionTrendList(request);
        }

        /// <summary>
        /// 添加条件单走势模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/condition/trend/add"), HttpPost]
        [Description("添加条件单走势模板")]
        public object AddSharesConditionTrend(AddSharesMonitorTrendRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesConditionTrend(request);
            return null;
        }

        /// <summary>
        /// 编辑条件单走势模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/condition/trend/modify"), HttpPost]
        [Description("编辑条件单走势模板")]
        public object ModifySharesConditionTrend(ModifySharesMonitorTrendRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesConditionTrend(request);
            return null;
        }

        /// <summary>
        /// 修改条件单走势模板状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/condition/trend/status/modify"), HttpPost]
        [Description("修改条件单走势模板状态")]
        public object ModifySharesConditionTrendStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesConditionTrendStatus(request);
            return null;
        }

        /// <summary>
        /// 删除条件单走势模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/condition/trend/delete"), HttpPost]
        [Description("删除条件单走势模板")]
        public object DeleteSharesConditionTrend(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesMonitorTrend(request);
            return null;
        }

        /// <summary>
        /// 获取条件单走势模板参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/condition/trend/par"), HttpPost]
        [Description("获取条件单走势模板参数")]
        public PageRes<SharesMonitorTrendParInfo> GetSharesConditionTrendPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesConditionTrendPar(request);
        }

        /// <summary>
        /// 获取条件单走势模板参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/condition/trend/par/plate"), HttpPost]
        [Description(" 获取条件单走势模板参数(板块涨跌幅)")]
        public PageRes<SharesMonitorTrendParInfo> GetSharesConditionTrendParPlate(GetSharesConditionTrendParPlatePageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesConditionTrendParPlate(request);
        }

        /// <summary>
        /// 添加条件单走势模板参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/condition/trend/par/add"), HttpPost]
        [Description("添加条件单走势模板参数")]
        public object AddSharesConditionTrendPar(AddSharesMonitorTrendParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesConditionTrendPar(request);
            return null;
        }

        /// <summary>
        /// 批量条件单走势模板参数(板块涨跌幅1)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("shares/condition/trend/par/add/batch"), HttpPost]
        [Description("批量条件单走势模板参数(板块涨跌幅1)")]
        public async Task<object> BatchAddSharesConditionTrendPar()
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
                        return tradeCenterHandler.BatchAddSharesConditionTrendPar(Type, list);
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
        /// 批量删除条件单走势模板参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/condition/trend/par/delete/batch"), HttpPost]
        [Description("批量删除条件单走势模板参数(板块涨跌幅1)")]
        public object BatchDeleteSharesConditionTrendPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.BatchDeleteSharesConditionTrendPar(request);
            return null;
        }

        /// <summary>
        /// 批量条件单走势模板参数(板块涨跌幅2)
        /// </summary>
        /// <returns></returns>
        [CheckUserLoginFilter]
        [Route("shares/condition/trend/par/add/batch2"), HttpPost]
        [Description("批量条件单走势模板参数(板块涨跌幅2)")]
        public async Task<object> BatchAddSharesConditionTrendPar2()
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
                            string rate =datas[2].Trim(); 

                            list.Add(new BatchAddSharesConditionTrendPar2Obj
                            {
                                GroupName= groupName,
                                Compare= compare,
                                Rate= rate
                            });
                        }
                        return tradeCenterHandler.BatchAddSharesConditionTrendPar2(Type, list);
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
        /// 编辑条件单走势模板参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/condition/trend/par/modify"), HttpPost]
        [Description("编辑条件单走势模板参数")]
        public object ModifySharesConditionTrendPar(ModifySharesMonitorTrendParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesConditionTrendPar(request);
            return null;
        }

        /// <summary>
        /// 删除条件单走势模板参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/condition/trend/par/delete"), HttpPost]
        [Description("删除条件单走势模板参数")]
        public object DeleteSharesConditionTrendPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesConditionTrendPar(request);
            return null;
        }

        /// <summary>
        /// 获取自动买入限制列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/limit/autobuy/list"), HttpPost]
        [Description("获取自动买入限制列表")]
        public PageRes<SharesLimitAutoBuyInfo> GetSharesLimitAutoBuyList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesLimitAutoBuyList(request);
        }

        /// <summary>
        /// 添加自动买入限制
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/limit/autobuy/add"), HttpPost]
        [Description("添加自动买入限制")]
        public object AddSharesLimitAutoBuy(AddSharesLimitAutoBuyRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesLimitAutoBuy(request);
            return null;
        }

        /// <summary>
        /// 编辑自动买入限制
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/limit/autobuy/modify"), HttpPost]
        [Description("编辑自动买入限制")]
        public object ModifySharesLimitAutoBuy(ModifySharesLimitAutoBuyRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesLimitAutoBuy(request);
            return null;
        }

        /// <summary>
        /// 修改自动买入限制状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/limit/autobuy/status/modify"), HttpPost]
        [Description("修改自动买入限制状态")]
        public object ModifySharesLimitAutoBuyStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesLimitAutoBuyStatus(request);
            return null;
        }

        /// <summary>
        /// 删除自动买入限制
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/limit/autobuy/delete"), HttpPost]
        [Description("删除自动买入限制")]
        public object DeleteSharesLimitAutoBuy(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesLimitAutoBuy(request);
            return null;
        }

        /// <summary>
        /// 获取自动买入优先级列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/priority/autobuy/list"), HttpPost]
        [Description("获取自动买入优先级列表")]
        public PageRes<SharesPriorityAutoBuyInfo> GetSharesPriorityAutoBuyList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesPriorityAutoBuyList(request);
        }

        /// <summary>
        /// 添加自动买入优先级
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/priority/autobuy/add"), HttpPost]
        [Description("添加自动买入优先级")]
        public object AddSharesPriorityAutoBuy(AddSharesPriorityAutoBuyRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesPriorityAutoBuy(request);
            return null;
        }

        /// <summary>
        /// 编辑自动买入优先级
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/priority/autobuy/modify"), HttpPost]
        [Description("编辑自动买入优先级")]
        public object ModifySharesPriorityAutoBuy(ModifySharesPriorityAutoBuyRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifySharesPriorityAutoBuy(request);
            return null;
        }

        /// <summary>
        /// 删除自动买入优先级
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/priority/autobuy/delete"), HttpPost]
        [Description("删除自动买入优先级")]
        public object DeleteSharesPriorityAutoBuy(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesPriorityAutoBuy(request);
            return null;
        }

        /// <summary>
        /// 获取自动买入优先级适用板块列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/priority/autobuy/plate/list"), HttpPost]
        [Description("获取自动买入优先级适用板块列表")]
        public PageRes<SharesPriorityAutoBuyPlateInfo> GetSharesPriorityAutoBuyPlateList(GetSharesPriorityAutoBuyPlateListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesPriorityAutoBuyPlateList(request);
        }

        /// <summary>
        /// 添加自动买入优先级适用板块endregion
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/priority/autobuy/plate/add"), HttpPost]
        [Description("添加自动买入优先级适用板块")]
        public object AddSharesPriorityAutoBuyPlate(AddSharesPriorityAutoBuyPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddSharesPriorityAutoBuyPlate(request);
            return null;
        }

        /// <summary>
        /// 删除自动买入优先级适用板块
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/priority/autobuy/plate/delete"), HttpPost]
        [Description("删除自动买入优先级适用板块")]
        public object DeleteSharesPriorityAutoBuyPlate(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteSharesPriorityAutoBuyPlate(request);
            return null;
        }

        #endregion

        #region===自动加入模板===
        /// <summary>
        /// 获取自动加入模板详情列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/join/details/list"), HttpPost]
        [Description("获取自动加入模板详情列表")]
        public PageRes<ConditiontradeTemplateJoinDetailsInfo> GetConditiontradeTemplateJoinDetailsList(GetConditiontradeTemplateJoinDetailsListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetConditiontradeTemplateJoinDetailsList(request);
        }

        /// <summary>
        /// 添加自动加入模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/join/details/add"), HttpPost]
        [Description("添加自动加入模板详情")]
        public object AddConditiontradeTemplateJoinDetails(AddConditiontradeTemplateJoinDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.AddConditiontradeTemplateJoinDetails(request);
            return null;
        }

        /// <summary>
        /// 编辑自动加入模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/join/details/modify"), HttpPost]
        [Description("编辑自动加入模板详情")]
        public object ModifyConditiontradeTemplateJoinDetails(ModifyConditiontradeTemplateJoinDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifyConditiontradeTemplateJoinDetails(request);
            return null;
        }

        /// <summary>
        /// 修改自动加入模板详情状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/join/details/status/modify"), HttpPost]
        [Description("修改自动加入模板详情状态")]
        public object ModifyConditiontradeTemplateJoinDetailsStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifyConditiontradeTemplateJoinDetailsStatus(request);
            return null;
        }

        /// <summary>
        /// 删除自动加入模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/join/details/delete"), HttpPost]
        [Description("删除自动加入模板详情")]
        public object DeleteConditiontradeTemplateJoinDetails(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.DeleteConditiontradeTemplateJoinDetails(request);
            return null;
        }

        /// <summary>
        /// 获取自动加入模板额外条件分组列表
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/group/list"), HttpPost]
        [Description("获取自动加入模板额外条件分组列表")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherGroupInfo> GetConditiontradeTemplateJoinOtherGroupList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateJoinOtherGroupList(request);
        }

        /// <summary>
        /// 添加自动加入模板额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/group/add"), HttpPost]
        [Description("添加自动加入模板额外条件分组")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateJoinOtherGroup(AddConditiontradeTemplateBuyOtherGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateJoinOtherGroup(request);
            return null;
        }

        /// <summary>
        /// 编辑自动加入模板额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/group/modify"), HttpPost]
        [Description("编辑自动加入模板额外条件分组")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateJoinOtherGroup(ModifyConditiontradeTemplateBuyOtherGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateJoinOtherGroup(request);
            return null;
        }

        /// <summary>
        /// 修改自动加入模板额外条件分组状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/group/status/modify"), HttpPost]
        [Description("修改自动加入模板额外条件分组状态")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateJoinOtherGroupStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateJoinOtherGroupStatus(request);
            return null;
        }

        /// <summary>
        /// 删除自动加入模板额外条件分组
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/group/delete"), HttpPost]
        [Description("删除自动加入模板额外条件分组")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateJoinOtherGroup(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateJoinOtherGroup(request);
            return null;
        }

        /// <summary>
        /// 获取自动加入模板额外条件列表
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/list"), HttpPost]
        [Description("获取自动加入模板额外条件列表")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherInfo> GetConditiontradeTemplateJoinOtherList(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateJoinOtherList(request);
        }

        /// <summary>
        /// 添加自动加入模板额外条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/add"), HttpPost]
        [Description("添加自动加入模板额外条件")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateJoinOther(AddConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateJoinOther(request);
            return null;
        }

        /// <summary>
        /// 编辑自动加入模板额外条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/modify"), HttpPost]
        [Description("编辑自动加入模板额外条件")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateJoinOther(ModifyConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateJoinOther(request);
            return null;
        }

        /// <summary>
        /// 修改自动加入模板额外条件状态
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/status/modify"), HttpPost]
        [Description("修改自动加入模板额外条件状态")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateJoinOtherStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateJoinOtherStatus(request);
            return null;
        }

        /// <summary>
        /// 删除自动加入模板额外条件
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/delete"), HttpPost]
        [Description("删除自动加入模板额外条件")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateJoinOther(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateJoinOther(request);
            return null;
        }

        /// <summary>
        /// 查询自动加入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par"), HttpPost]
        [Description("查询自动加入模板额外条件类型参数")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateJoinOtherPar(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateJoinOtherPar(request);
        }

        /// <summary>
        /// 查询自动加入模板额外条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/plate"), HttpPost]
        [Description("查询自动加入模板额外条件类型参数(板块涨跌幅)")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateJoinOtherParPlate(GetConditiontradeTemplateBuyOtherParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateJoinOtherParPlate(request);
        }

        /// <summary>
        /// 添加自动加入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/add"), HttpPost]
        [Description("添加自动加入模板额外条件类型参数")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateJoinOtherPar(AddConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateJoinOtherPar(request);
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
                        return tradeCenterHandler.BatchAddConditiontradeTemplateJoinOtherPar(Type, RelId, list);
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
                        return tradeCenterHandler.BatchAddConditiontradeTemplateJoinOtherPar2(Type, RelId, list);
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
        [CheckUserPowerFilter]
        public object BatchDeleteConditiontradeTemplateJoinOtherPar(BatchDeleteConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.BatchDeleteConditiontradeTemplateJoinOtherPar(request);
            return null;
        }

        /// <summary>
        /// 编辑自动加入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/modify"), HttpPost]
        [Description("编辑自动加入模板额外条件类型参数")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateJoinOtherPar(ModifyConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateJoinOtherPar(request);
            return null;
        }

        /// <summary>
        /// 删除自动加入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/delete"), HttpPost]
        [Description("删除自动加入模板额外条件类型参数")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateJoinOtherPar(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateJoinOtherPar(request);
            return null;
        }

        /// <summary>
        /// 获取自动加入模板额外条件列表-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/list/other"), HttpPost]
        [Description("获取自动加入模板额外条件列表-额外关系")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherInfo> GetConditiontradeTemplateJoinOtherList_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateJoinOtherList_Other(request);
        }

        /// <summary>
        /// 添加自动加入模板额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/add/other"), HttpPost]
        [Description("添加自动加入模板额外条件-额外关系")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateJoinOther_Other(AddConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateJoinOther_Other(request);
            return null;
        }

        /// <summary>
        /// 编辑自动加入模板额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/modify/other"), HttpPost]
        [Description("编辑自动加入模板额外条件-额外关系")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateJoinOther_Other(ModifyConditiontradeTemplateBuyOtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateJoinOther_Other(request);
            return null;
        }

        /// <summary>
        /// 修改自动加入模板额外条件状态-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/status/modify/other"), HttpPost]
        [Description("修改自动加入模板额外条件状态-额外关系")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateJoinOtherStatus_Other(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateJoinOtherStatus_Other(request);
            return null;
        }

        /// <summary>
        /// 删除自动加入模板额外条件-额外关系
        /// </summary>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/delete/other"), HttpPost]
        [Description("删除自动加入模板额外条件-额外关系")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateJoinOther_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateJoinOther_Other(request);
            return null;
        }

        /// <summary>
        /// 查询自动加入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/other"), HttpPost]
        [Description("查询自动加入模板额外条件类型参数-额外关系")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateJoinOtherPar_Other(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateJoinOtherPar_Other(request);
        }

        /// <summary>
        /// 查询自动加入模板额外条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/plate/other"), HttpPost]
        [Description("查询自动加入模板额外条件类型参数(板块涨跌幅)-额外关系")]
        [CheckUserPowerFilter]
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateJoinOtherParPlate_Other(GetConditiontradeTemplateBuyOtherParPlateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return tradeCenterHandler.GetConditiontradeTemplateJoinOtherParPlate_Other(request);
        }

        /// <summary>
        /// 添加自动加入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/add/other"), HttpPost]
        [Description("添加自动加入模板额外条件类型参数-额外关系")]
        [CheckUserPowerFilter]
        public object AddConditiontradeTemplateJoinOtherPar_Other(AddConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.AddConditiontradeTemplateJoinOtherPar_Other(request);
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
                        return tradeCenterHandler.BatchAddConditiontradeTemplateJoinOtherPar_Other(Type, RelId, list);
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
                        return tradeCenterHandler.BatchAddConditiontradeTemplateJoinOtherPar2_Other(Type, RelId, list);
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
        [CheckUserPowerFilter]
        public object BatchDeleteConditiontradeTemplateJoinOtherPar_Other(BatchDeleteConditiontradeTemplateBuyOtherPar_OtherRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.BatchDeleteConditiontradeTemplateJoinOtherPar_Other(request);
            return null;
        }

        /// <summary>
        /// 编辑自动加入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/modify/other"), HttpPost]
        [Description("编辑自动加入模板额外条件类型参数-额外关系")]
        [CheckUserPowerFilter]
        public object ModifyConditiontradeTemplateJoinOtherPar_Other(ModifyConditiontradeTemplateBuyOtherParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.ModifyConditiontradeTemplateJoinOtherPar_Other(request);
            return null;
        }

        /// <summary>
        /// 删除自动加入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("conditiontrade/template/join/other/par/delete/other"), HttpPost]
        [Description("删除自动加入模板额外条件类型参数-额外关系")]
        [CheckUserPowerFilter]
        public object DeleteConditiontradeTemplateJoinOtherPar_Other(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            tradeCenterHandler.DeleteConditiontradeTemplateJoinOtherPar_Other(request);
            return null;
        }
        #endregion

        #region===搜索模板===
        /// <summary>
        /// 获取搜索模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/search/details"), HttpPost]
        [Description("获取搜索模板详情")]
        public object GetConditiontradeTemplateSearchDetails(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetConditiontradeTemplateSearchDetails(request);
        }

        /// <summary>
        /// 编辑搜索模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("conditiontrade/template/search/details/modify"), HttpPost]
        [Description("编辑搜索模板详情")]
        public object ModifyConditiontradeTemplateSearchDetails(ModifyConditiontradeTemplateSearchDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ModifyConditiontradeTemplateSearchDetails(request);
            return null;
        }
        #endregion


        /// <summary>
        /// 获取k线数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/kline/list"), HttpPost]
        [Description("获取k线数据")]
        public PageRes<SharesKlineInfo> GetSharesKlineList(GetSharesKlineListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return tradeCenterHandler.GetSharesKlineList(request);
        }

        /// <summary>
        /// 重置K线数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("shares/kline/reset"), HttpPost]
        [Description("重置K线数据")]
        public object ResetSharesKLine(ResetSharesKLineRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            tradeCenterHandler.ResetSharesKLine(request);
            return null;
        }
    }
}
