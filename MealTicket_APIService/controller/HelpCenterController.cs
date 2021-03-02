using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Ninject;
using SMS_SendHandler;
using System.ComponentModel;
using MealTicket_APIService.Filter;
using MealTicket_Handler;
using MealTicket_Handler.Model;

namespace MealTicket_APIService.controller
{
    /// <summary>
    /// 帮助中心接口
    /// </summary>
    [RoutePrefix("helpcenter")]
    public class HelpCenterController : ApiController
    {
        private HelpCenterHandler helpCenterHandler;

        public HelpCenterController()
        {
            helpCenterHandler = WebApiManager.Kernel.Get<HelpCenterHandler>();
        }

        /// <summary>
        /// 帮助中心-获取问题分类列表
        /// </summary>
        /// <returns></returns>
        [Route("question/type/list"),HttpPost]
        [Description("帮助中心-获取问题分类列表")]
        public List<QuestionType> GetQuestionType()
        {
            return helpCenterHandler.GetQuestionType();
        }

        /// <summary>
        /// 帮助中心-获取问题列表
        /// </summary>
        /// <returns></returns>
        [Route("question/list"), HttpPost]
        [Description("帮助中心-获取问题列表")]
        public PageRes<Question> GetQuestionList(GetQuestionListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            return helpCenterHandler.GetQuestionList(request);
        }

        /// <summary>
        /// 帮助中心-获取问题详情
        /// </summary>
        /// <param name="id">问题id</param>
        /// <returns></returns>
        [Route("question/details"), HttpPost]
        [Description("帮助中心-获取问题详情")]
        public QuestionDetails GetQuestionDetails(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return helpCenterHandler.GetQuestionDetails(request);
        }
    }
}
