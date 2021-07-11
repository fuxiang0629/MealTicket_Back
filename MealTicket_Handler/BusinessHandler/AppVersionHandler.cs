using MealTicket_DBCommon;
using MealTicket_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.BusinessHandler
{
    public class AppVersionHandler
    {
        /// <summary>
        /// 获取app版本信息
        /// </summary>
        /// <returns></returns>
        public AppVersionInfo GetAppVersionInfo(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var appVersion = (from item in db.t_app_version
                                  where item.MarketCode == basedata.MarketCode && item.AppVersion == basedata.AppVersion
                                  select new AppVersionInfo
                                  {
                                      ExamineStatus = item.ExamineStatus,
                                      AppVersionOrder = item.AppVersionOrder,
                                      WebViewUrl = item.WebViewUrl,
                                      WaitSecond=item.WaitTime
                                  }).FirstOrDefault();
                if (appVersion == null)
                {
                    throw new WebApiException(400,"无效的版本号");
                }
                appVersion.ForceVersionOrder = basedata.MarketCode == "android" ? Singleton.Instance.ForceVersionOrder_Android : Singleton.Instance.ForceVersionOrder_Ios;
                appVersion.MaxVersionOrder = basedata.MarketCode == "android" ? Singleton.Instance.MaxVersionOrder_Android : Singleton.Instance.MaxVersionOrder_Ios;
                appVersion.MaxVersionName = basedata.MarketCode == "android" ? Singleton.Instance.MaxVersionName_Android : Singleton.Instance.MaxVersionName_Ios; 
                appVersion.MaxVersionUrl = basedata.MarketCode == "android" ? Singleton.Instance.MaxVersionUrl_Android : Singleton.Instance.MaxVersionUrl_Ios;
                return appVersion;
            }
        }
    }
}
