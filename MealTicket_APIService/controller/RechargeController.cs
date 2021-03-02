using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using MealTicket_APIService.Filter;
using MealTicket_Handler.BusinessHandler;
using MealTicket_Handler.Model;
using Ninject;

namespace MealTicket_APIService.controller
{
    [RoutePrefix("recharge")]
    public class RechargeController : ApiController
    {
        private RechargeHandler rechargeHandler;

        public RechargeController()
        {
            rechargeHandler = WebApiManager.Kernel.Get<RechargeHandler>();
        }

        /// <summary>
        /// 获取充值支付渠道分组
        /// </summary>
        /// <returns></returns>
        [Description("获取充值支付渠道分组")]
        [Route("paychannel/group"), HttpPost]
        [CheckUserLoginFilter]
        public List<RechargePaychannelGroupInfo> GetRechargePaychannelGroup()
        {
            return rechargeHandler.GetRechargePaychannelGroup();
        }

        /// <summary>
        /// 获取充值在线支付渠道列表
        /// </summary>
        /// <returns></returns>
        [Description("获取充值在线支付渠道列表")]
        [Route("paychannel/online"), HttpPost]
        [CheckUserLoginFilter]
        public List<RechargePaychannelOnlineInfo> GetRechargePaychannelOnline()
        {
            return rechargeHandler.GetRechargePaychannelOnline();
        }

        /// <summary>
        /// 获取充值线下卡卡支付信息
        /// </summary>
        /// <returns></returns>
        [Description("获取充值卡卡支付信息")]
        [Route("payinfo/offline"), HttpPost]
        [CheckUserLoginFilter]
        public RechargePayOflineInfo GetRechargePayOfflineInfo()
        {
            return rechargeHandler.GetRechargePayOfflineInfo();
        }

        /// <summary>
        /// 开始充值
        /// </summary>
        /// <returns></returns>
        [Description("开始充值")]
        [Route("startpay"), HttpPost]
        [CheckRealNameFilter]
        public object StartRechargePay(StartRechargePayRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return rechargeHandler.StartRechargePay(request, basedata);
        }

        /// <summary>
        /// 查询充值支付是否成功
        /// </summary>
        /// <returns></returns>
        [Description("查询充值支付是否成功")]
        [Route("pay/success"), HttpPost]
        [CheckUserLoginFilter]
        public PayIsSuccess RechargePayIsSuccess(RechargePayIsSuccessRequest request)
        {
            if (request==null)
            {
                throw new WebApiException(400, "参数错误");
            }

            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return rechargeHandler.RechargePayIsSuccess(request, basedata);
        }

        /// <summary>
        /// 查询充值记录列表
        /// </summary>
        /// <returns></returns>
        [Description("查询充值记录列表")]
        [Route("record"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<RechargeRecordInfo> GetRechargeRecord(GetRechargeRecordRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }

            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return rechargeHandler.GetRechargeRecord(request, basedata);
        }
    }
}
