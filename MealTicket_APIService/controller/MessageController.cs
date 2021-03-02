using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using MealTicket_APIService.Filter;
using MealTicket_Handler;
using MealTicket_Handler.Model;
using Ninject;

namespace MealTicket_APIService.controller
{
    [RoutePrefix("message")]
    public class MessageController : ApiController
    {
        private MessageHandler messageHandler;

        public MessageController()
        {
            messageHandler = WebApiManager.Kernel.Get<MessageHandler>();
        }

        /// <summary>
        /// 获取未读消息数量
        /// </summary>
        /// <returns></returns>
        [Description("获取未读消息数量")]
        [Route("message/unread/count"), HttpPost]
        public MessageUnreadCountInfo GetMessageUnreadCount()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return messageHandler.GetMessageUnreadCount(basedata);
        }

        /// <summary>
        /// 获取消息分组列表
        /// </summary>
        /// <returns></returns>
        [Description("获取消息分组列表")]
        [Route("message/group/list"), HttpPost]
        [CheckUserLoginFilter]
        public List<MessageGroupInfo> AccountGetMessageGroupList()
        {
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return messageHandler.GetMessageGroupList(basedata);
        }

        /// <summary>
        /// 获取某一分组消息列表
        /// </summary>
        /// <returns></returns>
        [Description("获取某一分组消息列表")]
        [Route("message/list"), HttpPost]
        [CheckUserLoginFilter]
        public PageRes<MessageInfo> AccountGetMessageList(GetMessageListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            HeadBase basedata = ActionContext.ActionArguments["basedata"] as HeadBase;
            return messageHandler.GetMessageList(request, basedata);
        }
    }
}
