using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace MealTicket_APIService.controller
{
    public class NoticeSendController : ApiController
    {
        /// <summary>
        /// 到达警戒线通知
        /// </summary>
        /// <returns></returns>
        [Description("到达警戒线通知")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        public object OverCordon()
        {
            return null;
        }

        /// <summary>
        /// 即将强制平仓通知
        /// </summary>
        /// <returns></returns>
        [Description("即将强制平仓通知")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        public object ClosingForceSoon()
        {
            return null;
        }

        /// <summary>
        /// 发生强制平仓通知
        /// </summary>
        /// <returns></returns>
        [Description("发生强制平仓通知")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        public object ClosingForce()
        {
            return null;
        }

        /// <summary>
        /// 发生自动平仓通知
        /// </summary>
        /// <returns></returns>
        [Description("发生自动平仓通知")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        public object ClosingAuto()
        {
            return null;
        }

        /// <summary>
        /// 股票买入委托结束通知
        /// </summary>
        /// <returns></returns>
        [Description("股票买入委托结束通知")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        public object BuySharesFinish()
        {
            return null;
        }

        /// <summary>
        /// 股票卖出委托结束通知
        /// </summary>
        /// <returns></returns>
        [Description("股票卖出委托结束通知")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        public object SellSharesFinish()
        {
            return null;
        }

        /// <summary>
        /// 提现成功通知
        /// </summary>
        /// <returns></returns>
        [Description("提现成功通知")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        public object CashSuccess()
        {
            return null;
        }

        /// <summary>
        /// 提现失败通知
        /// </summary>
        /// <returns></returns>
        [Description("提现失败通知")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        public object CashFail()
        {
            return null;
        }

        /// <summary>
        /// 拒绝提现通知
        /// </summary>
        /// <returns></returns>
        [Description("拒绝提现通知")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [OverrideAuthorization]
        [OverrideActionFilters]
        public object CashReject()
        {
            return null;
        }
    }
}
