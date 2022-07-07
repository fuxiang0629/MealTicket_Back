using FXCommon.Common;
using MealTicket_Web_APIService.Filter;
using MealTicket_Web_APIService.runner;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace MealTicket_Web_APIService
{
    class ServiceModel : NinjectModule
    {
        public override void Load()
        {
            Bind<IFilter>().To<LogActionFilter>();
            Bind<IFilter>().To<AuthorizationFilter>();
            Bind<IFilter>().To<CustomExceptionFilter>();
            Bind<Runner>().To<PlateTagCalRunner>();
            Bind<Runner>().To<PlateLeaderTriRunner>();
            Bind<Runner>().To<SharesGroupStatisticMtlineRunner>();
            Bind<Runner>().To<HotSpotHisRunner>();
            Bind<Runner>().To<SharesGroupStatisticRunner>();
            Bind<Runner>().To<BiddingRunner>();
            Bind<Runner>().To<SearchMonitorRunner>();
            Bind<Runner>().To<SearchMonitorMarkRunner>();
            Bind<Runner>().To<SharesMonitorTriRunner>();
            Bind<Runner>().To<HotSpotHighMarkRunner>();
            Bind<Runner>().To<HotSpotHighMark_TodayRunner>();
            Bind<Runner>().To<PlateLimitUpStatisticRunner>();
            Bind<Runner>().To<SharesPremiumStatisticRunner>();
            Bind<Runner>().To<MarketSentimentCalRunner>();
            Bind<Runner>().To<SearchBiddingRunner>();
            //Bind<Runner>().To<SharesCycleManagerStockRunner>();
        }
    }
}
