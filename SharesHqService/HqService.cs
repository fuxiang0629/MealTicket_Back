using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using FXCommon.Common;

namespace SharesHqService
{
    public partial class HqService : ServiceBase
    {
        /// <summary>
        /// 接口绑定
        /// </summary>
        IKernel _kernel;
        List<Runner> runners;
        public HqService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var Instance=Singleton.Instance;
            _kernel = new StandardKernel(new ServiceModule()); 
            runners = _kernel.GetAll<Runner>().ToList();
            runners.ForEach((e) => e.Run());
        }

        protected override void OnStop()
        {
            runners.ForEach((e) => e.Dispose());
            Singleton.Instance.Dispose();
            _kernel.Dispose();
        }
    }
}
