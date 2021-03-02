using MealTicket_Handler.Model;
using Ninject;
using Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MealTicket_APIService
{
    public static class ExtensionHelper
    {
        /// <summary>
        /// 获取当前配置内的所有web api
        /// </summary>
        public static List<ActionList> GetAllWebApi(this HttpConfiguration config)
        {
            List<ActionList> list = new List<ActionList>();
            var service = config.Services.GetHttpControllerTypeResolver().GetControllerTypes(
             config.Services.GetAssembliesResolver());
            foreach (var controller in service)
            {
                var methods = from item in controller.GetMethods()
                              where item.IsPublic && !item.IsStatic
                                   && !item.IsSpecialName
                                   && !item.DeclaringType.IsAssignableFrom(typeof(ApiController))
                                   && item.GetCustomAttribute<NonActionAttribute>() == null
                              select item;
                foreach (var item in methods)
                {
                    var rrr = item.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
                    string des = string.Empty;
                    if (rrr != null)
                    {
                        des = rrr.Description;
                    }
                    list.Add(new ActionList
                    {
                        Url = controller.Name.Replace("Controller","")+"."+ item.Name,
                        Description = des
                    });
                }
            }
            return list.ToList();
        }
    }
}
