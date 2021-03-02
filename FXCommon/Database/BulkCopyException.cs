using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FXCommon.Database
{
    /// <summary>
    /// 批量导入异常
    /// </summary>
    [Serializable]
    public class BulkCopyException : Exception
    {
        /// <summary>
        /// 默认初始化
        /// </summary>
        public BulkCopyException() : base() { }

        /// <summary>
        /// 使用指定的错误信息初始化
        /// </summary>
        /// <param name="message">错误消息</param>
        public BulkCopyException(string message) : base(message) { }

        /// <summary>
        /// 用序列化数据初始化
        /// </summary>
        /// <param name="info">存有有关所引发异常的序列化的对象数据</param>
        /// <param name="context">包含有关源或目标的上下文信息</param>
        public BulkCopyException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        /// <summary>
        /// 使用指定错误信息和对作为此异常原因的内部异常的引用来初始化
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="exception">导致当前异常的异常</param>
        public BulkCopyException(string message, Exception exception)
            : base(message, exception) { }
    }
}
