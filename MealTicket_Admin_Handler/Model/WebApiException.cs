using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    /// <summary>
    /// Web API相关异常
    /// </summary>
    [Serializable]
    public class WebApiException : Exception
    {
        /// <summary>
        /// 业务错误码
        /// </summary>
        public int errorCode { get; set; }

        /// <summary>
        /// 使用指定的错误信息初始化
        /// </summary>
        public WebApiException(int _errorCode, string message)
            : base(message)
        {
            errorCode = _errorCode;
        }
    }
}
