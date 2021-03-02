using FXCommon.Common;
using MealTicket_Handler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_APIService
{
    public partial class APIService : ServiceBase
    {
        /// <summary>
        /// 控制api启动关闭的管理对象
        /// </summary>
        WebApiManager _manager;

        public APIService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                //在此处添加代码以启动服务。
                _manager = new WebApiManager();
                _manager.Start<Startup>(ConfigurationManager.AppSettings["url"]);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("启动出错", ex);
                OnStop();
            }
        }

        protected override void OnStop()
        {
            //在此处添加代码以执行停止服务所需的关闭操作。
            _manager.Dispose();
        }
    }
}
