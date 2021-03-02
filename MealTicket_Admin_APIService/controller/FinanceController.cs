using MealTicket_Admin_APIService.Filter;
using MealTicket_Admin_Handler;
using MealTicket_Admin_Handler.Model;
using Ninject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MealTicket_Admin_APIService.controller
{
    [RoutePrefix("finance")]
    public class FinanceController:ApiController
    {
        private FinanceHandler financeHandler;

        public FinanceController()
        {
            financeHandler = WebApiManager.Kernel.Get<FinanceHandler>();
        }

        #region====经营管理====
        /// <summary>
        /// 查询平台收支情况
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("platform/wallet"),HttpPost]
        [Description("查询平台收支情况")]
        public PageRes<PlatformWallet> GetPlatformWallet(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            return financeHandler.GetPlatformWallet(request);
        }

        /// <summary>
        /// 查询充值记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("recharge/record/list"), HttpPost]
        [Description("查询充值记录")]
        public PageRes<RechargeRecordInfo> GetRechargeRecordList(GetRechargeRecordListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return financeHandler.GetRechargeRecordList(request);
        }
        #endregion


        #region====提现管理====
        /// <summary>
        /// 查询提现记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("cash/record/list"), HttpPost]
        [Description("查询提现记录")]
        public PageRes<CashRecordInfo> GetCashRecordList(GetCashRecordListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return financeHandler.GetCashRecordList(request);
        }

        /// <summary>
        /// 开始处理提现
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("cash/starthandle"), HttpPost]
        [Description("开始处理提现")]
        public object StartCashHandle(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            financeHandler.StartCashHandle(request);
            return null;
        }

        /// <summary>
        /// 撤销处理提现
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("cash/cancelhandle"), HttpPost]
        [Description("撤销处理提现")]
        public object CancelCashHandle(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            financeHandler.CancelCashHandle(request);
            return null;
        }

        /// <summary>
        /// 结束提现
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("cash/finish"), HttpPost]
        [Description("结束提现")]
        public object FinishCash(FinishCashRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            financeHandler.FinishCash(request, basedata);
            return null;
        }

        /// <summary>
        /// 开始提现
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("cash/start"), HttpPost]
        [Description("开始提现")]
        public object StartCash(StartCashRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            financeHandler.StartCash(request, basedata);
            return null;
        }

        /// <summary>
        /// 查询提现详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("cash/record/details"), HttpPost]
        [Description("查询提现详情")]
        public CashRecordDetails GetCashRecordDetails(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return financeHandler.GetCashRecordDetails(request);
        }

        /// <summary>
        /// 查询转账记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("cash/transfer/record"), HttpPost]
        [Description("查询转账记录")]
        public PageRes<CashTransferRecord> GetCashTransferRecord(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return financeHandler.GetCashTransferRecord(request);
        }

        /// <summary>
        /// 查询转账详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("cash/transfer/details"), HttpPost]
        [Description("查询转账详情")]
        public List<CashDetails> GetCashTransferDetails(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return financeHandler.GetCashTransferDetails(request);
        }

        /// <summary>
        /// 查询提现打款账户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("cash/account/list"), HttpPost]
        [Description("查询提现打款账户列表")]
        public List<CashAccountInfo> GetCashAccountList(GetCashAccountListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return financeHandler.GetCashAccountList(request);
        }
        #endregion
    }
}
