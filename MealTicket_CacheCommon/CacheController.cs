using MealTicket_CacheCommon_Session;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace MealTicket_CacheCommon
{
    [RoutePrefix("cache")]
    public class CacheController : ApiController
    {
        /// <summary>
        /// 登入
        /// </summary>
        /// <returns></returns>
        [Route("login"), HttpPost]
        public object CacheLogin(LoginInfo request)
        {
            if (request == null)
            {
                return new
                {
                    ErrorCode = 402,
                    ErrorMessage = "参数错误"
                };
            }

            LoginResult loginResult = new LoginResult();
            if (!Singleton.Instance.loginDic.TryGetValue(request.Username, out loginResult))
            {
                return new
                {
                    ErrorCode = 401,
                    ErrorMessage = "账户名密码错误"
                };
            }

            if (loginResult.Password != request.Password)
            {
                return new
                {
                    ErrorCode = 401,
                    ErrorMessage = "账户名密码错误"
                };
            }

            return new
            {
                Result = loginResult.VisitKey
            };
        }

        /// <summary>
        /// 查询缓存
        /// </summary>
        /// <returns></returns>
        [Route("get"), HttpGet]
        public object CacheGet(string key)
        {
            string userToken = ActionContext.Request.Headers.GetValues("userToken").FirstOrDefault();
            if (string.IsNullOrEmpty(userToken))
            {
                return new
                {
                    ErrorCode = 403,
                    ErrorMessage = "无权访问"
                };
            }

            string result = Singleton.Instance.getCacheData(userToken, key);
            return new
            {
                Result = result
            };
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <returns></returns>
        [Route("set"), HttpPost]
        public object CacheSet(CacheInfo request)
        {
            string userToken = ActionContext.Request.Headers.GetValues("userToken").FirstOrDefault();
            if (string.IsNullOrEmpty(userToken))
            {
                return new
                {
                    ErrorCode = 403,
                    ErrorMessage = "无权访问"
                };
            }

            Singleton.Instance.setCacheData(userToken, request.Key, request.Value);
            return new
            {
                Result=""
            };
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        /// <returns></returns>
        [Route("refresh"), HttpGet]
        public object CacheRefresh(string key)
        {
            string userToken = ActionContext.Request.Headers.GetValues("userToken").FirstOrDefault();
            if (string.IsNullOrEmpty(userToken))
            {
                return new
                {
                    ErrorCode = 403,
                    ErrorMessage = "无权访问"
                };
            }

            Singleton.Instance.refreshCacheData(userToken,key);
            return new
            {
                Result = ""
            };
        }
    }
}
