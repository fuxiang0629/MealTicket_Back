using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyServer
    {
    }

    public class ModifyServerRequest
    {
        /// <summary>
        /// 【必填】服务器代号
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// 【选填】描述
        /// </summary>
        public string ServerDes { get; set; }
    }
}
