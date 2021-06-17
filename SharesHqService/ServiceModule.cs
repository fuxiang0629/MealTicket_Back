using Ninject.Modules;
using System.Configuration;
using System.Net;
using FXCommon.Common;

namespace SharesHqService
{
    public class ServiceModule : NinjectModule
    {
        public override void Load()
        {
            Bind<Runner>().To<AllSharesUpdateRunner>().InSingletonScope();
            Bind<Runner>().To<SharesQuotesUpdateRunner>().InSingletonScope();
            Bind<Runner>().To<BusinessTableUpdateRunner>().InSingletonScope();
        }
    }
}
