using MealTicket_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class BannerHandler
    {
        /// <summary>
        /// 获取广告位列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<BannerInfo> GetBannerList(GetBannerListRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var banner = (from item in db.t_banner
                              join item2 in db.t_banner_group on item.GroupId equals item2.Id
                              where item2.GroupCode == request.BannerCode && item.Status == 1 && item2.Status==1
                              orderby item.OrderIndex
                              select new BannerInfo
                              {
                                  ImgUrl = item.ImgUrl,
                                  ActionType = item.ActionType,
                                  ActionPath = item.ImgPath
                              }).ToList();
                return banner;
            }
        }
    }
}
