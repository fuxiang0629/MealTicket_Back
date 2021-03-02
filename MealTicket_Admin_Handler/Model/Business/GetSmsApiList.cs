using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSmsApiList
    {
    }

    public class NoticeApiInfo 
    {
        /// <summary>
        /// api地址
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// Api描述
        /// </summary>
        public string ApiDes { get; set; }

        /// <summary>
        /// Api参数
        /// </summary>
        public string ApiPara { get; set; }
    }
}
