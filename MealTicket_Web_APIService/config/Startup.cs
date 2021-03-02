﻿using Owin;
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

namespace MealTicket_Web_APIService
{
    /// <summary>
    /// Owin启动配置。实现服务的默认配置。
    /// </summary>
    public class Startup
    {
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
        }
    }
}
