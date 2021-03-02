using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using MealTicket_Handler.BusinessHandler;
using MealTicket_Handler.Model;
using Ninject;

namespace MealTicket_APIService.controller
{
    [RoutePrefix("information")]
    public class InformationController:ApiController
    {
        private InformationHandler informationHandler;

        public InformationController()
        {
            informationHandler = WebApiManager.Kernel.Get<InformationHandler>();
        }

        /// <summary>
        /// 获取资讯列表
        /// </summary>
        /// <returns></returns>
        [Description("获取资讯列表")]
        [Route("list"), HttpPost]
        public PageRes<InformationInfo> GetInformationList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return informationHandler.GetInformationList(request);
        }

        /// <summary>
        /// 获取资讯详情
        /// </summary>
        /// <returns></returns>
        [Description("获取资讯列表")]
        [Route("details"), HttpPost]
        public InformationDetails GetInformationDetails(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return informationHandler.GetInformationDetails(request);
        }
    }
}
