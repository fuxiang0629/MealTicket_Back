using MealTicket_Admin_Handler.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler
{
    public class MenuHandler
    {
        /// <summary>
        /// 获取菜单列表
        /// </summary>
        /// <returns></returns>
        public List<MenuInfo> GetMenuList(GetMenuListRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                List<long> rightMenuId = new List<long>();
                if (!basedata.IsAdministrator) 
                {
                    rightMenuId = (from item in db.t_admin_role_rel
                                   join item2 in db.t_admin_role on item.RoleId equals item2.Id
                                   join item3 in db.t_admin_role_right_rel on item2.Id equals item3.RoleId
                                   join item4 in db.t_admin_right on item3.RightId equals item4.Id
                                   where item.AccountId == basedata.AccountId && item2.Status == 1
                                   select item4.MenuId).Distinct().ToList();
                }
                var menuList = (from item in db.t_admin_menu
                                join item2 in db.t_admin_menu on item.Father equals item2.Id
                                join item3 in db.t_admin_menu on item2.Father equals item3.Id
                                where (rightMenuId.Contains(item.Id) || basedata.IsAdministrator) && item.Status == 1 && item2.Status == 1 && item3.Status == 1
                                select new { item, item2, item3 }).ToList();

                List<MenuInfo> result = new List<MenuInfo>();
                //获取一级菜单
                if (request.FatherMenuId == 0)
                {
                    result = (from item in menuList
                              group item by item.item3 into g
                              orderby g.Key.OrderIndex
                              select new MenuInfo
                              {
                                  icon=g.Key.Icon,
                                  id=g.Key.Id,
                                  name=g.Key.Title,
                                  path=g.Key.Url
                              }).ToList();
                }
                //获取二,三级菜单
                else
                {
                    result = (from item in menuList
                              where item.item3.Id== request.FatherMenuId
                              group item by item.item2 into g
                              orderby g.Key.OrderIndex
                              select new MenuInfo
                              {
                                  icon = g.Key.Icon,
                                  id = g.Key.Id,
                                  name = g.Key.Title,
                                  path = g.Key.Url,
                                  routes = (from x in g
                                            orderby x.item.OrderIndex
                                            select new MenuInfo
                                            {
                                                icon = x.item.Icon,
                                                id = x.item.Id,
                                                name = x.item.Title,
                                                path = x.item.Url
                                            }).ToList()
                              }).ToList();
                }
                return result;
            }
        }
    }
}
