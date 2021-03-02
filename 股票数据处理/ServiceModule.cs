using Ninject.Modules;
using System.Configuration;
using System.Net;

namespace ShareData_Service
{
    public class ServiceModule : NinjectModule
    {
        public override void Load()
        {
            //Bind<Runner>().To<GetRecordFileRunner>().InSingletonScope();
            //Bind<Runner>().To<DownloadFileRunner>().InSingletonScope()
            //                    .WithConstructorArgument("exchange", "CallcenterRecordFile")
            //                    .WithConstructorArgument("queuename", "DownLoadRecordFile");
            //Bind<Runner>().To<HandlerFileRunner>().InSingletonScope()
            //                    .WithConstructorArgument("exchange", "CallcenterRecordFile")
            //                    .WithConstructorArgument("queuename", "HandlerRecordFile");
            //Bind<Runner>().To<TrimFileRunner>().InSingletonScope()
            //                    .WithConstructorArgument("exchange", "CallcenterRecordFile")
            //                    .WithConstructorArgument("queuename", "TrimRecordFile");
            //Bind<Runner>().To<MatchFileRunner>().InSingletonScope()
            //                    .WithConstructorArgument("exchange", "CallcenterRecordFile")
            //                    .WithConstructorArgument("queuename", "MatchRecordFile");
            //Bind<Runner>().To<DownloadRetryRunner>().InSingletonScope();
            //Bind<Runner>().To<HandlerFileRetryRunner>().InSingletonScope();
            //Bind<Runner>().To<TrimFileRetryRunner>().InSingletonScope();
            //Bind<Runner>().To<MatchFileRetryRunner>().InSingletonScope();
            //Bind<Runner>().To<FinishMatchFileRetryRunner>().InSingletonScope();
        }
    }
}
