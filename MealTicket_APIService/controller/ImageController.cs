using FXCommon.Common;
using FXCommon.FileUpload;
using MealTicket_APIService.Filter;
using MealTicket_Handler.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Rocket_Client_API.controller
{
    [RoutePrefix("img")]
    public class ImageController:ApiController
    {
        /// <summary>
        /// 图片上传接口
        /// </summary>
        /// <returns></returns>
        [Description("图片上传接口")]
        [CheckUserLoginFilter]
        [Route("upload"), HttpPost]
        public async Task<object> UploadImage()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new WebApiException(400, "请求媒体参数不正确，请确保使用的是multipart/form-data方式");
            }
            // 采用MultipartMemoryStreamProvider  
            var provider = new MultipartMemoryStreamProvider();
            // 读取文件数据  
            await Request.Content.ReadAsMultipartAsync(provider);

            string path = "";
            string filename = "";
            object DataInfo = null;
            if (provider.Contents.Count() > 0)
            {
                QiNiuHelper qiNiu = new QiNiuHelper();
                //获取首个文件
                var file = provider.Contents[0];
                Logger.WriteFileLog("fileName:"+file.Headers.ContentDisposition.FileName, null);
                Logger.WriteFileLog("Name:" + file.Headers.ContentDisposition.Name, null);
                filename = file.Headers.ContentDisposition.FileName.Trim('"');
                string name = file.Headers.ContentDisposition.Name.Trim('"');
                //获取对应文件后缀名
                string fileExt = System.IO.Path.GetExtension(filename);
                fileExt = fileExt.Replace(".", "");
                // 获取到流  
                var ms = file.ReadAsStreamAsync().Result;
                // 进行流操作  
                using (var br = new BinaryReader(ms))
                {
                    // 读取文件内容到内存中  
                    var data = br.ReadBytes((int)ms.Length);
                    if (name == "RealNameF" || name == "RealNameB")//身份证人头面上传
                    {
                        string id_card_side = name == "RealNameF" ? "front" : "back";

                        try
                        {
                            //识别文字
                            string uri = string.Format("https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id={0}&client_secret={1}", ConfigurationManager.AppSettings["baiduAppKey"], ConfigurationManager.AppSettings["baiduSecretKey"]);
                            string res = HtmlSender.GetJson(uri);
                            var resJson = JsonConvert.DeserializeObject<dynamic>(res);
                            string access_token = resJson.access_token;

                            if (string.IsNullOrEmpty(access_token))
                            {
                                throw new Exception();
                            }
                            uri = string.Format("https://aip.baidubce.com/rest/2.0/ocr/v1/idcard?access_token={0}", access_token);
                            string content = string.Format("id_card_side={0}&image={1}", id_card_side, HttpUtility.UrlEncode(Convert.ToBase64String(data)));
                            res = HtmlSender.Request(content, uri, "application/x-www-form-urlencoded", Encoding.UTF8, "POST");
                            resJson = JsonConvert.DeserializeObject<dynamic>(res);
                            if (resJson != null && resJson.words_result != null)
                            {
                                if (name == "RealNameF")
                                {
                                    string RealName = resJson.words_result.姓名.words;
                                    if (string.IsNullOrEmpty(RealName))
                                    {
                                        throw new Exception();
                                    }
                                    DataInfo = new
                                    {
                                        RealName = RealName,
                                        Sex = resJson.words_result.性别.words == "男" ? 1 : resJson.words_result.性别.words == "女" ? 2 : 0,
                                        BirthDay = resJson.words_result.出生.words,
                                        CardNo = resJson.words_result.公民身份号码.words,
                                        Address = resJson.words_result.住址.words
                                    };
                                }
                                else
                                {
                                    string CheckOrg = resJson.words_result.签发机关.words;
                                    if (string.IsNullOrEmpty(CheckOrg))
                                    {
                                        throw new Exception();
                                    }
                                    DataInfo = new
                                    {
                                        CheckOrg = CheckOrg,
                                        ValidDateFrom = resJson.words_result.签发日期.words,
                                        ValidDateTo = resJson.words_result.失效日期.words
                                    };
                                }
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new WebApiException(400, "无法识别身份证图片");
                        }
                    }
                    string errorMessage;
                    path = qiNiu.Upload(fileExt, data, name, out errorMessage);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        throw new WebApiException(400, errorMessage);
                    }
                }
            }
            else
            {
                throw new WebApiException(400, "上传图片内容不能为空");
            }
            return new
            {
                ImgName=filename,
                ImgPath = path,
                ImgUrl = string.Format("{0}/{1}",ConfigurationManager.AppSettings["imgurl"], path),
                DataInfo= DataInfo
            };
        }
    }
}
