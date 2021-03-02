using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace MealTicket_Admin_APIService.controller
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [OverrideAuthorization]
    [OverrideActionFilters]
    [RoutePrefix("urditor")]
    public class UeditorController:ApiController
    {
        [Route("process"), HttpPost,HttpGet]
        public HttpResponseMessage ProcessRequest(string action)
        {
            Handler handler = null;
            switch (action)
            {
                case "config":
                    handler = new ConfigHandler();
                    break;
                case "uploadimage":
                    handler = new UploadHandler(Request.Content, new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("imageAllowFiles"),
                        PathFormat = Config.GetString("imagePathFormat"),
                        SizeLimit = Config.GetInt("imageMaxSize"),
                        UploadFieldName = Config.GetString("imageFieldName")
                    });
                    break;
                case "uploadscrawl":
                    handler = new UploadHandler(Request.Content, new UploadConfig()
                    {
                        AllowExtensions = new string[] { ".png" },
                        PathFormat = Config.GetString("scrawlPathFormat"),
                        SizeLimit = Config.GetInt("scrawlMaxSize"),
                        UploadFieldName = Config.GetString("scrawlFieldName"),
                        Base64 = true,
                        Base64Filename = "scrawl.png"
                    });
                    break;
                case "uploadvideo":
                    handler = new UploadHandler(Request.Content, new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("videoAllowFiles"),
                        PathFormat = Config.GetString("videoPathFormat"),
                        SizeLimit = Config.GetInt("videoMaxSize"),
                        UploadFieldName = Config.GetString("videoFieldName")
                    });
                    break;
                case "uploadfile":
                    handler = new UploadHandler(Request.Content, new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("fileAllowFiles"),
                        PathFormat = Config.GetString("filePathFormat"),
                        SizeLimit = Config.GetInt("fileMaxSize"),
                        UploadFieldName = Config.GetString("fileFieldName")
                    });
                    break;
                default:
                    handler = new NotSupportedHandler();
                    break;
            }
            string res= handler.Process().Result;

            return new HttpResponseMessage
            {
                Content = new StringContent(res)
            };
        }
    }
}
