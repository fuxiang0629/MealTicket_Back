using System;
using System.IO;
using System.Text;

namespace FXCommon.Common
{
    /// <summary>
    /// 错误日志
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// 同步锁
        /// </summary>
        private static object Lock = new object();

        /// <summary>
        /// 记录文件日志
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="ex">异常</param>
        public static void WriteFileLog(string message, Exception ex)
        {
            try
            {
                StringBuilder builder = new StringBuilder(200);
                builder.AppendLine(DateTime.Now.ToString());
                builder.AppendLine(message);
                builder.AppendLine(ex == null ? string.Empty :
                                    (ex.InnerException == null ? ex.ToString() :
                                    (ex.InnerException.InnerException == null ?
                                        ex.InnerException.ToString() : ex.InnerException.InnerException.ToString())));
                builder.AppendLine();
                WriteFileLog(builder.ToString());
            }
            catch { }
        }

        /// <summary>
        /// 记录文件日志
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="filePath">特定地址</param>
        /// <param name="ex">异常</param>
        public static void WriteFileLog(string message,string filePath, Exception ex)
        {
            try
            {
                StringBuilder builder = new StringBuilder(200);
                builder.AppendLine("=============="+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+ "==============");
                builder.AppendLine(message);
                builder.AppendLine(ex == null ? string.Empty :
                                    (ex.InnerException == null ? ex.ToString() :
                                    (ex.InnerException.InnerException == null ?
                                        ex.InnerException.ToString() : ex.InnerException.InnerException.ToString())));
                builder.AppendLine("===============================================");
                builder.AppendLine();
                WriteFileLog(builder.ToString(), filePath);
            }
            catch { }
        }

        /// <summary>
        /// 记录文件日志
        /// </summary>
        /// <param name="message">错误信息</param>
        private static void WriteFileLog(string message, string filePath="Log")
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                lock (Lock)
                {
                    File.AppendAllText(Path.Combine(path, DateTime.Now.ToString("yyyyMMdd") + ".log"), message);
                }
            }
            catch { }
        }
    }
}
