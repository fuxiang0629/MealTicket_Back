using Qiniu.Http;
using Qiniu.Storage;
using Qiniu.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FXCommon.FileUpload
{
    public class QiNiuHelper
    {
        private string AccessKey;
        private string SecretKey;
        private string PublicImageBucket;

        public QiNiuHelper()
        {
            AccessKey = ConfigurationManager.AppSettings["qiniu_access_key"];
            SecretKey = ConfigurationManager.AppSettings["qiniu_secret_key"];
            PublicImageBucket = ConfigurationManager.AppSettings["qiniu_bucket_public_image"];
        }

        public string Upload(string fileExtension, byte[] data,string typeKey,out string errorMessage)
        {
            string uploadToken = CreateToken(AccessKey, SecretKey, PublicImageBucket);
            string SupportTypes= ConfigurationManager.AppSettings[typeKey+ "_SupportTypes"];//后缀控制
            string FolderPath = ConfigurationManager.AppSettings[typeKey + "_FolderPath"];//文件夹位置
            if (string.IsNullOrEmpty(SupportTypes) || string.IsNullOrEmpty(FolderPath))
            {
                errorMessage = "不支持的文件name";
                return null;
            }

            //判断后缀名是否支持
            if (!(","+SupportTypes+",").Contains(","+fileExtension+",") && SupportTypes!="*")
            {
                errorMessage = "不支持的文件类型";
                return null;
            }
            //生成文件相对路径
            string fileName=string.Format("{0}_{1}.{2}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), Guid.NewGuid().ToString("N"), fileExtension);
            string fileSavedPath = FolderPath+"/"+ fileName;
            string zone = ConfigurationManager.AppSettings["zone"];
            Config config = new Config
            {
                Zone = zone == "east" ? Zone.ZONE_CN_East : zone == "north" ? Zone.ZONE_CN_North : zone == "south" ? Zone.ZONE_CN_South : Zone.ZONE_US_North
            };
            UploadManager uploadManager = new UploadManager(config);
            var result = uploadManager.UploadData(data, fileSavedPath, uploadToken, null);
            if (result.Code == (int)HttpCode.OK)
            {
                errorMessage = "";
                return fileSavedPath;
            }
            else
            {
                errorMessage = "";
                return null;
            }
        }

        /// <summary>
        /// 创建用于文件上传的token
        /// </summary>
        /// <param name="accessKey">七牛access key</param>
        /// <param name="secretKey">七牛secret key</param>
        /// <param name="bucketName">空间名称</param>
        /// <returns></returns>
        private static string CreateToken(string accessKey, string secretKey, string bucketName)
        {
            PutPolicy putPolicy = new PutPolicy { Scope = bucketName };
            putPolicy.SetExpires(3600);
            Mac mac = new Mac(accessKey, secretKey);
            return Auth.CreateUploadToken(mac, putPolicy.ToJsonString());
        }
    }
}
