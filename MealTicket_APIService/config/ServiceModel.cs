using FXCommon.Common;
using MealTicket_APIService.Filter;
using MealTicket_APIService.runner;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace MealTicket_APIService
{
    class ServiceModel : NinjectModule
    {
        public override void Load()
        {
            Bind<IFilter>().To<LogActionFilter>();
            Bind<IFilter>().To<AuthorizationFilter>();
            Bind<IFilter>().To<CustomExceptionFilter>();
            Bind<Runner>().To<ClosingAutoRunner>();
            Bind<Runner>().To<ClosingForceRunner>();
            Bind<Runner>().To<TradeCleanRunner>();
            Bind<Runner>().To<HoldServiceCalcuRunner>();
            Bind<Runner>().To<RechargeCloseRunner>();
            Bind<Runner>().To<RechargeRefundRunner>();
            Bind<Runner>().To<RechargeRefundQueryRunner>();
            Bind<Runner>().To<UpdateTransactiondataRunner>();
            Bind<Runner>().To<SmsSendRunner>();
            Bind<Runner>().To<TransactiondataRealTimeRunner>();
            Bind<Runner>().To<TransactiondataDateRunner>();
            Bind<Runner>().To<TradeAutoRunner>();
        }
    }
}
