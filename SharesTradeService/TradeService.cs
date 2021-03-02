using FXCommon.Common;
using Ninject;
using SharesTradeService.Handler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SharesTradeService
{
    public partial class TradeService : ServiceBase
    {
        /// <summary>
        /// 接口绑定
        /// </summary>
        IKernel _kernel;
        List<Runner> runners;
        public TradeService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var Instance = Singleton.instance;
            _kernel = new StandardKernel(new ServiceModule());
            runners = _kernel.GetAll<Runner>().ToList();
            runners.ForEach((e) => e.Run());
        }

        protected override void OnStop()
        {
            runners.ForEach((e) => e.Dispose());
            Singleton.instance.Dispose();
            _kernel.Dispose();
        }
    }
}
