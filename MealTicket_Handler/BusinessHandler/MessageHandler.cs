using MealTicket_DBCommon;
using MealTicket_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class MessageHandler
    {

        /// <summary>
        /// 获取未读消息数量
        /// </summary>
        /// <returns></returns>
        public MessageUnreadCountInfo GetMessageUnreadCount(HeadBase basedata)
        {
            AuthorityHandler _handler = new AuthorityHandler();
            try
            {
                _handler.CheckAccountLogin(basedata.UserToken, basedata);
                using (var db = new meal_ticketEntities())
                {
                    var MessageUnread = from item in db.t_message
                                        where item.IsRead == false && item.AccountId == basedata.AccountId
                                        select item;
                    return new MessageUnreadCountInfo
                    {
                        UnreadCount = MessageUnread.Count()
                    };
                }
            }
            catch (Exception ex)
            {
                return new MessageUnreadCountInfo
                {
                    UnreadCount = 0
                };
            }
        }

        /// <summary>
        /// 获取消息分组列表
        /// </summary>
        /// <returns></returns>
        public List<MessageGroupInfo> GetMessageGroupList(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var MessageGroup = (from item in db.t_message_group
                                    where item.Status == 1
                                    orderby item.OrderIndex
                                    select new MessageGroupInfo
                                    {
                                        ImgUrl = item.ImgUrl,
                                        GroupId = item.Id,
                                        GroupTitle = item.Title,
                                        UnreadCount = 0
                                    }).ToList();

                foreach (var item in MessageGroup)
                {
                    var message = from x in db.t_message
                                  join x2 in db.t_message_group_rel on x.Id equals x2.MessageId
                                  where x2.GroupId == item.GroupId && x.IsRead == false && x.AccountId == basedata.AccountId
                                  select x;
                    item.UnreadCount = message.Count();
                }
                return MessageGroup;
            }
        }

        /// <summary>
        /// 获取某一分组消息列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<MessageInfo> GetMessageList(GetMessageListRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var message = from item in db.t_message
                              join item2 in db.t_message_group_rel on item.Id equals item2.MessageId
                              where item2.GroupId == request.GroupId && item.AccountId == basedata.AccountId
                              select item;
                if (request.MaxId > 0)
                {
                    message = from item in message
                              where item.Id <= request.MaxId
                              select item;
                }

                int totalCount = message.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = message.Max(e => e.Id);
                }

                var unreadMessage = (from item in message
                                     where item.IsRead == false
                                     select item).ToList();
                foreach (var x in unreadMessage)
                {
                    x.IsRead = true;
                }
                if (unreadMessage.Count() > 0)
                {
                    db.SaveChanges();
                }

                return new PageRes<MessageInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in message
                            orderby item.CreateTime descending
                            select new MessageInfo
                            {
                                Content = item.Content,
                                CreateTime = item.CreateTime,
                                MessageId = item.Id,
                                Title = item.Title
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }
    }
}
