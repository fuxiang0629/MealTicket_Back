﻿using FXCommon.Common;
using MealTicket_Web_Handler;
using MealTicket_Web_Handler.Transactiondata;
using Microsoft.Owin.Hosting;
using Ninject;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace MealTicket_Web_APIService
{
    /// <summary>
    /// Api框架管理器。用于管理框架内部组件的启动和关闭
    /// </summary>
    public class WebApiManager : IDisposable
    {
        /// <summary>
        /// 依赖注入
        /// </summary>
        public static IKernel Kernel { get; set; }

        public static List<Runner> runners;

        public Singleton session;

        /// <summary>
        /// owin启动类
        /// </summary>
        IDisposable _app;

        /// <summary>
        /// 是否正常启动
        /// </summary>
        bool _isstart;

        /// <summary>
        /// 关闭
        /// </summary>
        public void Dispose()
        {
            _app.Dispose();
            if(_isstart)
            {
                Kernel.Dispose();
            }
            if (runners != null)
            {
                foreach (var item in runners)
                {
                    item.Dispose();
                }
            }
            if (session != null)
            {
                session.Dispose();
            }
        }

        /// <summary>
        /// 启动web api。默认使用框架内部配置。
        /// 默认启用：
        /// 1.框架自定义缓存对象
        /// 2.框架自定义循环任务
        /// 3.启动api
        /// </summary>
        /// <param name="url">监听基地址</param>
        /// <param name="urls">其他扩展地址</param>
        public void Start(string url, params string[] urls)
        {
            Start<Startup>(url, urls);
        }

        /// <summary>
        /// 启动web api。
        /// </summary>
        /// <typeparam name="TStartup">启动的配置类，可以使用任何符合Owin规范的配置</typeparam>
        /// <param name="url">监听基地址</param>
        /// <param name="urls">其他扩展地址</param>
        public void Start<TStartup>(string url, params string[] urls)
        {
            //框架内部缓存信息
            session = Singleton.Instance;

            session._BuyTipSession = new MealTicket_Web_Handler.session.BuyTipSession();
            session._BuyTipSession.StartUpdate(1000);
            session._SharesQuotesSession = new MealTicket_Web_Handler.session.SharesQuotesSession();
            session._SharesQuotesSession.StartUpdate(3000);
            session._PlateRateSession = new MealTicket_Web_Handler.session.PlateRateSession();
            session._PlateRateSession.StartUpdate(3000);
            session._SharesBaseSession = new MealTicket_Web_Handler.session.SharesBaseSession();
            session._SharesBaseSession.StartUpdate(3600000);
            session._AccountTrendTriSession = new MealTicket_Web_Handler.session.AccountTrendTriSession();
            session._AccountTrendTriSession.StartUpdate(3000);
            session._AccountRiseLimitTriSession = new MealTicket_Web_Handler.session.AccountRiseLimitTriSession();
            session._AccountRiseLimitTriSession.StartUpdate(3000);
            session._SharesQuotesDateSession = new MealTicket_Web_Handler.session.SharesQuotesDateSession();
            session._SharesQuotesDateSession.StartUpdate(3600000);
            session._BasePlateSession = new MealTicket_Web_Handler.session.BasePlateSession();
            session._BasePlateSession.StartUpdate(3000);


            var mqHandler = session.StartMqHandler();//生成Mq队列对象
            var transactionDataTask = session.StartTransactionDataTask();
            transactionDataTask.DoTask();
            mqHandler.StartListen();//启动队列监听

            var mqHandler_SecurityBarsData = session.StartMqHandler_SecurityBarsData();//生成Mq队列对象
            var securityBarsDataTask = session.StartSecurityBarsDataTask();
            securityBarsDataTask.DoTask();
            //mqHandler_SecurityBarsData.StartListen();//启动队列监听

            //加载依赖注入
            Kernel = LoadKernel();   
            //加载循环任务
            runners = Kernel.GetAll<Runner>().ToList();
            runners.ForEach(e => e.Run());
            StartOptions options = new StartOptions();
            options.Urls.Add(url);
            foreach (var item in urls)
            {
                options.Urls.Add(item);
            }
            //启动app
            _app = WebApp.Start<TStartup>(options);
            _isstart = true;
        }

        /// <summary>
        /// 加载依赖注入。
        /// 可重载方法，返回自定义的依赖注入模块
        /// </summary>
        protected IKernel LoadKernel()
        {
            return new StandardKernel(new ServiceModel());
        }
    }
}
