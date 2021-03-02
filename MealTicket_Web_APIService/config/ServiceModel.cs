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
            Bind<Runner>().To<ClearTransactiondataRunner>();
        }
    }
}
