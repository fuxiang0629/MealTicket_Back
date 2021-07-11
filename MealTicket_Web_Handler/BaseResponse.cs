using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    /// <summary>
    /// 基础返回数据
    /// </summary>
    public class BaseResponse
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// 错误描述
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 是否压缩
        /// </summary>
        public bool IsZip { get; set; }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime DataTime
        {
            get
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// 接口返回业务数据
        /// </summary>
        public object Data { get; set; }
    }
}
