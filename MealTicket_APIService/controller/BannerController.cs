using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using MealTicket_Handler;
using MealTicket_Handler.Model;
using Ninject;

namespace MealTicket_APIService.controller
{
    [RoutePrefix("banner")]
    public class BannerController : ApiController
    {
        private BannerHandler bannerHandler;

        public BannerController()
        {
            bannerHandler = WebApiManager.Kernel.Get<BannerHandler>();
        }

        /// <summary>
        /// 获取广告位列表
        /// </summary>
        /// <returns></returns>
        [Description("获取广告位列表")]
        [Route("list"), HttpPost]
        public List<BannerInfo> GetBannerList(GetBannerListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            return bannerHandler.GetBannerList(request);
        }

    }
}
