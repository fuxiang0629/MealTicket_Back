using FXCommon.Common;
using FXCommon.FileUpload;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// UploadHandler 的摘要说明
/// </summary>
public class UploadHandler : Handler {

    HttpContent content;
    public UploadConfig UploadConfig { get; private set; }
    public UploadResult Result { get; private set; }

    public UploadHandler(HttpContent _content, UploadConfig config){
        this.UploadConfig = config;
        this.Result = new UploadResult() { State = UploadState.Unknown };
        this.content = _content;
    }

    public override async Task<string> Process()
    {
        if (!content.IsMimeMultipartContent())
        {
            Result.State = UploadState.NetworkError2;
            return WriteResult();
        }
        // 采用MultipartMemoryStreamProvider  
        var provider = new MultipartMemoryStreamProvider();
        // 读取文件数据  
        await content.ReadAsMultipartAsync(provider);

        string path = "";
        string filename = "";
        string fileMd5 = "";
        if (provider.Contents.Count() > 0)
        {
            QiNiuHelper qiNiu = new QiNiuHelper();
            //获取首个文件
            var file = provider.Contents[0];
            filename = file.Headers.ContentDisposition.FileName.Trim('"');
            string name = "Ueditor";
            //获取对应文件后缀名
            string fileExt = filename.Split('.')[1];
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
                    Result.State = UploadState.NetworkError3;
                    return WriteResult();
                }
                fileMd5 = data.ToMD5();


                Result.State = UploadState.Success;
                Result.Url = string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], path);
                return WriteResult();
            }
        }
        else
        {
            Result.State = UploadState.NetworkError;
            return WriteResult();
        }
    }

    private string WriteResult()
    {
        var temp = new
        {
            state = GetStateMessage(Result.State),
            url = Result.Url,
            title = Result.OriginFileName,
            original = Result.OriginFileName,
            error = Result.ErrorMessage
        };
        return JsonConvert.SerializeObject(temp);
    }

    private string GetStateMessage(UploadState state) {
        switch (state) {
            case UploadState.Success:
                return "SUCCESS";
            case UploadState.FileAccessError:
                return "文件访问出错，请检查写入权限";
            case UploadState.SizeLimitExceed:
                return "文件大小超出服务器限制";
            case UploadState.TypeNotAllow:
                return "不允许的文件格式";
            case UploadState.NetworkError:
                return "网络错误";
        }
        return "未知错误";
    }

    private bool CheckFileType(string filename) {
        var fileExtension = Path.GetExtension(filename).ToLower();
        return UploadConfig.AllowExtensions.Select(x => x.ToLower()).Contains(fileExtension);
    }

    private bool CheckFileSize(int size) {
        return size < UploadConfig.SizeLimit;
    }
}

public class UploadConfig {
    /// <summary>
    /// 文件命名规则
    /// </summary>
    public string PathFormat { get; set; }

    /// <summary>
    /// 上传表单域名称
    /// </summary>
    public string UploadFieldName { get; set; }

    /// <summary>
    /// 上传大小限制
    /// </summary>
    public int SizeLimit { get; set; }

    /// <summary>
    /// 上传允许的文件格式
    /// </summary>
    public string[] AllowExtensions { get; set; }

    /// <summary>
    /// 文件是否以 Base64 的形式上传
    /// </summary>
    public bool Base64 { get; set; }

    /// <summary>
    /// Base64 字符串所表示的文件名
    /// </summary>
    public string Base64Filename { get; set; }
}

public class UploadResult {
    public UploadState State { get; set; }
    public string Url { get; set; }
    public string OriginFileName { get; set; }
    public string ErrorMessage { get; set; }
}

public enum UploadState {
    Success = 0,
    SizeLimitExceed = -1,
    TypeNotAllow = -2,
    FileAccessError = -3,
    NetworkError = -4,
    NetworkError2 = -5,
    NetworkError3 = -6,
    NetworkError4 = -7,
    NetworkError5 = -8,
    NetworkError6 = -9,
    Unknown = 1,
}

