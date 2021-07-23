using Ninject.Modules;
using System.Configuration;
using System.Net;
using FXCommon.Common;

namespace TransactionDataUpdate_App
{
    public class ServiceModule : NinjectModule
    {
        public override void Load()
        {
            Bind<Runner>().To<TransactiondataUpdateRunner>().InSingletonScope();
        }
    }
}
