using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{

    [Serializable]
    public class AuthorizationException : Exception
    {
        /// <summary>
        /// 业务错误码
        /// </summary>
        public int errorCode { get; set; }

        /// <summary>
        /// 使用指定的错误信息初始化
        /// </summary>
        public AuthorizationException(int _errorCode,string _errorMessage)
            : base(_errorMessage)
        {
            errorCode = _errorCode;
        }
    }
}
