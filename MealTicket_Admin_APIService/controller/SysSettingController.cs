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
    [RoutePrefix("sys")]
    public class SysSettingController : ApiController
    {
        private SysSettingHandler sysSettingHandler;

        public SysSettingController()
        {
            sysSettingHandler = WebApiManager.Kernel.Get<SysSettingHandler>();
        }

        #region====参数管理====
        /// <summary>
        /// 查询系统参数列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("par/list"), HttpPost]
        [Description("查询系统参数列表")]
        public PageRes<SysParInfo> GetSysParList(GetSysParListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetSysParList(request);
        }

        /// <summary>
        /// 编辑系统参数
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("par/modify"), HttpPost]
        [Description("编辑系统参数")]
        public object ModifySysPar(ModifySysParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySysPar(request);
            return null;
        }

        /// <summary>
        /// 查询app版本列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("appversion/list"), HttpPost]
        [Description("查询app版本列表")]
        public PageRes<SysAppversionInfo> GetSysAppversionList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetSysAppversionList(request);
        }

        /// <summary>
        /// 添加app版本
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("appversion/add"), HttpPost]
        [Description("添加app版本")]
        public object AddSysAppversion(AddSysAppversionRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddSysAppversion(request);
            return null;
        }

        /// <summary>
        /// 编辑app版本
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("appversion/modify"), HttpPost]
        [Description("编辑app版本")]
        public object ModifySysAppversion(ModifySysAppversionRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySysAppversion(request);
            return null;
        }

        /// <summary>
        /// 删除app版本
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("appversion/delete"), HttpPost]
        [Description("删除app版本")]
        public object DeleteSysAppversion(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeleteSysAppversion(request);
            return null;
        }
        #endregion

        #region====股票账号====
        /// <summary>
        /// 查询服务器列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("server/list"), HttpPost]
        [Description("查询服务器列表")]
        public PageRes<ServerInfo> GetServerList(GetServerListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetServerList(request);
        }

        /// <summary>
        /// 添加服务器
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("server/add"), HttpPost]
        [Description("添加服务器")]
        public object AddServer(AddServerRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddServer(request);
            return null;
        }

        /// <summary>
        /// 编辑服务器
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("server/modify"), HttpPost]
        [Description("编辑服务器")]
        public object ModifyServer(ModifyServerRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyServer(request);
            return null;
        }

        /// <summary>
        /// 删除服务器
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("server/delete"), HttpPost]
        [Description("删除服务器")]
        public object DeleteServer(DeleteServerRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeleteServer(request);
            return null;
        }

        /// <summary>
        /// 绑定服务器账户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("server/account/bind"), HttpPost]
        [Description("绑定服务器账户")]
        public object BindServerAccount(BindServerAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.BindServerAccount(request);
            return null;
        }

        /// <summary>
        /// 查询券商账户列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/account/list")]
        [Description("查询券商账户列表"), HttpPost]
        public PageRes<BrokerAccountInfo> GetBrokerAccountList(GetBrokerAccountListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetBrokerAccountList(request);
        }

        /// <summary>
        /// 添加券商账户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/account/add"), HttpPost]
        [Description("添加券商账户")]
        public object AddBrokerAccount(AddBrokerAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddBrokerAccount(request);
            return null;
        }

        /// <summary>
        /// 编辑券商账户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/account/modify"), HttpPost]
        [Description("编辑券商账户")]
        public object ModifyBrokerAccount(ModifyBrokerAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyBrokerAccount(request);
            return null;
        }

        /// <summary>
        /// 修改券商账户状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/account/status/modify"), HttpPost]
        [Description("修改券商账户状态")]
        public object ModifyBrokerAccountStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyBrokerAccountStatus(request);
            return null;
        }

        /// <summary>
        /// 删除券商账户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/account/delete"), HttpPost]
        [Description("删除券商账户")]
        public object DeleteBrokerAccount(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeleteBrokerAccount(request);
            return null;
        }

        /// <summary>
        /// 查询券商账户持仓信息
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/account/position/info")]
        [Description(" 查询券商账户持仓/资金信息"), HttpPost]
        public GetBrokerAccountPositionInfoRes GetBrokerAccountPositionInfo(GetBrokerAccountPositionInfoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetBrokerAccountPositionInfo(request);
        }

        /// <summary>
        /// 同步券商账户持仓信息
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/account/position/update")]
        [Description(" 同步券商账户持仓信息/资金信息"), HttpPost]
        public object UpdateBrokerAccountPositionInfo(UpdateBrokerAccountPositionInfoRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.UpdateBrokerAccountPositionInfo(request);
            return null;
        }

        /// <summary>
        /// 查询券商账户系统持仓信息
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/account/sys/position/info"), HttpPost]
        [Description("查询券商账户系统持仓信息")]
        public PageRes<BrokerAccountSysPositionInfo> GetBrokerAccountSysPositionInfo(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetBrokerAccountSysPositionInfo(request);
        }

        /// <summary>
        /// 查询券商账户系统持仓详情
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/account/sys/position/details"), HttpPost]
        [Description("查询券商账户系统持仓详情")]
        public PageRes<BrokerAccountSysPositionDetails> GetBrokerAccountSysPositionDetails(GetBrokerAccountSysPositionDetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetBrokerAccountSysPositionDetails(request);
        }

        /// <summary>
        /// 查询系统持仓券商账户列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/sys/hold/account/list"), HttpPost]
        [Description("查询系统持仓券商账户列表")]
        public List<BrokerSysHoldAccountInfo> GetBrokerSysHoldAccountList(GetBrokerSysHoldAccountListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetBrokerSysHoldAccountList(request);
        }

        /// <summary>
        /// 券商账户系统持仓详情-回收
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/account/sys/position/recovery"), HttpPost]
        [Description("券商账户系统持仓详情-回收")]
        public object RecoveryBrokerAccountSysPosition(RecoveryBrokerAccountSysPositionRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.RecoveryBrokerAccountSysPosition(request);
            return null;
        }

        /// <summary>
        /// 查询券商列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/list"), HttpPost]
        [Description("查询券商列表")]
        public PageRes<BrokerInfo> GetBrokerList(GetBrokerListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetBrokerList(request);
        }

        /// <summary>
        /// 添加券商
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/add"), HttpPost]
        [Description("添加券商")]
        public object AddBroker(AddBrokerRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddBroker(request);
            return null;
        }

        /// <summary>
        /// 编辑券商
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/modify"), HttpPost]
        [Description("编辑券商")]
        public object ModifyBroker(ModifyBrokerRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyBroker(request);
            return null;
        }

        /// <summary>
        /// 修改券商状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/status/modify"), HttpPost]
        [Description("修改券商状态")]
        public object ModifyBrokerStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyBrokerStatus(request);
            return null;
        }

        /// <summary>
        /// 删除券商
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/delete"), HttpPost]
        [Description("删除券商")]
        public object DeleteBroker(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeleteBroker(request);
            return null;
        }

        /// <summary>
        /// 查询券商营业部列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/department/list"), HttpPost]
        [Description("查询券商营业部列表")]
        public PageRes<BrokerDepartmentInfo> GetBrokerDepartmentList(GetBrokerDepartmentListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetBrokerDepartmentList(request);
        }

        /// <summary>
        /// 添加券商营业部
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/department/add"), HttpPost]
        [Description("添加券商营业部")]
        public object AddBrokerDepartment(AddBrokerDepartmentRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddBrokerDepartment(request);
            return null;
        }

        /// <summary>
        /// 编辑券商营业部
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/department/modify"), HttpPost]
        [Description("编辑券商营业部")]
        public object ModifyBrokerDepartment(ModifyBrokerDepartmentRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyBrokerDepartment(request);
            return null;
        }

        /// <summary>
        /// 修改券商营业部状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/department/status/modify"), HttpPost]
        [Description("修改券商营业部状态")]
        public object ModifyBrokerDepartmentStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyBrokerDepartmentStatus(request);
            return null;
        }

        /// <summary>
        /// 删除券商营业部
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/department/delete"), HttpPost]
        [Description("删除券商营业部")]
        public object DeleteBrokerDepartment(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeleteBrokerDepartment(request);
            return null;
        }

        /// <summary>
        /// 查询券商交易服务器列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/tradehost/list"), HttpPost]
        [Description("查询券商交易服务器列表")]
        public PageRes<BrokerTradeHostInfo> GetBrokerTradeHostList(GetBrokerTradeHostListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetBrokerTradeHostList(request);
        }

        /// <summary>
        /// 添加券商交易服务器
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/tradehost/add"), HttpPost]
        [Description("添加券商交易服务器")]
        public object AddBrokerTradeHost(AddBrokerTradeHostRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddBrokerTradeHost(request);
            return null;
        }

        /// <summary>
        /// 编辑券商交易服务器
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/tradehost/modify"), HttpPost]
        [Description("编辑券商交易服务器")]
        public object ModifyBrokerTradeHost(ModifyBrokerTradeHostRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyBrokerTradeHost(request);
            return null;
        }

        /// <summary>
        /// 修改券商交易服务器状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/tradehost/status/modify"), HttpPost]
        [Description("修改券商交易服务器状态")]
        public object ModifyBrokerTradeHostStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyBrokerTradeHostStatus(request);
            return null;
        }

        /// <summary>
        /// 删除券商交易服务器
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/tradehost/delete"), HttpPost]
        [Description("删除券商交易服务器")]
        public object DeleteBrokerTradeHost(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeleteBrokerTradeHost(request);
            return null;
        }

        /// <summary>
        /// 查询行情服务器列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/hqhost/list"), HttpPost]
        [Description("查询行情服务器列表")]
        public PageRes<BrokerHqHostInfo> GetBrokerHqHostList(GetBrokerHqHostListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetBrokerHqHostList(request);
        }

        /// <summary>
        /// 添加行情服务器
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/hqhost/add"), HttpPost]
        [Description("添加行情服务器")]
        public object AddBrokerHqHost(AddBrokerHqHostRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddBrokerHqHost(request);
            return null;
        }

        /// <summary>
        /// 编辑行情服务器
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/hqhost/modify"), HttpPost]
        [Description("编辑行情服务器")]
        public object ModifyBrokerHqHost(ModifyBrokerHqHostRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyBrokerHqHost(request);
            return null;
        }

        /// <summary>
        /// 修改行情服务器状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/hqhost/status/modify"), HttpPost]
        [Description("修改行情服务器状态")]
        public object ModifyBrokerHqHostStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyBrokerHqHostStatus(request);
            return null;
        }

        /// <summary>
        /// 删除行情服务器
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("broker/hqhost/delete"), HttpPost]
        [Description("删除行情服务器")]
        public object DeleteBrokerHqHost(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeleteBrokerHqHost(request);
            return null;
        }
        #endregion

        #region====支付管理====  
        /// <summary>
        /// 查询支付渠道分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/channel/group"), HttpPost]
        [Description("查询支付渠道分组")]
        public List<SysPaymentChannelGroupInfo> GetSysPaymentChannelGroup()
        {
            return sysSettingHandler.GetSysPaymentChannelGroup();
        }

        /// <summary>
        /// 编辑支付渠道分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/channel/group/modify"), HttpPost]
        [Description("编辑支付渠道分组")]
        public object ModifySysPaymentChannelGroup(ModifySysPaymentChannelGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            sysSettingHandler.ModifySysPaymentChannelGroup(request);
            return null;
        }

        /// <summary>
        /// 修改支付渠道分组状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/channel/group/status/modify"), HttpPost]
        [Description("修改支付渠道分组状态")]
        public object ModifySysPaymentChannelGroupStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySysPaymentChannelGroupStatus(request);
            return null;
        }

        /// <summary>
        /// 查询支付渠道列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/channel/list"), HttpPost]
        [Description("查询支付渠道列表")]
        public PageRes<SysPaymentChannelInfo> GetSysPaymentChannelList(GetSysPaymentChannelListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetSysPaymentChannelList(request);
        }

        /// <summary>
        /// 编辑支付渠道排序值
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/channel/orderindex/modify"), HttpPost]
        [Description("编辑支付渠道排序值")]
        public object ModifySysPaymentChannelOrderIndex(ModifySysPaymentChannelOrderIndexRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySysPaymentChannelOrderIndex(request);
            return null;
        }

        /// <summary>
        /// 修改支付渠道状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/channel/status/modify"), HttpPost]
        [Description("修改支付渠道状态")]
        public object ModifySysPaymentChannelStatus(ModifySysPaymentChannelStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySysPaymentChannelStatus(request);
            return null;
        }

        /// <summary>
        /// 绑定支付渠道账户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/channel/account/bind"), HttpPost]
        [Description("绑定支付渠道账户")]
        public object BindSysPaymentChannelAccount(BindSysPaymentChannelAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.BindSysPaymentChannelAccount(request);
            return null;
        }

        /// <summary>
        /// 查询打款渠道列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("paymentcash/channel/list"), HttpPost]
        [Description("查询打款渠道列表")]
        public PageRes<SysPaymentCashChannelInfo> GetSysPaymentCashChannelList(GetSysPaymentCashChannelListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetSysPaymentCashChannelList(request);
        }

        /// <summary>
        /// 修改打款渠道状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("paymentcash/channel/status/modify"), HttpPost]
        [Description("修改打款渠道状态")]
        public object ModifySysPaymentCashChannelStatus(ModifySysPaymentCashChannelStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySysPaymentCashChannelStatus(request);
            return null;
        }

        /// <summary>
        /// 绑定打款渠道退款账户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("paymentcash/channel/refund/account/bind"), HttpPost]
        [Description("绑定打款渠道退款账户")]
        public object BindSysPaymentCashChannelRefundAccount(BindSysPaymentCashChannelRefundAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.BindSysPaymentCashChannelRefundAccount(request);
            return null;
        }

        /// <summary>
        /// 绑定打款渠道转账账户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("paymentcash/channel/transfer/account/bind"), HttpPost]
        [Description("绑定打款渠道转账账户")]
        public object BindSysPaymentCashChannelTransferAccount(BindSysPaymentCashChannelTransferAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.BindSysPaymentCashChannelTransferAccount(request);
            return null;
        }

        /// <summary>
        /// 查询支付账户列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/account/list"), HttpPost]
        [Description("查询支付账户列表")]
        public PageRes<SysPaymentAccountInfo> GetSysPaymentAccountList(GetSysPaymentAccountListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetSysPaymentAccountList(request);
        }

        /// <summary>
        /// 添加支付账户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/account/add"), HttpPost]
        [Description("添加支付账户")]
        public object AddSysPaymentAccount(AddSysPaymentAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddSysPaymentAccount(request);
            return null;
        }

        /// <summary>
        /// 编辑支付账户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/account/modify"), HttpPost]
        [Description("编辑支付账户")]
        public object ModifySysPaymentAccount(ModifySysPaymentAccountRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySysPaymentAccount(request);
            return null;
        }

        /// <summary>
        /// 修改支付账户状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/account/status/modify"), HttpPost]
        [Description("修改支付账户状态")]
        public object ModifySysPaymentAccountStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySysPaymentAccountStatus(request);
            return null;
        }

        /// <summary>
        /// 删除支付账户
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/account/delete"), HttpPost]
        [Description("删除支付账户")]
        public object DeleteSysPaymentAccount(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeleteSysPaymentAccount(request);
            return null;
        }

        /// <summary>
        /// 查询支付参数信息
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/par/list"), HttpPost]
        [Description("查询支付参数信息")]
        public List<SysPaymentParInfo> GetSysPaymentParList(GetSysPaymentParListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetSysPaymentParList(request);
        }

        /// <summary>
        /// 修改支付参数信息
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("payment/par/modify"), HttpPost]
        [Description("修改支付参数信息")]
        public object ModifySysPaymentPar(ModifySysPaymentParRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySysPaymentPar(request);
            return null;
        }

        /// <summary>
        /// 查询商品列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("goods/list"), HttpPost]
        [Description("查询商品列表")]
        public PageRes<SysGoodsInfo> GetSysGoodsList(GetSysGoodsListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetSysGoodsList(request);
        }

        /// <summary>
        /// 添加商品
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("goods/add"), HttpPost]
        [Description("添加商品")]
        public object AddSysGoods(AddSysGoodsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddSysGoods(request);
            return null;
        }

        /// <summary>
        /// 编辑商品
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("goods/modify"), HttpPost]
        [Description("编辑商品")]
        public object ModifySysGoods(ModifySysGoodsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySysGoods(request);
            return null;
        }

        /// <summary>
        /// 修改商品状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("goods/status/modify"), HttpPost]
        [Description("修改商品状态")]
        public object ModifySysGoodsStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySysGoodsStatus(request);
            return null;
        }

        /// <summary>
        /// 删除商品
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("goods/delete"), HttpPost]
        [Description("删除商品")]
        public object DeleteSysGoods(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeleteSysGoods(request);
            return null;
        }
        #endregion

        #region====短信管理====
        /// <summary>
        /// 查询短信渠道列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/channel/list"), HttpPost]
        [Description("查询短信渠道列表")]
        public PageRes<SmsChannelInfo> GetSmsChannelList(GetSmsChannelListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetSmsChannelList(request);
        }

        /// <summary>
        /// 修改短信渠道状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/channel/status/modify"), HttpPost]
        [Description("修改短信渠道状态")]
        public object ModifySmsChannelStatus(ModifySmsChannelStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySmsChannelStatus(request);
            return null;
        }

        /// <summary>
        /// 查询短信渠道app列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/channel/app/list"), HttpPost]
        [Description("查询短信渠道app列表")]
        public PageRes<SmsChannelAppInfo> GetSmsChannelAppList(GetSmsChannelAppListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetSmsChannelAppList(request);
        }

        /// <summary>
        /// 添加短信渠道app
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/channel/app/add"), HttpPost]
        [Description("添加短信渠道app")]
        public object AddSmsChannelApp(AddSmsChannelAppRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddSmsChannelApp(request);
            return null;
        }

        /// <summary>
        /// 编辑短信渠道app
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/channel/app/modify"), HttpPost]
        [Description("编辑短信渠道app")]
        public object ModifySmsChannelApp(ModifySmsChannelAppRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySmsChannelApp(request);
            return null;
        }

        /// <summary>
        /// 修改短信渠道app状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/channel/app/status/modify"), HttpPost]
        [Description("修改短信渠道app状态")]
        public object ModifySmsChannelAppStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySmsChannelAppStatus(request);
            return null;
        }

        /// <summary>
        /// 删除短信渠道app
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/channel/app/delete"), HttpPost]
        [Description("删除短信渠道app")]
        public object DeleteSmsChannelApp(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeleteSmsChannelApp(request);
            return null;
        }

        /// <summary>
        /// 查询短信签名列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/sign/list"), HttpPost]
        [Description("查询短信签名列表")]
        public PageRes<SmsSignInfo> GetSmsSignList(GetSmsSignListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetSmsSignList(request);
        }

        /// <summary>
        /// 添加短信签名
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/sign/add"), HttpPost]
        [Description("添加短信签名")]
        public object AddSmsSign(AddSmsSignRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddSmsSign(request);
            return null;
        }

        /// <summary>
        /// 编辑短信签名
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/sign/modify"), HttpPost]
        [Description("编辑短信签名")]
        public object ModifySmsSign(ModifySmsSignRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySmsSign(request);
            return null;
        }

        /// <summary>
        /// 修改短信签名状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/sign/status/modify"), HttpPost]
        [Description("修改短信签名状态")]
        public object ModifySmsSignStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifySmsSignStatus(request);
            return null;
        }

        /// <summary>
        /// 删除短信签名
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/sign/delete"), HttpPost]
        [Description("删除短信签名")]
        public object DeleteSmsSign(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeleteSmsSign(request);
            return null;
        }

        /// <summary>
        /// 短信签名提交审核
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/sign/examine/apply"), HttpPost]
        [Description("短信签名提交审核")]
        public object SmsSignApplyExamine(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.SmsSignApplyExamine(request);
            return null;
        }
        #endregion

        #region====推送管理====
        /// <summary>
        /// 查询推送渠道列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/channel/list"), HttpPost]
        [Description("查询推送渠道列表")]
        public PageRes<PushChannelInfo> GetPushChannelList(GetPushChannelListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetPushChannelList(request);
        }

        /// <summary>
        /// 修改推送渠道状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/channel/status/modify"), HttpPost]
        [Description("修改推送渠道状态")]
        public object ModifyPushChannelStatus(ModifyPushChannelStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyPushChannelStatus(request);
            return null;
        }

        /// <summary>
        /// 查询推送渠道app列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/channel/app/list"), HttpPost]
        [Description("查询推送渠道app列表")]
        public PageRes<PushChannelAppInfo> GetPushChannelAppList(GetPushChannelAppListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetPushChannelAppList(request);
        }

        /// <summary>
        /// 添加推送渠道app
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/channel/app/add"), HttpPost]
        [Description("添加推送渠道app")]
        public object AddPushChannelApp(AddPushChannelAppRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddPushChannelApp(request);
            return null;
        }

        /// <summary>
        /// 编辑推送渠道app
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/channel/app/modify"), HttpPost]
        [Description("编辑推送渠道app")]
        public object ModifyPushChannelApp(ModifyPushChannelAppRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyPushChannelApp(request);
            return null;
        }

        /// <summary>
        /// 修改推送渠道app状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/channel/app/status/modify"), HttpPost]
        [Description("修改推送渠道app状态")]
        public object ModifyPushChannelAppStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyPushChannelAppStatus(request);
            return null;
        }

        /// <summary>
        /// 删除推送渠道app
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/channel/app/delete"), HttpPost]
        [Description("删除推送渠道app")]
        public object DeletePushChannelApp(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeletePushChannelApp(request);
            return null;
        }

        /// <summary>
        /// 查询推送分组列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/tag/list"), HttpPost]
        [Description("查询推送分组列表")]
        public PageRes<PushTagInfo> GetPushTagList(GetPushTagListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return sysSettingHandler.GetPushTagList(request);
        }

        /// <summary>
        /// 添加推送分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/tag/add"), HttpPost]
        [Description("添加推送分组")]
        public object AddPushTag(AddPushTagRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.AddPushTag(request);
            return null;
        }

        /// <summary>
        /// 编辑推送分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/tag/modify"), HttpPost]
        [Description("编辑推送分组")]
        public object ModifyPushTag(ModifyPushTagRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyPushTag(request);
            return null;
        }

        /// <summary>
        /// 修改推送分组状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/tag/status/modify"), HttpPost]
        [Description("修改推送分组状态")]
        public object ModifyPushTagStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.ModifyPushTagStatus(request);
            return null;
        }

        /// <summary>
        /// 删除推送分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/tag/delete"), HttpPost]
        [Description("删除推送分组")]
        public object DeletePushTag(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            sysSettingHandler.DeletePushTag(request);
            return null;
        }
        #endregion
    }
}
