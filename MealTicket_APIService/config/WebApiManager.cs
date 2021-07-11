using FXCommon.Common;
using MealTicket_Handler;
using Microsoft.Owin.Hosting;
using Ninject;
using System;
using System.Linq;

namespace MealTicket_APIService
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
            var mqHandler = session.StartMqHandler("");

            //加载依赖注入
            Kernel = LoadKernel();   
            //加载循环任务
            var runners = Kernel.GetAll<Runner>().ToList();
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
