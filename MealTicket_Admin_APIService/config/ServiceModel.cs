using FXCommon.Common;
using MealTicket_Admin_APIService.Filter;
using MealTicket_Admin_APIService.service;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace MealTicket_Admin_APIService
{
    class ServiceModel : NinjectModule
    {
        public override void Load()
        {
            Bind<IFilter>().To<LogActionFilter>();
            Bind<IFilter>().To<AuthorizationFilter>();
            Bind<IFilter>().To<CustomExceptionFilter>();
            Bind<Runner>().To<CashRefundResultQuery>();
        }
    }
}
