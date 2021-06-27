using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FXCommon.Common
{
    /// <summary>
    /// Html发送器
    /// </summary>
    public static class HtmlSender
    {
        /// <summary>
        /// xml请求类型
        /// </summary>
        const string Xml = "application/xml";

        /// <summary>
        /// json请求类型
        /// </summary>
        const string Json = "application/json";

        /// <summary>
        /// POST请求
        /// </summary>
        const string POST = "POST";

        /// <summary>
        /// Get请求
        /// </summary>
        const string GET = "GET";

        /// <summary>
        /// POST xml请求
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="url">发送地址</param>
        /// <returns>响应内容</returns>
        public static string POSTXml(string content, string url)
        {
            return Post(content, url, Xml);
        }

        /// <summary>
        /// Post json请求
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="url">地址</param>
        /// <returns>响应内容</returns>
        public static string POSTJson(string content, string url)
        {
            return Post(content, url, Json);
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="content">发送内容</param>
        /// <param name="url">地址</param>
        /// <param name="type">contenttype数据类型</param>
        /// <returns>响应内容</returns>
        public static string Post(string content, string url, string type)
        {
            return Request(content, url, type, Encoding.UTF8, POST);
        }

        /// <summary>
        /// Web请求
        /// </summary>
        /// <param name="content">发送内容</param>
        /// <param name="url">地址</param>
        /// <param name="type">contenttype数据类型</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="methord">HTTP动词</param>
        /// <returns>响应内容</returns>
        public static string Request(string content, string url, string type, Encoding encoding, string methord)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            byte[] buf = UnicodeEncoding.UTF8.GetBytes(content);
            myRequest.Method = methord;
            myRequest.ContentLength = buf.Length;
            myRequest.ContentType = type;
            using (Stream newStream = myRequest.GetRequestStream())
            {
                newStream.Write(buf, 0, buf.Length);
            }
            using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
            using (StreamReader reader = new StreamReader(myResponse.GetResponseStream(), encoding))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Web请求(Http Basic认证)
        /// </summary>
        /// <returns></returns>
        public static string Post_BasicAuth(string content, string url, string auth)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            byte[] buf = UnicodeEncoding.UTF8.GetBytes(content);
            myRequest.Method = "POST";
            myRequest.ContentLength = buf.Length;
            myRequest.ContentType = "application/json";
            myRequest.Headers.Add("Authorization", "Basic " + auth);
            using (Stream newStream = myRequest.GetRequestStream())
            {
                newStream.Write(buf, 0, buf.Length);
            }
            using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
            using (StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Get xml方法
        /// </summary>
        /// <param name="url">地址</param>
        /// <returns>响应内容</returns>
        public static string GetXml(string url)
        {
            return Get(url, Xml, Encoding.UTF8);
        }

        /// <summary>
        /// Get json方法
        /// </summary>
        /// <param name="url">地址</param>
        /// <returns>响应内容</returns>
        public static string GetJson(string url)
        {
            return Get(url, Json, Encoding.UTF8);
        }


        /// <summary>
        /// Get方法
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="contentType">contenttype数据类型</param>
        /// <returns>响应内容</returns>
        public static string Get(string url, string contentType)
        {
            return Get(url, contentType, Encoding.UTF8);
        }

        /// <summary>
        /// Get方法
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="contentType">contenttype数据类型</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>响应内容</returns>
        public static string Get(string url, string contentType, Encoding encoding,string header="")
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.Method = "GET";
            myRequest.ContentType = contentType;
            if (!string.IsNullOrEmpty(header))
            {
                myRequest.Headers.Add(header);
            }
            using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
            using (StreamReader reader = new StreamReader(myResponse.GetResponseStream(), encoding))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Get方法
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="contentType">contenttype数据类型</param>
        /// <returns>响应内容</returns>
        public static byte[] GetStream(string url, string contentType)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.Method = "GET";
            myRequest.ContentType = contentType;
            using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
            using (var stream = myResponse.GetResponseStream())
            {
                var list = new List<byte[]>();
                var length = 0;
                while (true)
                {
                    byte[] temp = new byte[10240];
                    var part = stream.Read(temp, 0, temp.Length);
                    length += part;
                    if (part < 10240)
                    {
                        var last = new byte[part];
                        Buffer.BlockCopy(temp, 0, last, 0, part);
                        list.Add(last);
                        break;
                    }
                    list.Add(temp);
                }
                var result = new byte[length];
                int start = 0;
                list.ForEach((e) =>
                {
                    Buffer.BlockCopy(e, 0, result, start, e.Length);
                    start += e.Length;
                });
                return result;
            }
        }
    }
}
