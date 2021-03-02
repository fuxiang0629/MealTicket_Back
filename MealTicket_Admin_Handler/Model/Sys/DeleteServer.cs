using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class DeleteServer
    {
    }

    public class DeleteServerRequest
    {
        /// <summary>
        /// 服务器代码
        /// </summary>
        public string ServerId { get; set; }
    }
}
