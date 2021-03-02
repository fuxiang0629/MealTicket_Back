using Ninject.Modules;
using System.Configuration;
using System.Net;
using FXCommon.Common;

namespace SharesTradeService
{
    public class ServiceModule : NinjectModule
    {
        public override void Load()
        {
            Bind<Runner>().To<QueryTradeResultRunner>().InSingletonScope();
        }
    }
}
