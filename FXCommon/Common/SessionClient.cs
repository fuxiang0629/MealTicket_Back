using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FXCommon.Common
{
    public class SessionClient
    {
        private string cacheLoginKey;
        private string cacheUrl;

        public int Connect(string url,string username,string password) 
        {
            cacheUrl = url;
            var request = new
            {
                Username = username,
                Password = password
            };
            string result=HtmlSender.POSTJson(JsonConvert.SerializeObject(request), cacheUrl + "/cache/login");
            var resultObj=JsonConvert.DeserializeObject<dynamic>(result);
            if (resultObj.ErrorCode != null)
            {
                return resultObj.ErrorCode;
            }
            cacheLoginKey = resultObj.Result;
            return 0;
        }

        public T Get<T>(string key,ref int errorCode) 
        {
            if (string.IsNullOrEmpty(cacheLoginKey))
            {
                errorCode = 405;
                return default(T);
            }

            string result = HtmlSender.Get(cacheUrl + "/cache/get?key=" + key, "application/json", Encoding.UTF8, "userToken:" + cacheLoginKey);
            var resultObj = JsonConvert.DeserializeObject<dynamic>(result); 
            if (resultObj.ErrorCode != null)
            {
                errorCode = resultObj.ErrorCode;
                return default(T);
            }
            errorCode = 0;
            result = resultObj.Result;
            return JsonConvert.DeserializeObject<T>(result);
        }

        public int Set<T>(string key,T value)
        {
            if (string.IsNullOrEmpty(cacheLoginKey))
            {
                return 405;
            }

            var request = new
            {
                Key = key,
                Value = JsonConvert.SerializeObject(value)
            };
            string result = HtmlSender.Request(JsonConvert.SerializeObject(request),cacheUrl + "/cache/set", "application/json", Encoding.UTF8, "POST","userToken:" + cacheLoginKey);
            var resultObj = JsonConvert.DeserializeObject<dynamic>(result);
            if (resultObj.ErrorCode != null)
            {
                return resultObj.ErrorCode;
            }
            return 0;
        }

        public int Refresh(string key)
        {
            if (string.IsNullOrEmpty(cacheLoginKey))
            {
                return 405;
            }
            HtmlSender.Get(cacheUrl + "/cache/refresh?key=" + key, "application/json", Encoding.UTF8, "userToken:" + cacheLoginKey);
            return 0; ;
        }
    }
}
