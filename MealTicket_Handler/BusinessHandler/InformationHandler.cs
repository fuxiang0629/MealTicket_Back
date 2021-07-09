using MealTicket_DBCommon;
using MealTicket_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.BusinessHandler
{
    public class InformationHandler
    {
        /// <summary>
        /// 获取资讯列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<InformationInfo> GetInformationList(PageRequest request) 
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var information = from item in db.t_information
                                  where item.StartShowTime<= timeNow && item.EndShowTime> timeNow && item.Status==1
                                  select item;
                if (request.MaxId >0)
                {
                    information = from item in information
                                  where item.Id <= request.MaxId
                                  select item;
                }

                int totalCount = information.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = information.Max(e => e.Id);
                }
                var list = (from item in information
                            orderby item.CreateTime descending
                            select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                var result = (from item in list
                              select new InformationInfo
                              {
                                  IsHaveContent = string.IsNullOrEmpty(item.Content) ? false : true,
                                  ContentIntroduction = item.ContentIntroduction,
                                  CreateTime = item.CreateTime,
                                  Title = item.Title,
                                  Id = item.Id
                              }).ToList();

                return new PageRes<InformationInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = result
                };
            }
        }

        /// <summary>
        /// 获取资讯详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public InformationDetails GetInformationDetails(DetailsRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var information = (from item in db.t_information
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (information==null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                return new InformationDetails 
                {
                    Content= information.Content,
                    ContentIntroduction= information.ContentIntroduction,
                    CreateTime= information.CreateTime,
                    Id= information.Id,
                    IsHaveContent=string.IsNullOrEmpty(information.Content)?false:true,
                    Title= information.Title
                };
            }
        }
    }
}
