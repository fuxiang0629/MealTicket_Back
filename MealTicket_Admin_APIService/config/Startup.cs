using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Ninject;
using System.Web.Http.Filters;
using Newtonsoft.Json.Converters;
using System.Web.Http.Cors;
using FXCommon.Common;
using WebAPI.HelpPage;
using MealTicket_Admin_Handler;

namespace MealTicket_Admin_APIService
{
    /// <summary>
    /// Owin启动配置。实现服务的默认配置。
    /// </summary>
    public class Startup
    {
        private AuthorityHandler authorityHandler;

        public Startup()
        {
            authorityHandler = WebApiManager.Kernel.Get<AuthorityHandler>();
        }
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
            });
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Filters.AddRange(WebApiManager.Kernel.GetAll<IFilter>());
            appBuilder.UseWebApi(config);

            var help = ConfigurationManager.AppSettings["help"];
            bool settingHelp = true;
            if (!string.IsNullOrEmpty(help))
            {
                //配置帮助文档
                var helps = help.Split(',');
                List<string> helpPar = new List<string>();
                foreach (var item in helps)
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, item);
                    if (File.Exists(path))
                    {
                        helpPar.Add(path);
                        continue;
                    }
                    else
                    {
                        settingHelp = false;
                    }
                }
                if (settingHelp)
                {
                    appBuilder.UseWebApiHelpPage(config, xmlDocumentationPhysicalPath: helpPar.ToArray());
                }
                //初始化数据库的方法列表
                InitActionList(config);
            }
        }

        /// <summary>
        /// 初始化ActionList
        /// </summary>
        void InitActionList(HttpConfiguration config)
        {
            try
            {
                var list = config.GetAllWebApi();
                authorityHandler.InitActionList(list);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("初始化ActionList失败。", ex);
            }
        }
    }
}
