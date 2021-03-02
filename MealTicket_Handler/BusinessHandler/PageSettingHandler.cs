using MealTicket_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class PageSettingHandler
    {
        /// <summary>
        /// 查询页面配置信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object GetPageSettingInfo(GetPageSettingInfoRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var page = (from item in db.t_page_setting
                            where item.PageCode == request.PageCode && item.Status == 1
                            select new
                            { 
                                Content=item.Content
                            }).FirstOrDefault();
                return page;
            }
        }
    }
}
