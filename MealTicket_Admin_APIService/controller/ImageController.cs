using FXCommon.Common;
using FXCommon.FileUpload;
using MealTicket_Admin_APIService.Filter;
using MealTicket_Admin_Handler.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MealTicket_Admin_APIService.controller
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
        public async Task<ImagePath> UploadImage()
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
            if (provider.Contents.Count() > 0)
            {
                QiNiuHelper qiNiu = new QiNiuHelper();
                //获取首个文件
                var file = provider.Contents[0];
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
            return new ImagePath
            {
                ImgName=filename,
                ImgPath = path,
                ImgUrl = string.Format("{0}/{1}",ConfigurationManager.AppSettings["imgurl"], path)
            };
        }
    }
}
