using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using MealTicket_APIService.Filter;
using MealTicket_Handler;
using MealTicket_Handler.Model;
using Ninject;

namespace MealTicket_APIService.controller
{
    [RoutePrefix("trade")]
    public class TradeController:ApiController
    {
        private TradeHandler sharesHandler;

        public TradeController()
        {
            sharesHandler = WebApiManager.Kernel.Get<TradeHandler>();
        }

        /// <summary>
        /// 根据股票代码获取股票列表
        /// </summary>
        /// <returns></returns>
        [Description("根据股票代码获取股票列表")]
        [Route("shares/list"), HttpPost]
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
            return sharesHandler.GetTradeSharesList(request);
        }

        /// <summary>
        /// 添加热门搜索结果
        /// </summary>
        /// <returns></returns>
        [Description("添加热门搜索结果")]
        [Route("shares/hotsearch"), HttpPost]
        public object AddTradeSharesHotSearch(AddTradeSharesHotSearchRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            sharesHandler.AddTradeSharesHotSearch(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询热门搜索结果
        /// </summary>
        /// <returns></returns>
        [Description("查询热门搜索结果")]
        [Route("shares/hotsearch/query"), HttpPost]
        public List<TradeSharesInfo> QueryTradeSharesHotSearch()
        {
            return sharesHandler.QueryTradeSharesHotSearch();
        }

        /// <summary>
        /// 获取某只股票五档数据
        /// </summary>
        /// <returns></returns>
        [Description("获取某只股票五档数据")]
        [Route("shares/quotes"), HttpPost]
        public TradeSharesQuotesInfo GetTradeSharesQuotesInfo(GetTradeSharesQuotesInfoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return sharesHandler.GetTradeSharesQuotesInfo(request,basedata);
        }

        /// <summary>
        /// 获取某只股票分笔成交数据
        /// </summary>
        /// <returns></returns>
        [Description("获取某只股票分笔成交数据")]
        [Route("shares/transactiondata"), HttpPost]
        public GetTradeSharesTransactionDataListRes GetTradeSharesTransactionDataList(GetTradeSharesTransactionDataListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sharesHandler.GetTradeSharesTransactionDataList(request);
        }

        /// <summary>
        /// 获取交易账户信息
        /// </summary>
        /// <returns></returns>
        [Description("获取交易账户信息")]
        [Route("account/info"), HttpPost]
        [CheckUserLoginFilter]
        public TradeAccountInfo GetTradeAccountInfo()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return sharesHandler.GetTradeAccountInfo(basedata);
        }

        /// <summary>
        /// 获取持仓列表
        /// </summary>
        /// <returns></returns>
        [Description("获取持仓列表")]
        [Route("hold/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<TradeHoldInfo> GetTradeHoldList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return sharesHandler.GetTradeHoldList(request,basedata);
        }

        /// <summary>
        /// 获取交易委托列表
        /// </summary>
        /// <returns></returns>
        [Description("获取交易委托列表")]
        [Route("entrust/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<TradeEntrustInfo> GetTradeEntrustList(GetTradeEntrustListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return sharesHandler.GetTradeEntrustList(request, basedata);
        }

        /// <summary>
        /// 获取交易成交记录列表
        /// </summary>
        /// <returns></returns>
        [Description("获取交易成交记录列表")]
        [Route("entrust/dealdetails/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<TradeEntrustDealDetailsInfo> GetTradeEntrustDealDetailsList(GetTradeEntrustDealDetailsListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return sharesHandler.GetTradeEntrustDealDetailsList(request, basedata);
        }

        /// <summary>
        /// 设置自动平仓时间
        /// </summary>
        /// <returns></returns>
        [Description("设置自动平仓时间")]
        [Route("closing/time/set"), HttpPost]
        [CheckUserLoginFilter]
        public object SetTradeClosingTime(SetTradeClosingTimeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            sharesHandler.SetTradeClosingTime(request, basedata);
            return null;
        }

        /// <summary>
        /// 补交保证金
        /// </summary>
        /// <returns></returns>
        [Description("补交保证金")]
        [Route("deposit/makeup"), HttpPost]
        [CheckUserLoginFilter]
        public object MakeupDeposit(MakeupDepositRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            sharesHandler.MakeupDeposit(request, basedata);
            return null;
        }

        /// <summary>
        /// 股票申请买入
        /// </summary>
        /// <returns></returns>
        [Description("股票申请买入")]
        [Route("buy"), HttpPost]
        [CheckUserLoginFilter]
        public object ApplyTradeBuy(ApplyTradeBuyRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            sharesHandler.ApplyTradeBuy(request, basedata);
            return null;
        }

        /// <summary>
        /// 股票申请卖出
        /// </summary>
        /// <returns></returns>
        [Description("股票申请卖出")]
        [Route("sell"), HttpPost]
        [CheckUserLoginFilter]
        public object ApplyTradeSell(ApplyTradeSellRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            sharesHandler.ApplyTradeSell(request, basedata);
            return null;
        }

        /// <summary>
        /// 股票申请撤单
        /// </summary>
        /// <returns></returns>
        [Description("股票申请撤单")]
        [Route("cancel"), HttpPost]
        [CheckUserLoginFilter]
        public object ApplyTradeCancel(ApplyTradeCancelRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            sharesHandler.ApplyTradeCancel(request, basedata);
            return null;
        }

        /// <summary>
        /// 获取用户交易杠杆倍数
        /// </summary>
        /// <returns></returns>
        [Description("获取用户交易杠杆倍数")]
        [Route("fundmultiple/list"), HttpPost]
        [CheckUserLoginFilter]
        public List<FundmultipleInfo> GetFundmultipleList()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return sharesHandler.GetFundmultipleList(basedata);
        }

        /// <summary>
        /// 修改用户交易杠杆倍数
        /// </summary>
        /// <returns></returns>
        [Description("修改用户交易杠杆倍数")]
        [Route("fundmultiple/modify"), HttpPost]
        [CheckUserLoginFilter]
        public object ModifyFundmultiple(ModifyFundmultipleRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            sharesHandler.ModifyFundmultiple(request,basedata);
            return null;
        }

        /// <summary>
        /// 获取用户跟投信息
        /// </summary>
        /// <returns></returns>
        [Description("获取用户跟投信息")]
        [Route("account/follow/info"), HttpPost]
        [CheckUserLoginFilter]
        public AccountFollowInfo GetAccountFollowInfo()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return sharesHandler.GetAccountFollowInfo(basedata);
        }

        /// <summary>
        /// 用户申请跟投
        /// </summary>
        /// <returns></returns>
        [Description("用户申请跟投")]
        [Route("account/follow/apply"), HttpPost]
        [CheckUserLoginFilter]
        public object ApplyFollow()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            sharesHandler.ApplyFollow(basedata);
            return null;
        }

        /// <summary>
        /// 用户取消跟投
        /// </summary>
        /// <returns></returns>
        [Description("用户取消跟投")]
        [Route("account/follow/cancel"), HttpPost]
        [CheckUserLoginFilter]
        public object CancelFollow()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            sharesHandler.CancelFollow(basedata);
            return null;
        }

        /// <summary>
        /// 获取用户历史持仓
        /// </summary>
        /// <returns></returns>
        [Description("获取用户历史持仓")]
        [Route("account/hold/history/list"), HttpPost]
        [CheckUserLoginFilter]
        public GetAccountHoldHistoryListRes GetAccountHoldHistoryList(GetAccountHoldHistoryListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return sharesHandler.GetAccountHoldHistoryList(request,basedata);
        }
    }
}
